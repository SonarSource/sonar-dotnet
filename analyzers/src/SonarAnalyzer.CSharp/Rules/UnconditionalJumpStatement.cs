﻿/*
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

namespace SonarAnalyzer.CSharp.Rules
{
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

        protected override LoopWalkerBase<StatementSyntax, SyntaxKind> GetWalker(SonarSyntaxNodeReportingContext context)
            => new LoopWalker(context, LoopStatements);

        private class LoopWalker : LoopWalkerBase<StatementSyntax, SyntaxKind>
        {
            protected override ISet<SyntaxKind> StatementsThatCanThrow { get; } = new HashSet<SyntaxKind>
            {
                SyntaxKind.InvocationExpression,
                SyntaxKind.ObjectCreationExpression,
                SyntaxKind.SimpleMemberAccessExpression,
                SyntaxKind.PointerMemberAccessExpression,
                SyntaxKind.ElementAccessExpression
            };

            protected override ISet<SyntaxKind> LambdaSyntaxes { get; } = new HashSet<SyntaxKind>
            {
                SyntaxKind.ParenthesizedLambdaExpression,
                SyntaxKind.SimpleLambdaExpression,
                SyntaxKind.AnonymousMethodExpression
            };

            protected override ISet<SyntaxKind> LocalFunctionSyntaxes { get; } = new HashSet<SyntaxKind> { SyntaxKindEx.LocalFunctionStatement };

            protected override ISet<SyntaxKind> ConditionalStatements { get; } = new HashSet<SyntaxKind>
            {
                SyntaxKind.IfStatement,
                SyntaxKind.SwitchStatement,
                SyntaxKind.CatchClause
            };

            protected override ILanguageFacade<SyntaxKind> Language => CSharpFacade.Instance;

            public LoopWalker(SonarSyntaxNodeReportingContext context, ISet<SyntaxKind> loopStatements) : base(context, loopStatements) { }

            public override void Visit()
            {
                var csWalker = new CsLoopwalker(this);
                csWalker.SafeVisit(rootExpression);
            }

            protected override bool IsPropertyAccess(StatementSyntax node) =>
                node.DescendantNodes().OfType<IdentifierNameSyntax>().Any(x => semanticModel.GetSymbolInfo(x).Symbol is { } symbol && symbol.Kind == SymbolKind.Property);

            protected override bool TryGetTryAncestorStatements(StatementSyntax node, List<SyntaxNode> ancestors, out IEnumerable<StatementSyntax> tryAncestorStatements)
            {
                var tryAncestor = (TryStatementSyntax)ancestors.FirstOrDefault(n => n.IsKind(SyntaxKind.TryStatement));

                if (tryAncestor == null || tryAncestor.Catches.Count == 0)
                {
                    tryAncestorStatements = null;
                    return false;
                }

                tryAncestorStatements = tryAncestor.Block.Statements;
                return true;
            }

            private sealed class CsLoopwalker : SafeCSharpSyntaxWalker
            {
                private readonly LoopWalker walker;

                public CsLoopwalker(LoopWalker loopWalker)
                {
                    walker = loopWalker;
                }

                public override void VisitContinueStatement(ContinueStatementSyntax node)
                {
                    base.VisitContinueStatement(node);
                    walker.StoreVisitData(node, walker.ConditionalContinues, walker.UnconditionalContinues);
                }

                public override void VisitBreakStatement(BreakStatementSyntax node)
                {
                    base.VisitBreakStatement(node);
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
