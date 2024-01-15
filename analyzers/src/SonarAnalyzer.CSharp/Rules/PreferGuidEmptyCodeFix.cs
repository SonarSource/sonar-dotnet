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
using Microsoft.CodeAnalysis.Formatting;

namespace SonarAnalyzer.Rules.CSharp
{
    [ExportCodeFixProvider(LanguageNames.CSharp)]
    public sealed class PreferGuidEmptyCodeFix : SonarCodeFix
    {
        internal const string Title = "Use Guid.Empty instead";

        public override ImmutableArray<string> FixableDiagnosticIds => ImmutableArray.Create(PreferGuidEmpty.DiagnosticId);

        protected override Task RegisterCodeFixesAsync(SyntaxNode root, SonarCodeFixContext context)
        {
            var toReplace = ToReplace(root, context);
            var replacement = Replacement(root, toReplace);

            context.RegisterCodeFix(
               Title,
               token => Task.FromResult(context.Document.WithSyntaxRoot(replacement)),
               context.Diagnostics);
            return Task.CompletedTask;
        }

        private static SyntaxNode ToReplace(SyntaxNode root, SonarCodeFixContext context)
        {
            var replacement = root.FindNode(context.Diagnostics.First().Location.SourceSpan);
            return replacement is ArgumentSyntax argument
                ? argument.Expression
                : replacement;
        }

        private static SyntaxNode Replacement(SyntaxNode root, SyntaxNode toReplace) =>
            root.ReplaceNode(toReplace, SyntaxFactory.IdentifierName("Guid.Empty")).WithAdditionalAnnotations(Formatter.Annotation);
    }
}
