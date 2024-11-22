/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2014-2024 SonarSource SA
 * mailto:info AT sonarsource DOT com
 * This program is free software; you can redistribute it and/or
 * modify it under the terms of the Sonar Source-Available License Version 1, as published by SonarSource SA.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
 * See the Sonar Source-Available License for more details.
 *
 * You should have received a copy of the Sonar Source-Available License
 * along with this program; if not, see https://sonarsource.com/license/ssal/
 */

using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Formatting;

namespace SonarAnalyzer.Rules.CSharp
{
    [ExportCodeFixProvider(LanguageNames.CSharp)]
    public sealed class RedundantInheritanceListCodeFix : SonarCodeFix
    {
        private const string Title = "Remove redundant declaration";
        public override ImmutableArray<string> FixableDiagnosticIds => ImmutableArray.Create(RedundantInheritanceList.DiagnosticId);

        protected override Task RegisterCodeFixesAsync(SyntaxNode root, SonarCodeFixContext context)
        {
            var diagnostic = context.Diagnostics.First();
            var diagnosticSpan = diagnostic.Location.SourceSpan;
            var baseList = (BaseListSyntax)root.FindNode(diagnosticSpan);
            var redundantIndex = int.Parse(diagnostic.Properties[RedundantInheritanceList.RedundantIndexKey]);

            context.RegisterCodeFix(
                Title,
                c =>
                {
                    var newRoot = RemoveDeclaration(root, baseList, redundantIndex);
                    return Task.FromResult(context.Document.WithSyntaxRoot(newRoot));
                },
                context.Diagnostics);

            return Task.CompletedTask;
        }

        internal static bool HasLineEnding(SyntaxNode node) =>
            node.HasTrailingTrivia
            && node.GetTrailingTrivia().Last().IsKind(SyntaxKind.EndOfLineTrivia);

        private static bool HasLineEnding(SyntaxToken token) =>
            token.HasTrailingTrivia
            && token.TrailingTrivia.Last().IsKind(SyntaxKind.EndOfLineTrivia);

        private static SyntaxNode RemoveDeclaration(SyntaxNode root, BaseListSyntax baseList, int redundantIndex)
        {
            var newBaseList = baseList.RemoveNode(baseList.Types[redundantIndex], SyntaxRemoveOptions.KeepNoTrivia)
                                      .WithAdditionalAnnotations(Formatter.Annotation);

            if (newBaseList.Types.Count != 0)
            {
                return root.ReplaceNode(baseList, newBaseList);
            }

            var baseTypeHadLineEnding = HasLineEnding(baseList.Types[redundantIndex]);
            var colonHadLineEnding = HasLineEnding(baseList.ColonToken);
            var typeNameHadLineEnding = HasLineEnding(((BaseTypeDeclarationSyntax)baseList.Parent).Identifier);

            var annotation = new SyntaxAnnotation();
            var newRoot = root.ReplaceNode(baseList.Parent, baseList.Parent.WithAdditionalAnnotations(annotation));
            var declaration = (BaseTypeDeclarationSyntax)newRoot.GetAnnotatedNodes(annotation).First();

            newRoot = newRoot.RemoveNode(declaration.BaseList, SyntaxRemoveOptions.KeepNoTrivia);
            declaration = (BaseTypeDeclarationSyntax)newRoot.GetAnnotatedNodes(annotation).First();

            var needsNewLine = !typeNameHadLineEnding && (colonHadLineEnding || baseTypeHadLineEnding);

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

                newRoot = newRoot.ReplaceToken(declaration.Identifier, declaration.Identifier.WithTrailingTrivia(trivia));
            }

            declaration = (BaseTypeDeclarationSyntax)newRoot.GetAnnotatedNodes(annotation).First();
            return newRoot.ReplaceNode(declaration, declaration.WithoutAnnotations(annotation));
        }
    }
}
