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
        private static readonly ISet<SyntaxKind> TriviaKinds =
            new HashSet<SyntaxKind>
            {
                SyntaxKind.SingleLineCommentTrivia,
                SyntaxKind.MultiLineCommentTrivia,
                SyntaxKind.SingleLineDocumentationCommentTrivia,
                SyntaxKind.MultiLineDocumentationCommentTrivia
            };

        private static readonly ISet<SyntaxKind> ClassKinds =
            new HashSet<SyntaxKind>
            {
                SyntaxKind.ClassDeclaration,
                SyntaxKind.StructDeclaration,
                SyntaxKind.InterfaceDeclaration
            };

        private static readonly ISet<SyntaxKind> FunctionKinds = new
            HashSet<SyntaxKind>
            {
                SyntaxKind.ConstructorDeclaration,
                SyntaxKind.DestructorDeclaration,
                SyntaxKind.MethodDeclaration,
                SyntaxKind.OperatorDeclaration
            };

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

        protected override bool IsEndOfFile(SyntaxToken token) => token.IsKind(SyntaxKind.EndOfFileToken);

        protected override bool IsNoneToken(SyntaxToken token) => token.IsKind(SyntaxKind.None);

        protected override bool IsCommentTrivia(SyntaxTrivia trivia) => TriviaKinds.Contains(trivia.Kind());

        protected override bool IsClass(SyntaxNode node) => ClassKinds.Contains(node.Kind());

        protected override bool IsStatement(SyntaxNode node) => node is StatementSyntax &&
            !node.IsKind(SyntaxKind.Block);

        protected override bool IsFunction(SyntaxNode node)
        {
            if (node is PropertyDeclarationSyntax property && property.ExpressionBody != null)
            {
                return true;
            }

            if (node is BaseMethodDeclarationSyntax method && method.ExpressionBody() != null)
            {
                return true;
            }

            if (FunctionKinds.Contains(node.Kind()) &&
                node.ChildNodes().AnyOfKind(SyntaxKind.Block))
            {
                // Non-abstract, non-interface methods
                return true;
            }

            if (node is AccessorDeclarationSyntax accessor)
            {
                if (accessor.HasBodyOrExpressionBody())
                {
                    return true;
                }

                if (!(accessor.Parent.Parent is BasePropertyDeclarationSyntax prop))
                {
                    // Unexpected
                    return false;
                }

                if (prop.Modifiers.Any(SyntaxKind.AbstractKeyword))
                {
                    return false;
                }

                return !(prop.Parent is InterfaceDeclarationSyntax);
            }

            return false;
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
            return walker.VisitEndedCorrectly ? walker.Complexity : -1;
        }
    }
}
