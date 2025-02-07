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

namespace SonarAnalyzer.Rules.CSharp
{
    [ExportCodeFixProvider(LanguageNames.CSharp)]
    public sealed class LinkedListPropertiesInsteadOfMethodsCodeFix : SonarCodeFix
    {
        private const string Title = "Replace extension method call with property";

        public override ImmutableArray<string> FixableDiagnosticIds { get; } =
            ImmutableArray.Create(LinkedListPropertiesInsteadOfMethods.DiagnosticId);

        protected override Task RegisterCodeFixesAsync(SyntaxNode root, SonarCodeFixContext context)
        {
            var diagnostic = context.Diagnostics.First();
            var diagnosticSpan = diagnostic.Location.SourceSpan;
            var identifierSyntax = (IdentifierNameSyntax)root.FindNode(diagnosticSpan, getInnermostNodeForTie: true);

            if (identifierSyntax is { Parent: ExpressionSyntax { Parent: InvocationExpressionSyntax invocationExpression } expression })
            {
                var newMember = SyntaxFactory.MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, expression, SyntaxFactory.IdentifierName("Value"));
                context.RegisterCodeFix(
                    Title,
                    _ =>
                    {
                        var newRoot = root.ReplaceNode(invocationExpression, newMember);
                        return Task.FromResult(context.Document.WithSyntaxRoot(newRoot));
                    },
                    context.Diagnostics);
            }
            return Task.CompletedTask;
        }
    }
}
