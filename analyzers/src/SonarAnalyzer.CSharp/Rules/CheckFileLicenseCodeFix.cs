/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2014-2025 SonarSource Sàrl
 * mailto:info AT sonarsource DOT com
 * This program is free software; you can redistribute it and/or
 * modify it under the terms of the Sonar Source-Available License Version 1, as published by SonarSource Sàrl.
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
    public sealed class CheckFileLicenseCodeFix : SonarCodeFix
    {
        internal const string Title = "Add or update license header";
        public override ImmutableArray<string> FixableDiagnosticIds => ImmutableArray.Create(CheckFileLicense.DiagnosticId);

        protected override Task RegisterCodeFixesAsync(SyntaxNode root, SonarCodeFixContext context)
        {
            var diagnostic = context.Diagnostics.First();
            if (!diagnostic.Properties.Any() ||
                !diagnostic.Properties.ContainsKey(CheckFileLicense.IsRegularExpressionPropertyKey) ||
                !diagnostic.Properties.ContainsKey(CheckFileLicense.HeaderFormatPropertyKey))
            {
                return Task.CompletedTask;
            }

            if (!bool.TryParse(diagnostic.Properties[CheckFileLicense.IsRegularExpressionPropertyKey], out var b) || b)
            {
                return Task.CompletedTask;
            }

            var diagnosticSpan = diagnostic.Location.SourceSpan;
            var syntaxNode = root.FindNode(diagnosticSpan);

            context.RegisterCodeFix(
                Title,
                c =>
                {
                    var fileHeaderTrivias = CreateFileHeaderTrivias(diagnostic.Properties[CheckFileLicense.HeaderFormatPropertyKey]);
                    var newRoot = root.ReplaceNode(syntaxNode, syntaxNode.WithLeadingTrivia(fileHeaderTrivias));
                    return Task.FromResult(context.Document.WithSyntaxRoot(newRoot));
                },
                context.Diagnostics);

            return Task.CompletedTask;
        }

        private static IEnumerable<SyntaxTrivia> CreateFileHeaderTrivias(string comment)
        {
            return new[] { SyntaxFactory.Comment(comment), SyntaxFactory.CarriageReturnLineFeed };
        }
    }
}
