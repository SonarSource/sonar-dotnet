/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2024 SonarSource SA
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

using Microsoft.CodeAnalysis.CodeFixes;

namespace SonarAnalyzer.Rules.CSharp
{
    [ExportCodeFixProvider(LanguageNames.CSharp)]
    public sealed class LiteralSuffixUpperCaseCodeFix : SonarCodeFix
    {
        private const string Title = "Make literal suffix upper case";
        private const string LowercaseEllSuffix = "CS0078";     // The 'l' suffix is easily confused with the digit '1' -- use 'L' for clarity: 25l -> 25L

        public override ImmutableArray<string> FixableDiagnosticIds
        {
            get
            {
                return ImmutableArray.Create(LiteralSuffixUpperCase.DiagnosticId, LowercaseEllSuffix);
            }
        }

        protected override Task RegisterCodeFixesAsync(SyntaxNode root, SonarCodeFixContext context)
        {
            var diagnostic = context.Diagnostics.First();
            var diagnosticSpan = diagnostic.Location.SourceSpan;
            if (!(root.FindNode(diagnosticSpan, getInnermostNodeForTie: true) is LiteralExpressionSyntax literal))
            {
                return Task.CompletedTask;
            }

            var newLiteral = SyntaxFactory.Literal(
                literal.Token.Text.ToUpperInvariant(),
                (long)literal.Token.Value);

            if (!newLiteral.IsKind(SyntaxKind.None))
            {
                context.RegisterCodeFix(
                    Title,
                    c =>
                    {
                        var newRoot = root.ReplaceNode(literal,
                            literal.WithToken(newLiteral).WithTriviaFrom(literal));
                        return Task.FromResult(context.Document.WithSyntaxRoot(newRoot));
                    },
                    context.Diagnostics);
            }

            return Task.CompletedTask;
        }
    }
}
