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

namespace SonarAnalyzer.CSharp.Rules;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class UnconditionalJumpStatement : UnconditionalJumpStatementBase<StatementSyntax, SyntaxKind>
{
    protected override ILanguageFacade<SyntaxKind> Language => CSharpFacade.Instance;

    protected override ISet<SyntaxKind> LoopStatements { get; } = new HashSet<SyntaxKind>
    {
        SyntaxKind.ForEachStatement,
        SyntaxKind.ForStatement,
        SyntaxKind.WhileStatement,
        SyntaxKind.DoStatement
    };

    protected override LoopWalkerBase<StatementSyntax, SyntaxKind> Walker(SonarSyntaxNodeReportingContext context) =>
        new LoopWalker(context, LoopStatements);

    private sealed class LoopWalker : LoopWalkerBase<StatementSyntax, SyntaxKind>
    {
        private static readonly HashSet<SyntaxKind> StatementsThatCanThrowKinds =
        [
            SyntaxKind.InvocationExpression,
            SyntaxKind.ObjectCreationExpression,
            SyntaxKind.SimpleMemberAccessExpression,
            SyntaxKind.PointerMemberAccessExpression,
            SyntaxKind.ElementAccessExpression
        ];

        private static readonly HashSet<SyntaxKind> LambdaKinds =
        [
            SyntaxKind.ParenthesizedLambdaExpression,
            SyntaxKind.SimpleLambdaExpression,
            SyntaxKind.AnonymousMethodExpression
        ];

        private static readonly HashSet<SyntaxKind> LocalFunctionKinds = [SyntaxKindEx.LocalFunctionStatement];

        private static readonly HashSet<SyntaxKind> ConditionalStatementKinds =
        [
            SyntaxKind.IfStatement,
            SyntaxKind.SwitchStatement,
            SyntaxKind.CatchClause
        ];

        protected override ISet<SyntaxKind> StatementsThatCanThrow => StatementsThatCanThrowKinds;
        protected override ISet<SyntaxKind> LambdaSyntaxes => LambdaKinds;
        protected override ISet<SyntaxKind> LocalFunctionSyntaxes => LocalFunctionKinds;
        protected override ISet<SyntaxKind> ConditionalStatements => ConditionalStatementKinds;
        protected override ILanguageFacade<SyntaxKind> Language => CSharpFacade.Instance;

        public LoopWalker(SonarSyntaxNodeReportingContext context, ISet<SyntaxKind> loopStatements) : base(context, loopStatements) { }

        public override bool Visit() =>
            new CsLoopwalker(this).SafeVisit(rootExpression);

        protected override bool IsPropertyAccess(StatementSyntax node) =>
            node.DescendantNodes().OfType<IdentifierNameSyntax>().Any(x => model.GetSymbolInfo(x).Symbol is IPropertySymbol);

        protected override IEnumerable<StatementSyntax> TryBlockStatements(IReadOnlyList<SyntaxNode> ancestors) =>
            ancestors.OfType<TryStatementSyntax>().FirstOrDefault() is { Catches.Count: > 0 } tryStatement
                ? tryStatement.Block.Statements
                : Enumerable.Empty<StatementSyntax>();

        protected override bool IsJumpToRootLoop(StatementSyntax node) =>
            rootExpression.Parent is LabeledStatementSyntax rootLabel
            && JumpLabel(node) is { } label
            && label.Identifier.ValueText == rootLabel.Identifier.ValueText;

        private static IdentifierNameSyntax JumpLabel(StatementSyntax node) =>
            node switch
            {
                BreakStatementSyntax breakStatement => breakStatement.Name,
                ContinueStatementSyntax continueStatement => continueStatement.Name,
                _ => null
            };

        private sealed class CsLoopwalker : SafeCSharpSyntaxWalker
        {
            private readonly LoopWalker walker;

            public CsLoopwalker(LoopWalker loopWalker) =>
                walker = loopWalker;

            public override void VisitContinueStatement(ContinueStatementSyntax node)
            {
                base.VisitContinueStatement(node);
                walker.StoreVisitData(node, walker.conditionalContinues, walker.unconditionalContinues);
            }

            public override void VisitBreakStatement(BreakStatementSyntax node)
            {
                base.VisitBreakStatement(node);
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
