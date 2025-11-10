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

namespace SonarAnalyzer.VisualBasic.Rules
{
    [ExportCodeFixProvider(LanguageNames.VisualBasic)]
    public sealed class StringConcatenationWithPlusCodeFix : SonarCodeFix
    {
        internal const string Title = "Change to '&'";

        public override ImmutableArray<string> FixableDiagnosticIds => ImmutableArray.Create(StringConcatenationWithPlus.DiagnosticId);

        protected override Task RegisterCodeFixesAsync(SyntaxNode root, SonarCodeFixContext context)
        {
            var diagnostic = context.Diagnostics.First();
            var diagnosticSpan = diagnostic.Location.SourceSpan;

            if (root.FindNode(diagnosticSpan, getInnermostNodeForTie: true) is BinaryExpressionSyntax binary)
            {
                context.RegisterCodeFix(
                    Title,
                    c =>
                    {
                        var newRoot = CalculateNewRoot(root, binary);
                        return Task.FromResult(context.Document.WithSyntaxRoot(newRoot));
                    },
                    context.Diagnostics);
            }

            return Task.CompletedTask;
        }

        private static SyntaxNode CalculateNewRoot(SyntaxNode root, BinaryExpressionSyntax currentAsBinary)
        {
            return root.ReplaceNode(currentAsBinary,
                SyntaxFactory.ConcatenateExpression(
                    currentAsBinary.Left,
                    SyntaxFactory.Token(SyntaxKind.AmpersandToken).WithTriviaFrom(currentAsBinary.OperatorToken),
                    currentAsBinary.Right));
        }
    }
}
