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

using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Formatting;
using SonarAnalyzer.Helpers;

namespace SonarAnalyzer.Rules.CSharp
{
    [ExportCodeFixProvider(LanguageNames.CSharp)]
    public sealed class RedundantInheritanceListCodeFixProvider : SonarCodeFixProvider
    {
        internal const string Title = "Remove redundant declaration";
        public override ImmutableArray<string> FixableDiagnosticIds
        {
            get
            {
                return ImmutableArray.Create(RedundantInheritanceList.DiagnosticId);
            }
        }
        public override FixAllProvider GetFixAllProvider()
        {
            return WellKnownFixAllProviders.BatchFixer;
        }

        protected override Task RegisterCodeFixesAsync(SyntaxNode root, CodeFixContext context)
        {
            var diagnostic = context.Diagnostics.First();
            var diagnosticSpan = diagnostic.Location.SourceSpan;
            var baseList = (BaseListSyntax)root.FindNode(diagnosticSpan);
            var redundantIndex = int.Parse(diagnostic.Properties[RedundantInheritanceList.RedundantIndexKey]);

            context.RegisterCodeFix(
                CodeAction.Create(
                    Title,
                    c =>
                    {
                        var newRoot = RemoveDeclaration(root, baseList, redundantIndex);
                        return Task.FromResult(context.Document.WithSyntaxRoot(newRoot));
                    }),
                context.Diagnostics);

            return TaskHelper.CompletedTask;
        }

        private static SyntaxNode RemoveDeclaration(SyntaxNode root, BaseListSyntax baseList,
            int redundantIndex)
        {
            var newBaseList = baseList
                .RemoveNode(baseList.Types[redundantIndex], SyntaxRemoveOptions.KeepNoTrivia)
                .WithAdditionalAnnotations(Formatter.Annotation);

            if (newBaseList.Types.Count != 0)
            {
                return root.ReplaceNode(baseList, newBaseList);
            }

            var baseTypeHadLineEnding = HasLineEnding(baseList.Types[redundantIndex]);
            var colonHadLineEnding = HasLineEnding(baseList.ColonToken);
            var typeNameHadLineEnding = HasLineEnding(((BaseTypeDeclarationSyntax)baseList.Parent).Identifier);

            var annotation = new SyntaxAnnotation();
            var newRoot = root.ReplaceNode(
                baseList.Parent,
                baseList.Parent.WithAdditionalAnnotations(annotation));
            var declaration = (BaseTypeDeclarationSyntax)newRoot.GetAnnotatedNodes(annotation).First();

            newRoot = newRoot.RemoveNode(declaration.BaseList, SyntaxRemoveOptions.KeepNoTrivia);
            declaration = (BaseTypeDeclarationSyntax)newRoot.GetAnnotatedNodes(annotation).First();

            var needsNewLine = !typeNameHadLineEnding &&
                (colonHadLineEnding || baseTypeHadLineEnding);

            if (needsNewLine)
            {
                var trivia = SyntaxFactory.TriviaList();
                if (declaration.Identifier.HasTrailingTrivia)
                {
                    trivia = declaration.Identifier.TrailingTrivia;
                }

                trivia = colonHadLineEnding
                    ? trivia.Add(baseList.ColonToken.TrailingTrivia.Last())
                    : trivia.AddRange(baseList.Types[redundantIndex].GetTrailingTrivia());

                newRoot = newRoot.ReplaceToken(
                        declaration.Identifier,
                        declaration.Identifier
                            .WithTrailingTrivia(trivia));
            }

            declaration = (BaseTypeDeclarationSyntax)newRoot.GetAnnotatedNodes(annotation).First();
            return newRoot.ReplaceNode(
                declaration,
                declaration.WithoutAnnotations(annotation));
        }

        internal static bool HasLineEnding(SyntaxNode node)
        {
            return node.HasTrailingTrivia &&
                node.GetTrailingTrivia().Last().IsKind(SyntaxKind.EndOfLineTrivia);
        }
        private static bool HasLineEnding(SyntaxToken token)
        {
            return token.HasTrailingTrivia &&
                token.TrailingTrivia.Last().IsKind(SyntaxKind.EndOfLineTrivia);
        }
    }
}

