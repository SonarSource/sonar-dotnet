/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2019 SonarSource SA
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

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.VisualBasic;
using Microsoft.CodeAnalysis.VisualBasic.Syntax;
using SonarAnalyzer.Common;
using SonarAnalyzer.Helpers.VisualBasic;

namespace SonarAnalyzer.Metrics.VisualBasic
{
    public sealed class VisualBasicMetrics : MetricsBase
    {
        private static readonly ISet<SyntaxKind> FunctionKinds = new HashSet<SyntaxKind>
        {
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
        };

        private static readonly ISet<SyntaxKind> MethodBlocks = new HashSet<SyntaxKind>
        {
            SyntaxKind.ConstructorBlock,
            SyntaxKind.FunctionBlock,
            SyntaxKind.SubBlock,
            SyntaxKind.OperatorBlock,
            SyntaxKind.GetAccessorBlock,
            SyntaxKind.SetAccessorBlock,
            SyntaxKind.RaiseEventAccessorBlock,
            SyntaxKind.AddHandlerAccessorBlock,
            SyntaxKind.RemoveHandlerAccessorBlock
        };

        private readonly Lazy<ImmutableArray<int>> lazyExecutableLines;
        private readonly Lazy<ImmutableArray<SyntaxNode>> publicApiNodes;

        public VisualBasicMetrics(SyntaxTree tree, SemanticModel semanticModel)
            : base(tree)
        {
            var root = tree.GetRoot();
            if (root.Language != LanguageNames.VisualBasic)
            {
                throw new ArgumentException(InitalizationErrorTextPattern, nameof(tree));
            }

            this.lazyExecutableLines = new Lazy<ImmutableArray<int>>(() => VisualBasicExecutableLinesMetric.GetLineNumbers(tree, semanticModel));
            this.publicApiNodes = new Lazy<ImmutableArray<SyntaxNode>>(() => VisualBasicPublicApiMetric.GetMembers(tree));
        }

        public override ImmutableArray<int> ExecutableLines =>
            this.lazyExecutableLines.Value;

        protected override ImmutableArray<SyntaxNode> PublicApiNodes =>
            this.publicApiNodes.Value;

        public override int GetCognitiveComplexity(SyntaxNode node) =>
            VisualBasicCognitiveComplexityMetric.GetComplexity(node).Complexity;

        public override int GetCyclomaticComplexity(SyntaxNode node) =>
            node.DescendantNodesAndSelf()
                .Count(n =>
                    IsComplexityIncreasingKind(n) ||
                    IsFunction(n));

        protected override bool IsClass(SyntaxNode node)
        {
            switch (node.Kind())
            {
                case SyntaxKind.ClassBlock:
                case SyntaxKind.StructureBlock:
                case SyntaxKind.InterfaceBlock:
                case SyntaxKind.ModuleBlock:
                    return true;

                default:
                    return false;
            }
        }

        protected override bool IsCommentTrivia(SyntaxTrivia trivia) => trivia.IsComment();

        protected override bool IsDocumentationCommentTrivia(SyntaxTrivia trivia)
        {
            switch (trivia.Kind())
            {
                // Contrary to C#, VB.NET seems to always recognize the documentation comments.
                case SyntaxKind.DocumentationCommentExteriorTrivia:
                case SyntaxKind.DocumentationCommentTrivia:
                    return true;

                default:
                    return false;
            }
        }

        protected override bool IsEndOfFile(SyntaxToken token) =>
            token.IsKind(SyntaxKind.EndOfFileToken);

        protected override bool IsFunction(SyntaxNode node)
        {
            if (!FunctionKinds.Contains(node.Kind()) ||
                !MethodBlocks.Contains(node.Parent.Kind()) ||
                node.Parent.Parent.IsKind(SyntaxKind.InterfaceBlock))
            {
                return false;
            }

            if (node is MethodBaseSyntax method && method.Modifiers.Any(SyntaxKind.MustOverrideKeyword))
            {
                return false;
            }

            return true;
        }

        protected override bool IsNoneToken(SyntaxToken token) =>
            token.IsKind(SyntaxKind.None);

        protected override bool IsStatement(SyntaxNode node) =>
            node is ExecutableStatementSyntax;

        private bool IsComplexityIncreasingKind(SyntaxNode node)
        {
            switch (node.Kind())
            {
                case SyntaxKind.IfStatement:
                case SyntaxKind.SingleLineIfStatement:
                case SyntaxKind.TernaryConditionalExpression:
                case SyntaxKind.CaseStatement:

                case SyntaxKind.WhileStatement:
                case SyntaxKind.DoWhileStatement:
                case SyntaxKind.DoUntilStatement:
                case SyntaxKind.SimpleDoStatement:
                case SyntaxKind.ForStatement:
                case SyntaxKind.ForEachStatement:

                case SyntaxKind.ThrowStatement:
                case SyntaxKind.TryStatement:

                case SyntaxKind.ErrorStatement:

                case SyntaxKind.ResumeStatement:
                case SyntaxKind.ResumeNextStatement:
                case SyntaxKind.ResumeLabelStatement:

                case SyntaxKind.OnErrorGoToLabelStatement:
                case SyntaxKind.OnErrorGoToMinusOneStatement:
                case SyntaxKind.OnErrorGoToZeroStatement:
                case SyntaxKind.OnErrorResumeNextStatement:

                case SyntaxKind.GoToStatement:

                case SyntaxKind.ExitDoStatement:
                case SyntaxKind.ExitForStatement:
                case SyntaxKind.ExitFunctionStatement:
                case SyntaxKind.ExitOperatorStatement:
                case SyntaxKind.ExitPropertyStatement:
                case SyntaxKind.ExitSelectStatement:
                case SyntaxKind.ExitSubStatement:
                case SyntaxKind.ExitTryStatement:
                case SyntaxKind.ExitWhileStatement:

                case SyntaxKind.ContinueDoStatement:
                case SyntaxKind.ContinueForStatement:
                case SyntaxKind.ContinueWhileStatement:

                case SyntaxKind.StopStatement:

                case SyntaxKind.AndAlsoExpression:
                case SyntaxKind.OrElseExpression:

                case SyntaxKind.EndStatement:
                    return true;

                default:
                    return false;
            }
        }
    }
}
