/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2020 SonarSource SA
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
using SonarAnalyzer.Common;
using SonarAnalyzer.Helpers;
using SonarAnalyzer.ShimLayer.CSharp;

namespace SonarAnalyzer.Rules.CSharp
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    [Rule(DiagnosticId)]
    public sealed class UnconditionalJumpStatement : UnconditionalJumpStatementBase<StatementSyntax, SyntaxKind>
    {
        private static readonly DiagnosticDescriptor rule =
            DiagnosticDescriptorBuilder.GetDescriptor(DiagnosticId, MessageFormat, RspecStrings.ResourceManager);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(rule);
        protected override GeneratedCodeRecognizer GeneratedCodeRecognizer => Helpers.CSharp.CSharpGeneratedCodeRecognizer.Instance;
        protected override ISet<SyntaxKind> LoopStatements { get; } = new HashSet<SyntaxKind>
        {
            SyntaxKind.ForEachStatement,
            SyntaxKind.ForStatement,
            SyntaxKind.WhileStatement,
            SyntaxKind.DoStatement
        };

        protected override LoopWalkerBase<StatementSyntax, SyntaxKind> GetWalker(SyntaxNodeAnalysisContext context)
            => new LoopWalker(context, LoopStatements);


        private class LoopWalker : LoopWalkerBase<StatementSyntax, SyntaxKind>
        {
            protected override ISet<SyntaxKind> StatementsThatCanThrow { get; } = new HashSet<SyntaxKind>
            {
                SyntaxKind.InvocationExpression,
                SyntaxKind.ObjectCreationExpression
            };

            protected override ISet<SyntaxKind> LambdaSyntaxes { get; }
                = new HashSet<SyntaxKind> {
                    SyntaxKind.ParenthesizedLambdaExpression,
                    SyntaxKind.SimpleLambdaExpression };

            protected override ISet<SyntaxKind> LocalFunctionSyntaxes { get; } = new HashSet<SyntaxKind> { SyntaxKindEx.LocalFunctionStatement };

            protected override ISet<SyntaxKind> ConditionalStatements { get; } = new HashSet<SyntaxKind>
            {
                SyntaxKind.IfStatement,
                SyntaxKind.SwitchStatement,
                SyntaxKind.CatchClause
            };

            public LoopWalker(SyntaxNodeAnalysisContext context, ISet<SyntaxKind> loopStatements)
                : base(context, loopStatements)
            {
            }

            public override void Visit()
            {
                var csWalker = new CsLoopwalker(this);
                csWalker.SafeVisit(this.rootExpression);
            }

            private class CsLoopwalker : CSharpSyntaxWalker
            {
                private readonly LoopWalker walker;

                public CsLoopwalker(LoopWalker loopWalker)
                {
                    this.walker = loopWalker;
                }

                public override void VisitContinueStatement(ContinueStatementSyntax node)
                {
                    base.VisitContinueStatement(node);
                    this.walker.StoreVisitData(node, this.walker.ConditionalContinues, this.walker.UnconditionalContinues);
                }

                public override void VisitBreakStatement(BreakStatementSyntax node)
                {
                    base.VisitBreakStatement(node);
                    this.walker.StoreVisitData(node, this.walker.ConditionalTerminates, this.walker.UnconditionalTerminates);
                }

                public override void VisitReturnStatement(ReturnStatementSyntax node)
                {
                    base.VisitReturnStatement(node);
                    this.walker.StoreVisitData(node, this.walker.ConditionalTerminates, this.walker.UnconditionalTerminates);
                }

                public override void VisitThrowStatement(ThrowStatementSyntax node)
                {
                    base.VisitThrowStatement(node);
                    this.walker.StoreVisitData(node, this.walker.ConditionalTerminates, this.walker.UnconditionalTerminates);
                }
            }

            protected override bool IsAnyKind(SyntaxNode node, ISet<SyntaxKind> syntaxKinds) => node.IsAnyKind(syntaxKinds);
            protected override bool IsReturnStatement(SyntaxNode node) => node.IsKind(SyntaxKind.ReturnStatement);

            protected override bool TryGetTryAncestorStatements(StatementSyntax node, List<SyntaxNode> ancestors, out IEnumerable<StatementSyntax> tryAncestorStatements)
            {
                var tryAncestor = (TryStatementSyntax)ancestors.FirstOrDefault(n => n.IsKind(SyntaxKind.TryStatement));

                if (tryAncestor == null ||
                    tryAncestor.Catches.Count == 0)
                {
                    tryAncestorStatements = null;
                    return false;
                }

                tryAncestorStatements = tryAncestor.Block.Statements;
                return true;
            }
        }
    }
}
