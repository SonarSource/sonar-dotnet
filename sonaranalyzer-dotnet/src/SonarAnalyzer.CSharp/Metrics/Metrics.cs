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

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SonarAnalyzer.Common;
using SonarAnalyzer.Helpers;
using SonarAnalyzer.ShimLayer.CSharp;

namespace SonarAnalyzer.Metrics.CSharp
{
    public class Metrics : MetricsBase
    {
        private readonly SemanticModel semanticModel;

        public Metrics(SyntaxTree tree, SemanticModel semanticModel)
            : base(tree)
        {
            var root = tree.GetRoot();
            if (root.Language != LanguageNames.CSharp)
            {
                throw new ArgumentException(InitalizationErrorTextPattern, nameof(tree));
            }

            this.semanticModel = semanticModel;
        }

        public override ICollection<int> ExecutableLines
        {
            get
            {
                var walker = new ExecutableLinesWalker(this.semanticModel);
                walker.Visit(this.tree.GetRoot());
                return walker.ExecutableLines;
            }
        }

        protected override bool IsEndOfFile(SyntaxToken token) =>
            token.IsKind(SyntaxKind.EndOfFileToken);

        protected override bool IsNoneToken(SyntaxToken token) =>
            token.IsKind(SyntaxKind.None);

        protected override bool IsCommentTrivia(SyntaxTrivia trivia)
        {
            switch (trivia.Kind())
            {
                case SyntaxKind.SingleLineCommentTrivia:
                case SyntaxKind.MultiLineCommentTrivia:
                case SyntaxKind.SingleLineDocumentationCommentTrivia:
                case SyntaxKind.MultiLineDocumentationCommentTrivia:
                    return true;

                default:
                    return false;
            }
        }

        protected override bool IsClass(SyntaxNode node)
        {
            switch (node.Kind())
            {
                case SyntaxKind.ClassDeclaration:
                case SyntaxKind.StructDeclaration:
                case SyntaxKind.InterfaceDeclaration:
                    return true;

                default:
                    return false;
            }
        }

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
                    return methodDeclaration.ExpressionBody() != null ||
                        methodDeclaration.Body != null; // Non-abstract, non-interface methods

                case SyntaxKind.AddAccessorDeclaration:
                case SyntaxKind.GetAccessorDeclaration:
                case SyntaxKind.RemoveAccessorDeclaration:
                case SyntaxKind.SetAccessorDeclaration:
                    var accessor = (AccessorDeclarationSyntax)node;
                    if (accessor.HasBodyOrExpressionBody())
                    {
                        return true;
                    }

                    if (!accessor.Parent.Parent.IsKind(SyntaxKind.PropertyDeclaration) &&
                        !accessor.Parent.Parent.IsKind(SyntaxKind.EventDeclaration))
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

        protected override IEnumerable<SyntaxNode> PublicApiNodes
        {
            get
            {
                var root = this.tree.GetRoot();
                var publicNodes = ImmutableArray.CreateBuilder<SyntaxNode>();
                var toVisit = new Stack<SyntaxNode>();

                var members = root.ChildNodes()
                    .Where(childNode => childNode is MemberDeclarationSyntax);
                foreach (var member in members)
                {
                    toVisit.Push(member);
                }

                while (toVisit.Any())
                {
                    var member = toVisit.Pop();

                    var isPublic = member.ChildTokens().AnyOfKind(SyntaxKind.PublicKeyword);
                    if (isPublic)
                    {
                        publicNodes.Add(member);
                    }

                    if (!isPublic &&
                        !member.IsKind(SyntaxKind.NamespaceDeclaration))
                    {
                        continue;
                    }

                    members = member.ChildNodes()
                        .Where(childNode => childNode is MemberDeclarationSyntax);
                    foreach (var child in members)
                    {
                        toVisit.Push(child);
                    }
                }

                return publicNodes.ToImmutable();
            }
        }

        public override int GetComplexity(SyntaxNode node)
        {
            var walker = new CyclomaticComplexityWalker();
            walker.Walk(node);
            return walker.CyclomaticComplexity;
        }

        public override int GetCognitiveComplexity(SyntaxNode node)
        {
            var walker = new CognitiveComplexityWalker();
            walker.Walk(node);
            // nesting level should be 0 at the end of the analysis, otherwise there is a bug
            return walker.NestingLevel == 0 ? walker.Complexity : -1;
        }
    }
}
