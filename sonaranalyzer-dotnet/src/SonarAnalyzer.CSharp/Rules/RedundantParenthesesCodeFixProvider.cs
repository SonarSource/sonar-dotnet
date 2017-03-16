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

using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.CodeAnalysis.CodeActions;
using SonarAnalyzer.Helpers;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace SonarAnalyzer.Rules.CSharp
{
    [ExportCodeFixProvider(LanguageNames.CSharp)]
    public class RedundantParenthesesCodeFixProvider : SonarCodeFixProvider
    {
        internal const string Title = "Remove redundant parentheses";
        public sealed override ImmutableArray<string> FixableDiagnosticIds
        {
            get
            {
                return ImmutableArray.Create(RedundantParentheses.DiagnosticId);
            }
        }

        public sealed override FixAllProvider GetFixAllProvider()
        {
            return WellKnownFixAllProviders.BatchFixer;
        }

        protected sealed override Task RegisterCodeFixesAsync(SyntaxNode root, CodeFixContext context)
        {
            var diagnostic = context.Diagnostics.First();
            var diagnosticSpan = diagnostic.Location.SourceSpan;
            var syntaxNode = root.FindNode(diagnosticSpan);

            if (syntaxNode is ArgumentListSyntax ||
                syntaxNode is AttributeArgumentListSyntax)
            {
                context.RegisterCodeFix(
                    CodeAction.Create(
                        Title,
                        c =>
                        {
                            var newRoot = root.RemoveNode(syntaxNode, SyntaxRemoveOptions.KeepExteriorTrivia | SyntaxRemoveOptions.KeepEndOfLine);
                            return Task.FromResult(context.Document.WithSyntaxRoot(newRoot));
                        }),
                    context.Diagnostics);
            }
            else
            {
                // Do nothing, we don't want to mess the code if we don't find what we expect
            }

            return TaskHelper.CompletedTask;
        }
    }
}

