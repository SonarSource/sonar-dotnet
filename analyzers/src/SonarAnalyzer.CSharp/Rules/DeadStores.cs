/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2014-2025 SonarSource SA
 * mailto:info AT sonarsource DOT com
 * This program is free software; you can redistribute it and/or
 * modify it under the terms of the Sonar Source-Available License Version 1, as published by SonarSource SA.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
 * See the Sonar Source-Available License for more details.
 *
 * You should have received a copy of the Sonar Source-Available License
 * along with this program; if not, see https://sonarsource.com/license/ssal/
 */

using SonarAnalyzer.CFG.LiveVariableAnalysis;
using SonarAnalyzer.CFG.Sonar;
using SonarAnalyzer.CSharp.Core.LiveVariableAnalysis;

namespace SonarAnalyzer.Rules.CSharp
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed partial class DeadStores : SonarDiagnosticAnalyzer
    {
        private const string DiagnosticId = "S1854";
        private const string MessageFormat = "Remove this useless assignment to local variable '{0}'.";

        private static readonly DiagnosticDescriptor Rule = DescriptorFactory.Create(DiagnosticId, MessageFormat);
        private static readonly string[] AllowedNumericValues = { "-1", "0", "1" };
        private static readonly string[] AllowedStringValues = { string.Empty };
        private readonly bool useSonarCfg;

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(Rule);

        public DeadStores() : this(AnalyzerConfiguration.AlwaysEnabled) { }

        internal /* for testing */ DeadStores(IAnalyzerConfiguration configuration) =>
            useSonarCfg = configuration.UseSonarCfg();

        protected override void Initialize(SonarAnalysisContext context)
        {
            // No need to check for ExpressionBody as it can't contain variable assignment
            context.RegisterNodeAction(
                c => CheckForDeadStores(c, c.Model.GetDeclaredSymbol(c.Node)),
                SyntaxKind.AddAccessorDeclaration,
                SyntaxKind.ConstructorDeclaration,
                SyntaxKind.ConversionOperatorDeclaration,
                SyntaxKind.DestructorDeclaration,
                SyntaxKind.GetAccessorDeclaration,
                SyntaxKind.MethodDeclaration,
                SyntaxKind.OperatorDeclaration,
                SyntaxKind.RemoveAccessorDeclaration,
                SyntaxKind.SetAccessorDeclaration,
                SyntaxKindEx.CoalesceAssignmentExpression,
                SyntaxKindEx.InitAccessorDeclaration,
                SyntaxKindEx.LocalFunctionStatement);

            context.RegisterNodeAction(
                c => CheckForDeadStores(c, c.Model.GetSymbolInfo(c.Node).Symbol),
                SyntaxKind.AnonymousMethodExpression,
                SyntaxKind.ParenthesizedLambdaExpression,
                SyntaxKind.SimpleLambdaExpression);
        }

        private void CheckForDeadStores(SonarSyntaxNodeReportingContext context, ISymbol symbol)
        {
            if (symbol != null)
            {
                if (useSonarCfg)
                {
                    // Tuple expressions are not supported. See https://github.com/SonarSource/sonar-dotnet/issues/3094
                    if (!context.Node.DescendantNodes().AnyOfKind(SyntaxKindEx.TupleExpression) && CSharpControlFlowGraph.TryGet(context.Node, context.Model, out var cfg))
                    {
                        var lva = new SonarCSharpLiveVariableAnalysis(cfg, symbol, context.Model, context.Cancel);
                        var checker = new SonarChecker(context, lva, context.Node);
                        checker.Analyze(cfg.Blocks);
                    }
                }
                else if (context.Node.CreateCfg(context.Model, context.Cancel) is { } cfg)
                {
                    var lva = new RoslynLiveVariableAnalysis(cfg, CSharpSyntaxClassifier.Instance, context.Cancel);
                    var checker = new RoslynChecker(context, lva);
                    checker.Analyze(cfg.Blocks);
                }
            }
        }

        private abstract class CheckerBase<TCfg, TBlock>
        {
            private readonly LiveVariableAnalysisBase<TCfg, TBlock> lva;
            private readonly SonarSyntaxNodeReportingContext context;
            private readonly ISet<ISymbol> capturedVariables;

            protected abstract State CreateState(TBlock block);

            protected CheckerBase(SonarSyntaxNodeReportingContext context, LiveVariableAnalysisBase<TCfg, TBlock> lva)
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

                protected SemanticModel SemanticModel => owner.context.Model;

                protected State(CheckerBase<TCfg, TBlock> owner, TBlock block)
                {
                    this.owner = owner;
                    this.block = block;
                    liveOut = new HashSet<ISymbol>(owner.lva.LiveOut(block));
                }

                protected void ReportIssue(Location location, ISymbol symbol) =>
                    owner.context.ReportIssue(Rule, location, symbol.Name);

                protected bool IsSymbolRelevant(ISymbol symbol) =>
                    symbol != null && !owner.capturedVariables.Contains(symbol);

                protected bool IsLocal(ISymbol symbol) =>
                    owner.IsLocal(symbol);

                protected bool IsAllowedInitializationValue(ExpressionSyntax value, Optional<object> constantValue = default) =>
                    (constantValue.HasValue && IsAllowedInitializationConstant(constantValue.Value, value.IsKind(SyntaxKind.IdentifierName)))
                    || value?.Kind() is SyntaxKind.DefaultExpression or SyntaxKind.TrueLiteralExpression or SyntaxKind.FalseLiteralExpression
                    || value.IsNullLiteral()
                    || IsAllowedNumericInitialization(value)
                    || IsAllowedUnaryNumericInitialization(value)
                    || IsAllowedStringInitialization(value);

                protected bool IsMuted(SyntaxNode node, ISymbol symbol) =>
                    new MutedSyntaxWalker(SemanticModel, node, symbol).IsMuted();

                private static bool IsAllowedInitializationConstant(object constant, bool isIdentifier) =>
                    constant == null
                    || (isIdentifier && IsAllowedInitializationConstantIdentifier(constant));

                private static bool IsAllowedInitializationConstantIdentifier(object constant) =>
                    constant is string str ? AllowedStringValues.Contains(str) : AllowedNumericValues.Contains(constant.ToString());

                private static bool IsAllowedNumericInitialization(ExpressionSyntax expression) =>
                    expression.IsKind(SyntaxKind.NumericLiteralExpression) && AllowedNumericValues.Contains(((LiteralExpressionSyntax)expression).Token.ValueText);  // -1, 0 or 1

                private static bool IsAllowedUnaryNumericInitialization(ExpressionSyntax expression) =>
                    expression?.Kind() is SyntaxKind.UnaryPlusExpression or SyntaxKind.UnaryMinusExpression
                    && IsAllowedNumericInitialization(((PrefixUnaryExpressionSyntax)expression).Operand);

                private bool IsAllowedStringInitialization(ExpressionSyntax expression) =>
                    (expression.IsKind(SyntaxKind.StringLiteralExpression) && AllowedStringValues.Contains(((LiteralExpressionSyntax)expression).Token.ValueText))
                    || (expression.IsKind(SyntaxKind.InterpolatedStringExpression) && ((InterpolatedStringExpressionSyntax)expression).Contents.Count == 0)
                    || expression.IsStringEmpty(SemanticModel);
            }
        }
    }
}
