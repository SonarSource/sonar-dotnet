/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2018 SonarSource SA
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
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.VisualBasic;
using Microsoft.CodeAnalysis.VisualBasic.Syntax;
using SonarAnalyzer.Common;
using SonarAnalyzer.Helpers;
using SonarAnalyzer.Helpers.VisualBasic;

namespace SonarAnalyzer.Rules.VisualBasic
{
    [DiagnosticAnalyzer(LanguageNames.VisualBasic)]
    [Rule(DiagnosticId)]
    public sealed class UnconditionalJumpStatement : UnconditionalJumpStatementBase<StatementSyntax, SyntaxKind>
    {
        private static readonly DiagnosticDescriptor rule =
                    DiagnosticDescriptorBuilder.GetDescriptor(DiagnosticId, MessageFormat, RspecStrings.ResourceManager);

        protected override DiagnosticDescriptor Rule => rule;
        protected override Helpers.GeneratedCodeRecognizer GeneratedCodeRecognizer => Helpers.VisualBasic.GeneratedCodeRecognizer.Instance;
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(rule);

        protected override ISet<SyntaxKind> LoopStatements { get; } = new HashSet<SyntaxKind>
        {
            SyntaxKind.ForBlock,
            SyntaxKind.ForEachBlock,
            SyntaxKind.WhileBlock,
            SyntaxKind.DoLoopWhileBlock,
            SyntaxKind.DoLoopUntilBlock,
            SyntaxKind.SimpleDoLoopBlock
        };

        protected override string GetKeywordText(StatementSyntax statement) =>
            (statement as ExitStatementSyntax)?.ExitKeyword.ToString() ??
            (statement as ContinueStatementSyntax)?.ContinueKeyword.ToString() ??
            (statement as ReturnStatementSyntax)?.ReturnKeyword.ToString() ??
            (statement as ThrowStatementSyntax)?.ThrowKeyword.ToString();

        protected override LoopWalkerBase<StatementSyntax, SyntaxKind> GetWalker(SyntaxNodeAnalysisContext context)
            => new LoopWalker(context, LoopStatements);

        private class LoopWalker : LoopWalkerBase<StatementSyntax, SyntaxKind>
        {
            protected override ISet<SyntaxKind> StatementsThatCanThrow { get; } = new HashSet<SyntaxKind>
            {
                SyntaxKind.InvocationExpression,
                SyntaxKind.ObjectCreationExpression
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

            protected override ISet<SyntaxKind> ConditionalStatements { get; } = new HashSet<SyntaxKind>
            {
                SyntaxKind.SingleLineIfStatement,
                SyntaxKind.SingleLineIfPart,
                SyntaxKind.MultiLineIfBlock,
                SyntaxKind.CaseBlock,
                SyntaxKind.CaseElseBlock,
                SyntaxKind.CatchBlock
            };

            public LoopWalker(SyntaxNodeAnalysisContext context, ISet<SyntaxKind> loopStatements)
                : base(context, loopStatements)
            {
            }

            public override void Visit()
            {
                var vbWalker = new VbLoopwalker(this);
                vbWalker.Visit(this.rootExpression);
            }

            private class VbLoopwalker : VisualBasicSyntaxWalker
            {
                private readonly LoopWalker walker;

                public VbLoopwalker(LoopWalker loopWalker)
                {
                    this.walker = loopWalker;
                }

                public override void VisitContinueStatement(ContinueStatementSyntax node)
                {
                    base.VisitContinueStatement(node);
                    this.walker.StoreVisitData(node, this.walker.ConditionalContinues, this.walker.UnconditionalContinues);
                }

                public override void VisitExitStatement(ExitStatementSyntax node)
                {
                    base.VisitExitStatement(node);
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
                var tryAncestor = (TryBlockSyntax)ancestors.FirstOrDefault(n => n.IsKind(SyntaxKind.TryBlock));

                if (tryAncestor == null ||
                    tryAncestor.CatchBlocks.Count == 0)
                {
                    tryAncestorStatements = null;
                    return false;
                }

                tryAncestorStatements = tryAncestor.Statements;
                return true;
            }
        }
    }
}
