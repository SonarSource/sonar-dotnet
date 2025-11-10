/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2014-2025 SonarSource Sàrl
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

namespace SonarAnalyzer.CSharp.Rules
{
    [ExportCodeFixProvider(LanguageNames.CSharp)]
    public sealed class FieldShouldBeReadonlyCodeFix : SonarCodeFix
    {
        private const string Title = "Add 'readonly' keyword";

        public override ImmutableArray<string> FixableDiagnosticIds => ImmutableArray.Create(FieldShouldBeReadonly.DiagnosticId);

        protected override Task RegisterCodeFixesAsync(SyntaxNode root, SonarCodeFixContext context)
        {
            var diagnostic = context.Diagnostics.First();
            var diagnosticSpan = diagnostic.Location.SourceSpan;
            var syntaxNode = root.FindNode(diagnosticSpan);

            var variableDeclarator = syntaxNode.FirstAncestorOrSelf<VariableDeclaratorSyntax>();
            if (variableDeclarator?.Parent is not VariableDeclarationSyntax variableDeclaration)
            {
                return Task.CompletedTask;
            }

            if (variableDeclaration.Variables.Count == 1)
            {
                if (variableDeclaration.Parent is not FieldDeclarationSyntax fieldDeclaration)
                {
                    return Task.CompletedTask;
                }

                context.RegisterCodeFix(
                    Title,
                    c =>
                    {
                        var readonlyToken = SyntaxFactory.Token(SyntaxKind.ReadOnlyKeyword);
                        var newFieldDeclaration = HasAnyAccessibilityModifier(fieldDeclaration)
                            ? fieldDeclaration.AddModifiers(readonlyToken)
                            : fieldDeclaration.WithoutLeadingTrivia()
                                    .AddModifiers(readonlyToken.WithLeadingTrivia(fieldDeclaration.GetLeadingTrivia()));

                        var newRoot = root.ReplaceNode(fieldDeclaration, newFieldDeclaration);
                        return Task.FromResult(context.Document.WithSyntaxRoot(newRoot));
                    },
                    context.Diagnostics);
            }

            return Task.CompletedTask;
        }

        private static bool HasAnyAccessibilityModifier(FieldDeclarationSyntax fieldDeclaration) =>
            fieldDeclaration.Modifiers.Any(modifier => modifier.IsAnyKind(SyntaxKind.PrivateKeyword, SyntaxKind.PublicKeyword, SyntaxKind.InternalKeyword, SyntaxKind.ProtectedKeyword));
    }
}
