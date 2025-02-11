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

namespace SonarAnalyzer.VisualBasic.Rules
{
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

        protected override LoopWalkerBase<StatementSyntax, SyntaxKind> GetWalker(SonarSyntaxNodeReportingContext context)
            => new LoopWalker(context, LoopStatements);

        private class LoopWalker : LoopWalkerBase<StatementSyntax, SyntaxKind>
        {
            protected override ISet<SyntaxKind> StatementsThatCanThrow { get; } = new HashSet<SyntaxKind>
            {
                SyntaxKind.InvocationExpression,
                SyntaxKind.ObjectCreationExpression,
                SyntaxKind.SimpleMemberAccessExpression
            };

            protected override ISet<SyntaxKind> LambdaSyntaxes { get; } = new HashSet<SyntaxKind>
                {
                    SyntaxKind.FunctionLambdaHeader,
                    SyntaxKind.MultiLineFunctionLambdaExpression,
                    SyntaxKind.MultiLineSubLambdaExpression,
                    SyntaxKind.SingleLineFunctionLambdaExpression,
                    SyntaxKind.SingleLineSubLambdaExpression,
                    SyntaxKind.SubLambdaHeader
                };

            protected override ISet<SyntaxKind> LocalFunctionSyntaxes { get; } = new HashSet<SyntaxKind>();

            protected override ISet<SyntaxKind> ConditionalStatements { get; } = new HashSet<SyntaxKind>
            {
                SyntaxKind.SingleLineIfStatement,
                SyntaxKind.SingleLineIfPart,
                SyntaxKind.MultiLineIfBlock,
                SyntaxKind.CaseBlock,
                SyntaxKind.CaseElseBlock,
                SyntaxKind.CatchBlock
            };

            protected override ILanguageFacade<SyntaxKind> Language => VisualBasicFacade.Instance;

            public LoopWalker(SonarSyntaxNodeReportingContext context, ISet<SyntaxKind> loopStatements) : base(context, loopStatements) { }

            public override void Visit()
            {
                var vbWalker = new VbLoopwalker(this);
                vbWalker.SafeVisit(rootExpression);
            }

            protected override bool IsPropertyAccess(StatementSyntax node) =>
                node.DescendantNodes().OfType<IdentifierNameSyntax>().Any(x => semanticModel.GetSymbolInfo(x).Symbol is { } symbol && symbol.Kind == SymbolKind.Property);

            protected override bool TryGetTryAncestorStatements(StatementSyntax node, List<SyntaxNode> ancestors, out IEnumerable<StatementSyntax> tryAncestorStatements)
            {
                var tryAncestor = (TryBlockSyntax)ancestors.FirstOrDefault(n => n.IsKind(SyntaxKind.TryBlock));

                if (tryAncestor == null || tryAncestor.CatchBlocks.Count == 0)
                {
                    tryAncestorStatements = null;
                    return false;
                }

                tryAncestorStatements = tryAncestor.Statements;
                return true;
            }

            private class VbLoopwalker : SafeVisualBasicSyntaxWalker
            {
                private readonly LoopWalker walker;

                public VbLoopwalker(LoopWalker loopWalker)
                {
                    walker = loopWalker;
                }

                public override void VisitContinueStatement(ContinueStatementSyntax node)
                {
                    base.VisitContinueStatement(node);
                    walker.StoreVisitData(node, walker.ConditionalContinues, walker.UnconditionalContinues);
                }

                public override void VisitExitStatement(ExitStatementSyntax node)
                {
                    base.VisitExitStatement(node);
                    walker.StoreVisitData(node, walker.ConditionalTerminates, walker.UnconditionalTerminates);
                }

                public override void VisitReturnStatement(ReturnStatementSyntax node)
                {
                    base.VisitReturnStatement(node);
                    walker.StoreVisitData(node, walker.ConditionalTerminates, walker.UnconditionalTerminates);
                }

                public override void VisitThrowStatement(ThrowStatementSyntax node)
                {
                    base.VisitThrowStatement(node);
                    walker.StoreVisitData(node, walker.ConditionalTerminates, walker.UnconditionalTerminates);
                }
            }
        }
    }
}
