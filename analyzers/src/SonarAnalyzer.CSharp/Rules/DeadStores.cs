/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2021 SonarSource SA
 * mailto: contact AT sonarsource DOT com
 *
 * This program is free software; you can redistribute it and/or
 * modify it under the terms of the GNU Lesser General Public
 * License as published by the Free Software Foundation; either
 * version 3 of the License, or (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
 * Lesser General Public License for more details.
 *
 * You should have received a copy of the GNU Lesser General Public License
 * along with this program; if not, write to the Free Software Foundation,
 * Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02110-1301, USA.
 */

using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;
using SonarAnalyzer.Common;
using SonarAnalyzer.ControlFlowGraph;
using SonarAnalyzer.ControlFlowGraph.CSharp;
using SonarAnalyzer.Extensions;
using SonarAnalyzer.Helpers;
using SonarAnalyzer.LiveVariableAnalysis.CSharp;
using StyleCop.Analyzers.Lightup;

namespace SonarAnalyzer.Rules.CSharp
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    [Rule(DiagnosticId)]
    public sealed class DeadStores : SonarDiagnosticAnalyzer
    {
        private const string DiagnosticId = "S1854";
        private const string MessageFormat = "Remove this useless assignment to local variable '{0}'.";

        private static readonly DiagnosticDescriptor Rule = DiagnosticDescriptorBuilder.GetDescriptor(DiagnosticId, MessageFormat, RspecStrings.ResourceManager);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(Rule);

        protected override void Initialize(SonarAnalysisContext context)
        {
            // No need to check for ExpressionBody as it can't contain variable assignment
            context.RegisterSyntaxNodeActionInNonGenerated(
                c => CheckForDeadStores(c, c.SemanticModel.GetDeclaredSymbol(c.Node), ((BaseMethodDeclarationSyntax)c.Node).Body),
                SyntaxKind.MethodDeclaration,
                SyntaxKind.ConstructorDeclaration,
                SyntaxKind.DestructorDeclaration,
                SyntaxKind.ConversionOperatorDeclaration,
                SyntaxKind.OperatorDeclaration);

            context.RegisterSyntaxNodeActionInNonGenerated(
                c => CheckForDeadStores(c, c.SemanticModel.GetDeclaredSymbol(c.Node), ((AccessorDeclarationSyntax)c.Node).Body),
                SyntaxKind.GetAccessorDeclaration,
                SyntaxKind.SetAccessorDeclaration,
                SyntaxKindEx.InitAccessorDeclaration,
                SyntaxKind.AddAccessorDeclaration,
                SyntaxKind.RemoveAccessorDeclaration);

            context.RegisterSyntaxNodeActionInNonGenerated(
                c => CheckForDeadStores(c, c.SemanticModel.GetSymbolInfo(c.Node).Symbol, ((AnonymousFunctionExpressionSyntax)c.Node).Body),
                SyntaxKind.AnonymousMethodExpression,
                SyntaxKind.SimpleLambdaExpression,
                SyntaxKind.ParenthesizedLambdaExpression);

            context.RegisterSyntaxNodeActionInNonGenerated(
                c => CheckForDeadStores(c, c.SemanticModel.GetDeclaredSymbol(c.Node), ((LocalFunctionStatementSyntaxWrapper)c.Node).Body),
                SyntaxKindEx.LocalFunctionStatement);
        }

        private static void CheckForDeadStores(SyntaxNodeAnalysisContext context, ISymbol symbol, CSharpSyntaxNode node)
        {
            if (symbol != null
                && node != null
                // Currently the tuple expressions are not supported and this is known to cause false positives.
                // Please check:
                // - community feedback: https://github.com/SonarSource/sonar-dotnet/issues/3094
                // - implementation ticket: https://github.com/SonarSource/sonar-dotnet/issues/2933
                && !node.DescendantNodes().AnyOfKind(SyntaxKindEx.TupleExpression)
                && CSharpControlFlowGraph.TryGet(node, context.SemanticModel, out var cfg))
            {
                var lva = CSharpLiveVariableAnalysis.Analyze(cfg, symbol, context.SemanticModel);
                foreach (var block in cfg.Blocks)
                {
                    var blockLva = new InBlockLivenessAnalysis(context, symbol, node, block, lva.GetLiveOut(block), lva.CapturedVariables);
                    blockLva.Analyze();
                }
            }
        }

        private class InBlockLivenessAnalysis
        {
            private readonly SyntaxNodeAnalysisContext context;
            private readonly ISymbol nodeSymbol;
            private readonly SyntaxNode node;
            private readonly Block block;
            private readonly IEnumerable<ISymbol> blockLiveOut;
            private readonly IEnumerable<ISymbol> excludedLocals;

            private static readonly ISet<string> AllowedNumericValues = new HashSet<string> { "-1", "0", "1" };
            private static readonly ISet<string> AllowedStringValues = new HashSet<string> { string.Empty };

            public InBlockLivenessAnalysis(SyntaxNodeAnalysisContext context, ISymbol nodeSymbol, SyntaxNode node, Block block, IEnumerable<ISymbol> blockLiveOut, IEnumerable<ISymbol> excludedLocals)
            {
                this.context = context;
                this.nodeSymbol = nodeSymbol;
                this.node = node;
                this.block = block;
                this.blockLiveOut = blockLiveOut;
                this.excludedLocals = excludedLocals;
            }

            public void Analyze()
            {
                var assignmentLhs = new HashSet<SyntaxNode>();
                var liveOut = new HashSet<ISymbol>(blockLiveOut);

                foreach (var instruction in block.Instructions.Reverse())
                {
                    switch (instruction.Kind())
                    {
                        case SyntaxKind.IdentifierName:
                            ProcessIdentifier(instruction, assignmentLhs, liveOut);
                            break;

                        case SyntaxKind.AddAssignmentExpression:
                        case SyntaxKind.SubtractAssignmentExpression:
                        case SyntaxKind.MultiplyAssignmentExpression:
                        case SyntaxKind.DivideAssignmentExpression:
                        case SyntaxKind.ModuloAssignmentExpression:
                        case SyntaxKind.AndAssignmentExpression:
                        case SyntaxKind.ExclusiveOrAssignmentExpression:
                        case SyntaxKind.OrAssignmentExpression:
                        case SyntaxKind.LeftShiftAssignmentExpression:
                        case SyntaxKind.RightShiftAssignmentExpression:
                        case SyntaxKindEx.CoalesceAssignmentExpression:
                            ProcessOpAssignment(instruction, assignmentLhs, liveOut);
                            break;

                        case SyntaxKind.SimpleAssignmentExpression:
                            ProcessSimpleAssignment(instruction, assignmentLhs, liveOut);
                            break;

                        case SyntaxKind.VariableDeclarator:
                            ProcessVariableDeclarator(instruction, liveOut);
                            break;

                        case SyntaxKind.PreIncrementExpression:
                        case SyntaxKind.PreDecrementExpression:
                            ProcessPrefixExpression(instruction, liveOut);
                            break;

                        case SyntaxKind.PostIncrementExpression:
                        case SyntaxKind.PostDecrementExpression:
                            ProcessPostfixExpression(instruction, liveOut);
                            break;
                    }
                }
            }

            private void ProcessIdentifier(SyntaxNode instruction, HashSet<SyntaxNode> assignmentLhs, HashSet<ISymbol> liveOut)
            {
                var identifier = (IdentifierNameSyntax)instruction;
                var symbol = context.SemanticModel.GetSymbolInfo(identifier).Symbol;
                if (IsSymbolRelevant(symbol)
                    && !identifier.GetSelfOrTopParenthesizedExpression().IsInNameOfArgument(context.SemanticModel)
                    && CSharpLiveVariableAnalysis.IsLocalScoped(symbol, nodeSymbol))
                {
                    if (CSharpLiveVariableAnalysis.IsOutArgument(identifier))
                    {
                        liveOut.Remove(symbol);
                    }
                    else if (!assignmentLhs.Contains(identifier))
                    {
                        liveOut.Add(symbol);
                    }
                }
            }

            private void ProcessOpAssignment(SyntaxNode instruction, HashSet<SyntaxNode> assignmentLhs, HashSet<ISymbol> liveOut)
            {
                var assignment = (AssignmentExpressionSyntax)instruction;
                var left = assignment.Left.RemoveParentheses();
                if (IdentifierRelevantSymbol(left) is { } symbol)
                {
                    ReportOnAssignment(assignment, left, symbol, assignmentLhs, liveOut);
                }
            }

            private void ProcessSimpleAssignment(SyntaxNode instruction, HashSet<SyntaxNode> assignmentLhs, HashSet<ISymbol> liveOut)
            {
                var assignment = (AssignmentExpressionSyntax)instruction;
                var left = assignment.Left.RemoveParentheses();
                if (IdentifierRelevantSymbol(left) is { } symbol)
                {
                    ReportOnAssignment(assignment, left, symbol, assignmentLhs, liveOut);
                    liveOut.Remove(symbol);
                }
            }

            private void ProcessVariableDeclarator(SyntaxNode instruction, HashSet<ISymbol> liveOut)
            {
                var declarator = (VariableDeclaratorSyntax)instruction;
                if (context.SemanticModel.GetDeclaredSymbol(declarator) is ILocalSymbol symbol && IsSymbolRelevant(symbol))
                {
                    if (declarator.Initializer != null
                        && !IsAllowedInitialization(declarator.Initializer)
                        && !symbol.IsConst
                        && symbol.RefKind() == RefKind.None
                        && !liveOut.Contains(symbol)
                        && !IsUnusedLocal(symbol)
                        && !new MutedSyntaxWalker(context.SemanticModel, declarator, symbol).IsMuted())
                    {
                        var location = GetFirstLineLocationFromToken(declarator.Initializer.EqualsToken, declarator.Initializer);
                        context.ReportDiagnosticWhenActive(Diagnostic.Create(Rule, location, symbol.Name));
                    }
                    liveOut.Remove(symbol);
                }
            }

            private bool IsAllowedInitialization(EqualsValueClauseSyntax initializer) =>
                initializer.Value.IsKind(SyntaxKind.DefaultExpression)
                || IsAllowedObjectInitialization(initializer.Value)
                || IsAllowedBooleanInitialization(initializer.Value)
                || IsAllowedNumericInitialization(initializer.Value)
                || IsAllowedUnaryNumericInitialization(initializer.Value)
                || IsAllowedStringInitialization(initializer.Value);

            private static bool IsAllowedObjectInitialization(ExpressionSyntax expression) =>
                expression.IsNullLiteral();

            private static bool IsAllowedBooleanInitialization(ExpressionSyntax expression) =>
                expression.IsAnyKind(SyntaxKind.TrueLiteralExpression, SyntaxKind.FalseLiteralExpression);

            private static bool IsAllowedNumericInitialization(ExpressionSyntax expression) =>
                expression.IsKind(SyntaxKind.NumericLiteralExpression) && AllowedNumericValues.Contains(((LiteralExpressionSyntax)expression).Token.ValueText);  // -1, 0 or 1

            private static bool IsAllowedUnaryNumericInitialization(ExpressionSyntax expression) =>
                expression.IsAnyKind(SyntaxKind.UnaryPlusExpression, SyntaxKind.UnaryMinusExpression) && IsAllowedNumericInitialization(((PrefixUnaryExpressionSyntax)expression).Operand);

            private bool IsAllowedStringInitialization(ExpressionSyntax expression) =>
                (expression.IsKind(SyntaxKind.StringLiteralExpression) && AllowedStringValues.Contains(((LiteralExpressionSyntax)expression).Token.ValueText))
                || expression.IsStringEmpty(context.SemanticModel);

            private bool IsUnusedLocal(ISymbol declaredSymbol) =>
                node.DescendantNodes()
                    .OfType<IdentifierNameSyntax>()
                    .SelectMany(x => VariableUnusedBase.GetUsedSymbols(x, context.SemanticModel))
                    .All(x => !x.Equals(declaredSymbol));

            private void ProcessPrefixExpression(SyntaxNode instruction, HashSet<ISymbol> liveOut)
            {
                var prefixExpression = (PrefixUnaryExpressionSyntax)instruction;
                var parent = prefixExpression.GetSelfOrTopParenthesizedExpression();
                var operand = prefixExpression.Operand.RemoveParentheses();
                if (parent.Parent is ExpressionStatementSyntax
                    && IdentifierRelevantSymbol(operand) is { } symbol
                    && CSharpLiveVariableAnalysis.IsLocalScoped(symbol, nodeSymbol)
                    && !liveOut.Contains(symbol)
                    && !IsMuted(operand))
                {
                    context.ReportDiagnosticWhenActive(Diagnostic.Create(Rule, prefixExpression.GetLocation(), symbol.Name));
                }
            }

            private void ProcessPostfixExpression(SyntaxNode instruction, HashSet<ISymbol> liveOut)
            {
                var postfixExpression = (PostfixUnaryExpressionSyntax)instruction;
                var operand = postfixExpression.Operand.RemoveParentheses();
                if (IdentifierRelevantSymbol(operand) is { } symbol
                    && CSharpLiveVariableAnalysis.IsLocalScoped(symbol, nodeSymbol)
                    && !liveOut.Contains(symbol)
                    && !IsMuted(operand))
                {
                    context.ReportDiagnosticWhenActive(Diagnostic.Create(Rule, postfixExpression.GetLocation(), symbol.Name));
                }
            }

            private void ReportOnAssignment(AssignmentExpressionSyntax assignment, ExpressionSyntax left, ISymbol symbol, HashSet<SyntaxNode> assignmentLhs, HashSet<ISymbol> outState)
            {
                if (CSharpLiveVariableAnalysis.IsLocalScoped(symbol, nodeSymbol)
                    && !outState.Contains(symbol)
                    && !IsMuted(left))
                {
                    var location = GetFirstLineLocationFromToken(assignment.OperatorToken, assignment.Right);
                    context.ReportDiagnosticWhenActive(Diagnostic.Create(Rule, location, symbol.Name));
                }

                assignmentLhs.Add(left);
            }

            private bool IsMuted(SyntaxNode node) =>
                new MutedSyntaxWalker(context.SemanticModel, node).IsMuted();

            private static Location GetFirstLineLocationFromToken(SyntaxToken issueStartToken, SyntaxNode wholeIssue)
            {
                var line = wholeIssue.SyntaxTree.GetText().Lines[issueStartToken.GetLocation().GetLineSpan().StartLinePosition.Line];
                var rightSingleLine = line.Span.Intersection(TextSpan.FromBounds(issueStartToken.SpanStart, wholeIssue.Span.End));
                return Location.Create(wholeIssue.SyntaxTree, TextSpan.FromBounds(issueStartToken.SpanStart, rightSingleLine.HasValue ? rightSingleLine.Value.End : issueStartToken.Span.End));
            }

            private ISymbol IdentifierRelevantSymbol(SyntaxNode node) =>
                node.IsKind(SyntaxKind.IdentifierName)
                && context.SemanticModel.GetSymbolInfo(node).Symbol is { } symbol
                && IsSymbolRelevant(symbol)
                ? symbol
                : null;

            private bool IsSymbolRelevant(ISymbol symbol) =>
                symbol != null && !excludedLocals.Contains(symbol);
        }
    }
}
