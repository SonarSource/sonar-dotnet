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
using SonarAnalyzer.CFG.LiveVariableAnalysis;
using SonarAnalyzer.CFG.Sonar;
using SonarAnalyzer.Common;
using SonarAnalyzer.Extensions;
using SonarAnalyzer.Helpers;
using SonarAnalyzer.LiveVariableAnalysis.CSharp;
using StyleCop.Analyzers.Lightup;

namespace SonarAnalyzer.Rules.CSharp
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    [Rule(DiagnosticId)]
    public sealed partial class DeadStores : SonarDiagnosticAnalyzer
    {
        private const string DiagnosticId = "S1854";
        private const string MessageFormat = "Remove this useless assignment to local variable '{0}'.";

        private static readonly DiagnosticDescriptor Rule = DiagnosticDescriptorBuilder.GetDescriptor(DiagnosticId, MessageFormat, RspecStrings.ResourceManager);
        private static readonly string[] AllowedNumericValues = new[] { "-1", "0", "1" };
        private static readonly string[] AllowedStringValues = new[] { string.Empty };
        private readonly bool useSonarCfg;

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(Rule);

        public DeadStores() : this(AnalyzerConfiguration.AlwaysEnabled) { }

        internal /* for testing */ DeadStores(IAnalyzerConfiguration configuration) =>
            useSonarCfg = configuration.UseSonarCfg();

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

        private void CheckForDeadStores(SyntaxNodeAnalysisContext context, ISymbol symbol, CSharpSyntaxNode node)
        {
            if (symbol != null && node != null)
            {
                if (useSonarCfg)
                {
                    // Tuple expressions are not supported. See https://github.com/SonarSource/sonar-dotnet/issues/3094
                    if (!node.DescendantNodes().AnyOfKind(SyntaxKindEx.TupleExpression) && CSharpControlFlowGraph.TryGet(node, context.SemanticModel, out var cfg))
                    {
                        var lva = new SonarCSharpLiveVariableAnalysis(cfg, symbol, context.SemanticModel);
                        var checker = new SonarChecker(context, lva, node);
                        checker.Analyze(cfg.Blocks);
                    }
                }
                else if (node.CreateCfg(context.SemanticModel) is { } cfg)
                {
                    var lva = new RoslynLiveVariableAnalysis(cfg, symbol);
                    var checker = new RoslynChecker(context, lva);
                    checker.Analyze(cfg.Blocks);
                }
            }
        }

        private abstract class CheckerBase<TCfg, TBlock>
        {
            private readonly LiveVariableAnalysisBase<TCfg, TBlock> lva;
            private readonly SyntaxNodeAnalysisContext context;
            private readonly ISet<ISymbol> capturedVariables;

            protected abstract State CreateState(TBlock block);

            protected CheckerBase(SyntaxNodeAnalysisContext context, LiveVariableAnalysisBase<TCfg, TBlock> lva)
            {
                this.context = context;
                this.lva = lva;
                capturedVariables = lva.CapturedVariables.ToHashSet();
            }

            public void Analyze(IEnumerable<TBlock> blocks)
            {
                foreach (var block in blocks)
                {
                    var state = CreateState(block);
                    state.AnalyzeBlock();
                }
            }

            protected bool IsLocal(ISymbol symbol) =>
                lva.IsLocal(symbol);

            protected abstract class State
            {
                protected readonly TBlock block;
                protected readonly ISet<ISymbol> liveOut;
                private readonly CheckerBase<TCfg, TBlock> owner;

                public abstract void AnalyzeBlock();

                protected SemanticModel SemanticModel => owner.context.SemanticModel;

                protected State(CheckerBase<TCfg, TBlock> owner, TBlock block)
                {
                    this.owner = owner;
                    this.block = block;
                    liveOut = new HashSet<ISymbol>(owner.lva.LiveOut(block));
                }

                protected void ReportIssue(Location location, ISymbol symbol) =>
                    owner.context.ReportDiagnosticWhenActive(Diagnostic.Create(Rule, location, symbol.Name));

                protected bool IsSymbolRelevant(ISymbol symbol) =>
                    symbol != null && !owner.capturedVariables.Contains(symbol);

                protected bool IsLocal(ISymbol symbol) =>
                    owner.IsLocal(symbol);

                protected bool IsAllowedInitializationValue(ExpressionSyntax value, Optional<object> constantValue = default) =>
                    (constantValue.HasValue && IsAllowedInitializationConstant(constantValue.Value, value.IsKind(SyntaxKind.IdentifierName)))
                    || value.IsKind(SyntaxKind.DefaultExpression)
                    || value.IsNullLiteral()
                    || value.IsAnyKind(SyntaxKind.TrueLiteralExpression, SyntaxKind.FalseLiteralExpression)
                    || IsAllowedNumericInitialization(value)
                    || IsAllowedUnaryNumericInitialization(value)
                    || IsAllowedStringInitialization(value);

                private static bool IsAllowedInitializationConstant(object constant, bool isIdentifier) =>
                    constant == null
                    || (isIdentifier && IsAllowedInitializationConstantIdentifier(constant));

                private static bool IsAllowedInitializationConstantIdentifier(object constant) =>
                    constant is string str ? AllowedStringValues.Contains(str) : AllowedNumericValues.Contains(constant.ToString());

                private static bool IsAllowedNumericInitialization(ExpressionSyntax expression) =>
                    expression.IsKind(SyntaxKind.NumericLiteralExpression) && AllowedNumericValues.Contains(((LiteralExpressionSyntax)expression).Token.ValueText);  // -1, 0 or 1

                private static bool IsAllowedUnaryNumericInitialization(ExpressionSyntax expression) =>
                    expression.IsAnyKind(SyntaxKind.UnaryPlusExpression, SyntaxKind.UnaryMinusExpression) && IsAllowedNumericInitialization(((PrefixUnaryExpressionSyntax)expression).Operand);

                private bool IsAllowedStringInitialization(ExpressionSyntax expression) =>
                    (expression.IsKind(SyntaxKind.StringLiteralExpression) && AllowedStringValues.Contains(((LiteralExpressionSyntax)expression).Token.ValueText))
                    || expression.IsStringEmpty(SemanticModel);
            }
        }
    }
}
