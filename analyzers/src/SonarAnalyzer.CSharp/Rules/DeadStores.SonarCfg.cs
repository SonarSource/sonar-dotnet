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
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;
using SonarAnalyzer.CFG.Sonar;
using SonarAnalyzer.Extensions;
using SonarAnalyzer.Helpers;
using SonarAnalyzer.LiveVariableAnalysis.CSharp;
using StyleCop.Analyzers.Lightup;

namespace SonarAnalyzer.Rules.CSharp
{
    public partial class DeadStores : SonarDiagnosticAnalyzer
    {
        private class SonarChecker : CheckerBase<IControlFlowGraph, Block>
        {
            private readonly SyntaxNode node;

            public SonarChecker(SyntaxNodeAnalysisContext context, SonarCSharpLiveVariableAnalysis lva, SyntaxNode node) : base(context, lva) =>
                this.node = node;

            protected override State CreateState(Block block) =>
                new SonarState(this, block, node);

            private class SonarState : State
            {
                private readonly ISet<SyntaxNode> assignmentLhs = new HashSet<SyntaxNode>();
                private readonly SyntaxNode node;

                public SonarState(SonarChecker owner, Block block, SyntaxNode node) : base(owner, block) =>
                    this.node = node;

                public override void AnalyzeBlock()
                {
                    foreach (var instruction in block.Instructions.Reverse())
                    {
                        switch (instruction.Kind())
                        {
                            case SyntaxKind.IdentifierName:
                                ProcessIdentifier(instruction);
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
                                ProcessOpAssignment(instruction);
                                break;

                            case SyntaxKind.SimpleAssignmentExpression:
                                ProcessSimpleAssignment(instruction);
                                break;

                            case SyntaxKind.VariableDeclarator:
                                ProcessVariableDeclarator(instruction);
                                break;

                            case SyntaxKind.PreIncrementExpression:
                            case SyntaxKind.PreDecrementExpression:
                                ProcessPrefixExpression(instruction);
                                break;

                            case SyntaxKind.PostIncrementExpression:
                            case SyntaxKind.PostDecrementExpression:
                                ProcessPostfixExpression(instruction);
                                break;
                        }
                    }
                }

                private void ProcessIdentifier(SyntaxNode instruction)
                {
                    var identifier = (IdentifierNameSyntax)instruction;
                    var symbol = SemanticModel.GetSymbolInfo(identifier).Symbol;
                    if (IsSymbolRelevant(symbol)
                        && !identifier.GetSelfOrTopParenthesizedExpression().IsInNameOfArgument(SemanticModel)
                        && IsLocal(symbol))
                    {
                        if (SonarCSharpLiveVariableAnalysis.IsOutArgument(identifier))
                        {
                            liveOut.Remove(symbol);
                        }
                        else if (!assignmentLhs.Contains(identifier))
                        {
                            liveOut.Add(symbol);
                        }
                    }
                }

                private void ProcessOpAssignment(SyntaxNode instruction)
                {
                    var assignment = (AssignmentExpressionSyntax)instruction;
                    var left = assignment.Left.RemoveParentheses();
                    if (IdentifierRelevantSymbol(left) is { } symbol)
                    {
                        ReportOnAssignment(assignment, left, symbol);
                    }
                }

                private void ProcessSimpleAssignment(SyntaxNode instruction)
                {
                    var assignment = (AssignmentExpressionSyntax)instruction;
                    var left = assignment.Left.RemoveParentheses();
                    if (IdentifierRelevantSymbol(left) is { } symbol)
                    {
                        ReportOnAssignment(assignment, left, symbol);
                        liveOut.Remove(symbol);
                    }
                }

                private void ProcessVariableDeclarator(SyntaxNode instruction)
                {
                    var declarator = (VariableDeclaratorSyntax)instruction;
                    if (SemanticModel.GetDeclaredSymbol(declarator) is ILocalSymbol symbol && IsSymbolRelevant(symbol))
                    {
                        if (declarator.Initializer != null
                            && !IsAllowedInitializationValue(declarator.Initializer.Value)
                            && !symbol.IsConst
                            && symbol.RefKind() == RefKind.None
                            && !liveOut.Contains(symbol)
                            && !IsUnusedLocal(symbol)
                            && !IsMuted(declarator, symbol))
                        {
                            var location = GetFirstLineLocationFromToken(declarator.Initializer.EqualsToken, declarator.Initializer);
                            ReportIssue(location, symbol);
                        }
                        liveOut.Remove(symbol);
                    }
                }

                private bool IsUnusedLocal(ISymbol declaredSymbol) =>
                    node.DescendantNodes()
                        .OfType<IdentifierNameSyntax>()
                        .SelectMany(x => VariableUnusedBase.GetUsedSymbols(x, SemanticModel))
                        .All(x => !x.Equals(declaredSymbol));

                private void ProcessPrefixExpression(SyntaxNode instruction)
                {
                    var prefixExpression = (PrefixUnaryExpressionSyntax)instruction;
                    var parent = prefixExpression.GetSelfOrTopParenthesizedExpression();
                    var operand = prefixExpression.Operand.RemoveParentheses();
                    if (parent.Parent is ExpressionStatementSyntax
                        && IdentifierRelevantSymbol(operand) is { } symbol
                        && IsLocal(symbol)
                        && !liveOut.Contains(symbol)
                        && !IsMuted(operand))
                    {
                        ReportIssue(prefixExpression.GetLocation(), symbol);
                    }
                }

                private void ProcessPostfixExpression(SyntaxNode instruction)
                {
                    var postfixExpression = (PostfixUnaryExpressionSyntax)instruction;
                    var operand = postfixExpression.Operand.RemoveParentheses();
                    if (IdentifierRelevantSymbol(operand) is { } symbol
                        && IsLocal(symbol)
                        && !liveOut.Contains(symbol)
                        && !IsMuted(operand))
                    {
                        ReportIssue(postfixExpression.GetLocation(), symbol);
                    }
                }

                private void ReportOnAssignment(AssignmentExpressionSyntax assignment, ExpressionSyntax left, ISymbol symbol)
                {
                    if (IsLocal(symbol)
                        && !liveOut.Contains(symbol)
                        && !IsMuted(left))
                    {
                        var location = GetFirstLineLocationFromToken(assignment.OperatorToken, assignment.Right);
                        ReportIssue(location, symbol);
                    }

                    assignmentLhs.Add(left);
                }

                private bool IsMuted(SyntaxNode node) =>
                    new MutedSyntaxWalker(SemanticModel, node).IsMuted();

                private static Location GetFirstLineLocationFromToken(SyntaxToken issueStartToken, SyntaxNode wholeIssue)
                {
                    var line = wholeIssue.SyntaxTree.GetText().Lines[issueStartToken.GetLocation().GetLineSpan().StartLinePosition.Line];
                    var rightSingleLine = line.Span.Intersection(TextSpan.FromBounds(issueStartToken.SpanStart, wholeIssue.Span.End));
                    return Location.Create(wholeIssue.SyntaxTree, TextSpan.FromBounds(issueStartToken.SpanStart, rightSingleLine.HasValue ? rightSingleLine.Value.End : issueStartToken.Span.End));
                }

                private ISymbol IdentifierRelevantSymbol(SyntaxNode node) =>
                    node.IsKind(SyntaxKind.IdentifierName)
                    && SemanticModel.GetSymbolInfo(node).Symbol is { } symbol
                    && IsSymbolRelevant(symbol)
                    ? symbol
                    : null;
            }
        }
    }
}
