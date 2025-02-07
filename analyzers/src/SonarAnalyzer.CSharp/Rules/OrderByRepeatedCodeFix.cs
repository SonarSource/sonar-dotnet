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
    public sealed class OrderByRepeatedCodeFix : SonarCodeFix
    {
        internal const string Title = "Change 'OrderBy' to 'ThenBy'";
        public override ImmutableArray<string> FixableDiagnosticIds => ImmutableArray.Create(OrderByRepeated.DiagnosticId);

        protected override Task RegisterCodeFixesAsync(SyntaxNode root, SonarCodeFixContext context)
        {
            var diagnostic = context.Diagnostics.First();
            var diagnosticSpan = diagnostic.Location.SourceSpan;
            var syntaxNode = root.FindNode(diagnosticSpan);

            context.RegisterCodeFix(
                Title,
                c => ChangeToThenByAsync(context.Document, syntaxNode, c),
                context.Diagnostics);

            return Task.CompletedTask;
        }

        private static async Task<Document> ChangeToThenByAsync(Document document, SyntaxNode syntaxNode, CancellationToken cancel)
        {
            var root = await document.GetSyntaxRootAsync(cancel).ConfigureAwait(false);
            var newRoot = root.ReplaceNode(syntaxNode,
                SyntaxFactory.IdentifierName("ThenBy"));
            return document.WithSyntaxRoot(newRoot);
        }
    }
}
