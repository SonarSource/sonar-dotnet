
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
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using SonarAnalyzer.Helpers;

namespace SonarAnalyzer.Rules.CSharp
{
    [ExportCodeFixProvider(LanguageNames.CSharp)]
    public sealed class CheckFileLicenseCodeFixProvider : SonarCodeFixProvider
    {
        internal const string Title = "Add or update license header";
        public override ImmutableArray<string> FixableDiagnosticIds => ImmutableArray.Create(CheckFileLicense.DiagnosticId);
        public override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

        protected override Task RegisterCodeFixesAsync(SyntaxNode root, CodeFixContext context)
        {
            var diagnostic = context.Diagnostics.First();
            if (!diagnostic.Properties.Any() ||
                !diagnostic.Properties.ContainsKey(CheckFileLicense.IsRegularExpressionPropertyKey) ||
                !diagnostic.Properties.ContainsKey(CheckFileLicense.HeaderFormatPropertyKey))
            {
                return TaskHelper.CompletedTask;
            }

            if (!bool.TryParse(diagnostic.Properties[CheckFileLicense.IsRegularExpressionPropertyKey], out var b) || b)
            {
                return TaskHelper.CompletedTask;
            }

            var diagnosticSpan = diagnostic.Location.SourceSpan;
            var syntaxNode = root.FindNode(diagnosticSpan);

            context.RegisterCodeFix(
                CodeAction.Create(
                    Title,
                    c =>
                    {
                        var fileHeaderTrivias = CreateFileHeaderTrivias(diagnostic.Properties[CheckFileLicense.HeaderFormatPropertyKey]);
                        var newRoot = root.ReplaceNode(syntaxNode, syntaxNode.WithLeadingTrivia(fileHeaderTrivias));
                        return Task.FromResult(context.Document.WithSyntaxRoot(newRoot));
                    }),
                context.Diagnostics);

            return TaskHelper.CompletedTask;
        }

        private static IEnumerable<SyntaxTrivia> CreateFileHeaderTrivias(string comment)
        {
            return new[] { SyntaxFactory.Comment(comment), SyntaxFactory.CarriageReturnLineFeed };
        }
    }
}
