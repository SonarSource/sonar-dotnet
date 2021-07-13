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

using System;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SonarAnalyzer.Common;
using SonarAnalyzer.Helpers;
using StyleCop.Analyzers.Lightup;

namespace SonarAnalyzer.Metrics.CSharp
{
    public class CSharpMetrics : MetricsBase
    {
        private readonly Lazy<ImmutableArray<int>> lazyExecutableLines;

        public override ImmutableArray<int> ExecutableLines =>
            lazyExecutableLines.Value;

        public CSharpMetrics(SyntaxTree tree, SemanticModel semanticModel) : base(tree)
        {
            var root = tree.GetRoot();
            if (root.Language != LanguageNames.CSharp)
            {
                throw new ArgumentException(InitalizationErrorTextPattern, nameof(tree));
            }

            lazyExecutableLines = new Lazy<ImmutableArray<int>>(() => CSharpExecutableLinesMetric.GetLineNumbers(tree, semanticModel));
        }

        public override int GetCognitiveComplexity(SyntaxNode node) =>
            CSharpCognitiveComplexityMetric.GetComplexity(node).Complexity;

        public override int GetCyclomaticComplexity(SyntaxNode node) =>
            CSharpCyclomaticComplexityMetric.GetComplexity(node).Complexity;

        protected override bool IsClass(SyntaxNode node)
        {
            switch (node.Kind())
            {
                case SyntaxKind.ClassDeclaration:
                case SyntaxKindEx.RecordDeclaration:
                case SyntaxKind.StructDeclaration:
                case SyntaxKind.InterfaceDeclaration:
                    return true;

                default:
                    return false;
            }
        }

        protected override bool IsCommentTrivia(SyntaxTrivia trivia) => trivia.IsComment();

        protected override bool IsEndOfFile(SyntaxToken token) =>
            token.IsKind(SyntaxKind.EndOfFileToken);

        protected override bool IsFunction(SyntaxNode node)
        {
            switch (node.Kind())
            {
                case SyntaxKind.PropertyDeclaration:
                    return ((PropertyDeclarationSyntax)node).ExpressionBody != null;

                case SyntaxKind.ConstructorDeclaration:
                case SyntaxKind.ConversionOperatorDeclaration:
                case SyntaxKind.DestructorDeclaration:
                case SyntaxKind.MethodDeclaration:
                case SyntaxKind.OperatorDeclaration:
                    var methodDeclaration = (BaseMethodDeclarationSyntax)node;
                    return methodDeclaration.ExpressionBody() != null
                           || methodDeclaration.Body != null; // Non-abstract, non-interface methods

                case SyntaxKind.AddAccessorDeclaration:
                case SyntaxKind.GetAccessorDeclaration:
                case SyntaxKind.RemoveAccessorDeclaration:
                case SyntaxKind.SetAccessorDeclaration:
                case SyntaxKindEx.InitAccessorDeclaration:
                    var accessor = (AccessorDeclarationSyntax)node;
                    if (accessor.HasBodyOrExpressionBody())
                    {
                        return true;
                    }

                    if (!accessor.Parent.Parent.IsAnyKind(SyntaxKind.PropertyDeclaration, SyntaxKind.EventDeclaration))
                    {
                        // Unexpected
                        return false;
                    }

                    var basePropertyNode = (BasePropertyDeclarationSyntax)accessor.Parent.Parent;

                    if (basePropertyNode.Modifiers.Any(SyntaxKind.AbstractKeyword))
                    {
                        return false;
                    }

                    return !basePropertyNode.Parent.IsKind(SyntaxKind.InterfaceDeclaration);

                default:
                    return false;
            }
        }

        protected override bool IsNoneToken(SyntaxToken token) =>
            token.IsKind(SyntaxKind.None);

        protected override bool IsStatement(SyntaxNode node)
        {
            switch (node.Kind())
            {
                case SyntaxKind.BreakStatement:
                case SyntaxKind.CheckedStatement:
                case SyntaxKind.ContinueStatement:
                case SyntaxKind.DoStatement:
                case SyntaxKind.EmptyStatement:
                case SyntaxKind.ExpressionStatement:
                case SyntaxKind.FixedStatement:
                case SyntaxKind.ForEachStatement:
                case SyntaxKind.ForStatement:
                case SyntaxKind.GlobalStatement:
                case SyntaxKind.GotoCaseStatement:
                case SyntaxKind.GotoDefaultStatement:
                case SyntaxKind.GotoStatement:
                case SyntaxKind.IfStatement:
                case SyntaxKind.LabeledStatement:
                case SyntaxKind.LocalDeclarationStatement:
                case SyntaxKind.LockStatement:
                case SyntaxKind.ReturnStatement:
                case SyntaxKind.SwitchStatement:
                case SyntaxKind.ThrowStatement:
                case SyntaxKind.TryStatement:
                case SyntaxKind.UncheckedStatement:
                case SyntaxKind.UnsafeStatement:
                case SyntaxKind.UsingStatement:
                case SyntaxKind.WhileStatement:
                case SyntaxKind.YieldBreakStatement:
                case SyntaxKind.YieldReturnStatement:
                    return true;

                default:
                    return false;
            }
        }
    }
}
