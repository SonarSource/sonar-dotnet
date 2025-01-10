/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2014-2025 SonarSource SA
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
    public sealed class MemberInitializedToDefaultCodeFix : SonarCodeFix
    {
        private const string Title = "Remove redundant initializer";

        public override ImmutableArray<string> FixableDiagnosticIds =>
            ImmutableArray.Create(MemberInitializedToDefault.DiagnosticId, MemberInitializerRedundant.DiagnosticId);

        protected override Task RegisterCodeFixesAsync(SyntaxNode root, SonarCodeFixContext context)
        {
            var diagnostic = context.Diagnostics.First();
            var diagnosticSpan = diagnostic.Location.SourceSpan;

            if (!(root.FindNode(diagnosticSpan) is EqualsValueClauseSyntax initializer))
            {
                return Task.CompletedTask;
            }

            context.RegisterCodeFix(
                Title,
                c =>
                {
                    var parent = initializer.Parent;

                    SyntaxNode newParent;

                    if (!(parent is PropertyDeclarationSyntax propDecl))
                    {
                        newParent = parent.RemoveNode(initializer, SyntaxRemoveOptions.KeepNoTrivia);
                    }
                    else
                    {
                        var newPropDecl = propDecl
                            .WithInitializer(null)
                            .WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.None))
                            .WithTriviaFrom(propDecl);

                        newParent = newPropDecl;
                    }

                    var newRoot = root.ReplaceNode(
                        parent,
                        newParent.WithAdditionalAnnotations(Formatter.Annotation));
                    return Task.FromResult(context.Document.WithSyntaxRoot(newRoot));
                },
                context.Diagnostics);

            return Task.CompletedTask;
        }
    }
}
