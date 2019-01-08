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

using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace SonarAnalyzer.Helpers
{
    internal static class CSharpIfDirectiveHelper
    {
        /// <summary>
        /// Returns a list of the names of #if [NAME] sections that the specified
        /// node is contained in.
        /// </summary>
        /// <remarks>
        /// Note: currently we only handle directives with simple identifiers e.g. #if FOO, #elif FOO
        /// We don't handle logical operators e.g. #if !DEBUG, and we don't handle cases like
        /// #if !DEBUG ... #else... :DEBUG must be true in the else case.
        /// </remarks>
        public static IEnumerable<string> GetActiveConditionalCompilationSections(SyntaxNode node)
        {
            var directives = CollectPrecedingDirectiveSyntax(node);

            if (directives.Count == 0)
            {
                return Enumerable.Empty<string>();
            }

            var activeDirectives = new Stack<BranchingDirectiveTriviaSyntax>();

            foreach (var directive in directives)
            {
                switch (directive.RawKind)
                {
                    case (int)SyntaxKind.IfDirectiveTrivia:
                        activeDirectives.Push((BranchingDirectiveTriviaSyntax)directive);
                        break;

                    case (int)SyntaxKind.ElseDirectiveTrivia:
                    case (int)SyntaxKind.ElifDirectiveTrivia:

                        // If we hit an if or elif then that effective acts as an "end" for the previous if/elif block -> pop it
                        SafePop(activeDirectives);

                        activeDirectives.Push((BranchingDirectiveTriviaSyntax)directive);
                        break;

                    case (int)SyntaxKind.EndIfDirectiveTrivia:
                        SafePop(activeDirectives);
                        break;

                    default:
                        Debug.Fail($"Unexpected token type: {directive.Kind()}");
                        break;
                }
            }

            Debug.Assert(activeDirectives.All(a => a.IsActive), "Not all of the collected directives were active");
            Debug.Assert(activeDirectives.All(a => a.BranchTaken), "Not all of the collected directives were for the branch that was taken");

            var activeNames = activeDirectives.Select(FindDirectiveName)
                .WhereNotNull()
                .ToHashSet();

            return activeNames;
        }

        private static string FindDirectiveName(BranchingDirectiveTriviaSyntax directiveTriviaSyntax) =>
            directiveTriviaSyntax is ConditionalDirectiveTriviaSyntax conditionalDirective &&
            conditionalDirective?.Condition is IdentifierNameSyntax identifierName
                ? identifierName.Identifier.ValueText : null;

        private static void SafePop(Stack<BranchingDirectiveTriviaSyntax> stack)
        {
            Debug.Assert(stack.Count > 0, "Not expecting the stack to be empty when we encounter a #end or #elif directive");
            if (stack.Count > 0)
            {
                stack.Pop();
            }
        }

        private static IList<DirectiveTriviaSyntax> CollectPrecedingDirectiveSyntax(SyntaxNode node)
        {
            var walker = new BranchingDirectiveCollector(node);

            return walker.SafeVisit(node.SyntaxTree.GetRoot())
                ? walker.CollectedDirectives
                : new List<DirectiveTriviaSyntax>();
        }

        /// <summary>
        /// Collects all of the #if, #else, #elsif and #endif directives occuring in the
        /// syntax tree up to the specified node
        /// </summary>
        private class BranchingDirectiveCollector : CSharpSyntaxWalker
        {
            private readonly SyntaxNode terminatingNode;
            private bool found;

            public BranchingDirectiveCollector(SyntaxNode terminatingNode)
                : base(SyntaxWalkerDepth.StructuredTrivia)
            {
                this.terminatingNode = terminatingNode;
                this.found = false;
                CollectedDirectives = new List<DirectiveTriviaSyntax>();
            }

            public IList<DirectiveTriviaSyntax> CollectedDirectives { get; }

            public override void Visit(SyntaxNode node)
            {
                // Stop traversing once we've walked down to the terminating node
                if (this.found)
                {
                    return;
                }

                if (node == this.terminatingNode)
                {
                    VisitTerminatingNodeLeadingTrivia();
                    this.found = true;
                    return;
                }

                base.Visit(node);
            }

            private void VisitTerminatingNodeLeadingTrivia()
            {
                // Special case: the leading trivia of the terminating node
                // could contain directives. However, we won't have processed
                // these yet, as they are treated as children of the node
                // even though they appear before it in the text
                if (this.terminatingNode.HasLeadingTrivia)
                {
                    VisitLeadingTrivia(this.terminatingNode.GetFirstToken(includeZeroWidth: true));
                }
            }

            public override void VisitIfDirectiveTrivia(IfDirectiveTriviaSyntax node)
            {
                AddDirective(node);
                base.VisitIfDirectiveTrivia(node);
            }

            public override void VisitElseDirectiveTrivia(ElseDirectiveTriviaSyntax node)
            {
                AddDirective(node);
                base.VisitElseDirectiveTrivia(node);
            }

            public override void VisitElifDirectiveTrivia(ElifDirectiveTriviaSyntax node)
            {
                AddDirective(node);
                base.VisitElifDirectiveTrivia(node);
            }

            public override void VisitEndIfDirectiveTrivia(EndIfDirectiveTriviaSyntax node)
            {
                AddDirective(node);
                base.VisitEndIfDirectiveTrivia(node);
            }

            private void AddDirective(DirectiveTriviaSyntax node)
            {
                if (node.IsActive)
                {
                    CollectedDirectives.Add(node);
                }
            }
        }
    }
}
