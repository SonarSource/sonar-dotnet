/*
 * SonarAnalyzer for .NET
 * Copyright (C) SonarSource Sàrl
 * mailto:info AT sonarsource DOT com
 *
 * You can redistribute and/or modify this program under the terms of
 * the Sonar Source-Available License Version 1, as published by SonarSource Sàrl.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
 * See the Sonar Source-Available License for more details.
 *
 * You should have received a copy of the Sonar Source-Available License
 * along with this program; if not, see https://sonarsource.com/license/ssal/
 */

namespace SonarAnalyzer.VisualBasic.Rules;

[DiagnosticAnalyzer(LanguageNames.VisualBasic)]
public sealed class UnconditionalJumpStatement : UnconditionalJumpStatementBase<StatementSyntax, SyntaxKind>
{
    protected override ILanguageFacade<SyntaxKind> Language => VisualBasicFacade.Instance;

    protected override ISet<SyntaxKind> LoopStatements { get; } = new HashSet<SyntaxKind>
    {
        SyntaxKind.ForBlock,
        SyntaxKind.ForEachBlock,
        SyntaxKind.WhileBlock,
        SyntaxKind.DoLoopWhileBlock,
        SyntaxKind.DoLoopUntilBlock,
        SyntaxKind.SimpleDoLoopBlock
    };

    protected override LoopWalkerBase<StatementSyntax, SyntaxKind> Walker(SonarSyntaxNodeReportingContext context) =>
        new LoopWalker(context, LoopStatements);

    private sealed class LoopWalker : LoopWalkerBase<StatementSyntax, SyntaxKind>
    {
        private static readonly HashSet<SyntaxKind> StatementsThatCanThrowKinds =
        [
            SyntaxKind.InvocationExpression,
            SyntaxKind.ObjectCreationExpression,
            SyntaxKind.SimpleMemberAccessExpression
        ];

        private static readonly HashSet<SyntaxKind> LambdaKinds =
        [
            SyntaxKind.FunctionLambdaHeader,
            SyntaxKind.MultiLineFunctionLambdaExpression,
            SyntaxKind.MultiLineSubLambdaExpression,
            SyntaxKind.SingleLineFunctionLambdaExpression,
            SyntaxKind.SingleLineSubLambdaExpression,
            SyntaxKind.SubLambdaHeader
        ];

        private static readonly HashSet<SyntaxKind> LocalFunctionKinds = [];

        private static readonly HashSet<SyntaxKind> ConditionalStatementKinds =
        [
            SyntaxKind.SingleLineIfStatement,
            SyntaxKind.SingleLineIfPart,
            SyntaxKind.MultiLineIfBlock,
            SyntaxKind.CaseBlock,
            SyntaxKind.CaseElseBlock,
            SyntaxKind.CatchBlock
        ];

        protected override ISet<SyntaxKind> StatementsThatCanThrow => StatementsThatCanThrowKinds;
        protected override ISet<SyntaxKind> LambdaSyntaxes => LambdaKinds;
        protected override ISet<SyntaxKind> LocalFunctionSyntaxes => LocalFunctionKinds;
        protected override ISet<SyntaxKind> ConditionalStatements => ConditionalStatementKinds;
        protected override ILanguageFacade<SyntaxKind> Language => VisualBasicFacade.Instance;

        public LoopWalker(SonarSyntaxNodeReportingContext context, ISet<SyntaxKind> loopStatements) : base(context, loopStatements) { }

        public override bool Visit() =>
            new VbLoopwalker(this).SafeVisit(rootExpression);

        protected override bool IsPropertyAccess(StatementSyntax node) =>
            node.DescendantNodes().OfType<IdentifierNameSyntax>().Any(x => model.GetSymbolInfo(x).Symbol is IPropertySymbol);

        protected override IEnumerable<StatementSyntax> TryBlockStatements(IReadOnlyList<SyntaxNode> ancestors) =>
            ancestors.OfType<TryBlockSyntax>().FirstOrDefault() is { CatchBlocks.Count: > 0 } tryBlock
                ? tryBlock.Statements
                : Enumerable.Empty<StatementSyntax>();

        protected override bool IsJumpToRootLoop(StatementSyntax node) => false;

        private sealed class VbLoopwalker : SafeVisualBasicSyntaxWalker
        {
            private readonly LoopWalker walker;

            public VbLoopwalker(LoopWalker loopWalker) =>
                walker = loopWalker;

            public override void VisitContinueStatement(ContinueStatementSyntax node)
            {
                base.VisitContinueStatement(node);
                walker.StoreVisitData(node, walker.conditionalContinues, walker.unconditionalContinues);
            }

            public override void VisitExitStatement(ExitStatementSyntax node)
            {
                base.VisitExitStatement(node);
                walker.StoreVisitData(node, walker.conditionalTerminates, walker.unconditionalTerminates);
            }

            public override void VisitReturnStatement(ReturnStatementSyntax node)
            {
                base.VisitReturnStatement(node);
                walker.StoreVisitData(node, walker.conditionalTerminates, walker.unconditionalTerminates);
            }

            public override void VisitThrowStatement(ThrowStatementSyntax node)
            {
                base.VisitThrowStatement(node);
                walker.StoreVisitData(node, walker.conditionalTerminates, walker.unconditionalTerminates);
            }
        }
    }
}
