/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2017 SonarSource SA
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

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.VisualBasic;
using Microsoft.CodeAnalysis.VisualBasic.Syntax;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace SonarAnalyzer.Common.VisualBasic
{
    public sealed class Metrics : MetricsBase
    {
        public Metrics(SyntaxTree tree) : base(tree)
        {
            var root = tree.GetRoot();
            if (root.Language != LanguageNames.VisualBasic)
            {
                throw new ArgumentException(InitalizationErrorTextPattern, nameof(tree));
            }
        }

        protected override bool IsEndOfFile(SyntaxToken token) => token.IsKind(SyntaxKind.EndOfFileToken);

        protected override bool IsNoneToken(SyntaxToken token) => token.IsKind(SyntaxKind.None);

        protected override bool IsCommentTrivia(SyntaxTrivia trivia) => TriviaKinds.Contains(trivia.Kind());

        protected override bool IsClass(SyntaxNode node) => ClassKinds.Contains(node.Kind());

        protected override bool IsStatement(SyntaxNode node) => node is ExecutableStatementSyntax;

        protected override bool IsFunction(SyntaxNode node)
        {
            if (!FunctionKinds.Contains(node.Kind()) ||
                !MethodBlocks.Contains(node.Parent.Kind()) ||
                node.Parent.Parent.IsKind(SyntaxKind.InterfaceBlock))
            {
                return false;
            }

            var method = node as MethodBaseSyntax;
            if (method != null && method.Modifiers.Any(m => m.IsKind(SyntaxKind.MustOverrideKeyword)))
            {
                return false;
            }

            return true;
        }

        protected override IEnumerable<SyntaxNode> PublicApiNodes => Enumerable.Empty<SyntaxNode>(); // Not calculated for VB.Net

        private bool IsComplexityIncreasingKind(SyntaxNode node) =>
            ComplexityIncreasingKinds.Contains(node.Kind());

        public override int GetComplexity(SyntaxNode node) =>
            node.DescendantNodesAndSelf()
                .Count(n =>
                    IsComplexityIncreasingKind(n) ||
                    IsFunction(n));

        public override int GetCognitiveComplexity(SyntaxNode node)
        {
            return 0; // Not implemented
        }

        public override int GetExecutableLinesCount()
        {
            return 0; // Not implemented
        }

        private static readonly ISet<SyntaxKind> TriviaKinds = ImmutableHashSet.Create(
            SyntaxKind.CommentTrivia,
            SyntaxKind.DocumentationCommentExteriorTrivia,
            SyntaxKind.DocumentationCommentTrivia
        );

        private static readonly ISet<SyntaxKind> ClassKinds = ImmutableHashSet.Create(
            SyntaxKind.ClassBlock,
            SyntaxKind.StructureBlock,
            SyntaxKind.InterfaceBlock,
            SyntaxKind.ModuleBlock
        );

        private static readonly ISet<SyntaxKind> FunctionKinds = ImmutableHashSet.Create(
            SyntaxKind.SubNewStatement,
            SyntaxKind.SubStatement,
            SyntaxKind.FunctionStatement,
            SyntaxKind.OperatorStatement,
            SyntaxKind.GetAccessorStatement,
            SyntaxKind.SetAccessorStatement,
            SyntaxKind.RaiseEventAccessorStatement,
            SyntaxKind.AddHandlerAccessorStatement,
            SyntaxKind.RemoveHandlerAccessorStatement,
            SyntaxKind.DeclareSubStatement,
            SyntaxKind.DeclareFunctionStatement
        );

        private static readonly ISet<SyntaxKind> MethodBlocks = ImmutableHashSet.Create(
            SyntaxKind.ConstructorBlock,
            SyntaxKind.FunctionBlock,
            SyntaxKind.SubBlock,
            SyntaxKind.OperatorBlock,
            SyntaxKind.GetAccessorBlock,
            SyntaxKind.SetAccessorBlock,
            SyntaxKind.RaiseEventAccessorBlock,
            SyntaxKind.AddHandlerAccessorBlock,
            SyntaxKind.RemoveHandlerAccessorBlock
        );

        private static readonly ISet<SyntaxKind> ComplexityIncreasingKinds = ImmutableHashSet.Create(
            SyntaxKind.IfStatement,
            SyntaxKind.SingleLineIfStatement,
            SyntaxKind.TernaryConditionalExpression,
            SyntaxKind.CaseStatement,

            SyntaxKind.WhileStatement,
            SyntaxKind.DoWhileStatement,
            SyntaxKind.DoUntilStatement,
            SyntaxKind.SimpleDoStatement,
            SyntaxKind.ForStatement,
            SyntaxKind.ForEachStatement,

            SyntaxKind.ThrowStatement,
            SyntaxKind.TryStatement,

            SyntaxKind.ErrorStatement,

            SyntaxKind.ResumeStatement,
            SyntaxKind.ResumeNextStatement,
            SyntaxKind.ResumeLabelStatement,

            SyntaxKind.OnErrorGoToLabelStatement,
            SyntaxKind.OnErrorGoToMinusOneStatement,
            SyntaxKind.OnErrorGoToZeroStatement,
            SyntaxKind.OnErrorResumeNextStatement,

            SyntaxKind.GoToStatement,

            SyntaxKind.ExitDoStatement,
            SyntaxKind.ExitForStatement,
            SyntaxKind.ExitFunctionStatement,
            SyntaxKind.ExitOperatorStatement,
            SyntaxKind.ExitPropertyStatement,
            SyntaxKind.ExitSelectStatement,
            SyntaxKind.ExitSubStatement,
            SyntaxKind.ExitTryStatement,
            SyntaxKind.ExitWhileStatement,

            SyntaxKind.ContinueDoStatement,
            SyntaxKind.ContinueForStatement,
            SyntaxKind.ContinueWhileStatement,

            SyntaxKind.StopStatement,

            SyntaxKind.AndAlsoExpression,
            SyntaxKind.OrElseExpression,

            SyntaxKind.EndStatement
        );
    }
}
