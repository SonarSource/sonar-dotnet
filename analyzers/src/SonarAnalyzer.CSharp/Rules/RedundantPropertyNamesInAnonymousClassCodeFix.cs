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
    public sealed class RedundantPropertyNamesInAnonymousClassCodeFix : SonarCodeFix
    {
        internal const string Title = "Remove redundant explicit property names";
        public override ImmutableArray<string> FixableDiagnosticIds => ImmutableArray.Create(RedundantPropertyNamesInAnonymousClass.DiagnosticId);

        protected override Task RegisterCodeFixesAsync(SyntaxNode root, SonarCodeFixContext context)
        {
            var diagnostic = context.Diagnostics.First();
            var diagnosticSpan = diagnostic.Location.SourceSpan;
            var nameEquals = root.FindNode(diagnosticSpan) as NameEqualsSyntax;
            if (!(nameEquals?.Parent?.Parent is AnonymousObjectCreationExpressionSyntax anonymousObjectCreation))
            {
                return Task.CompletedTask;
            }

            context.RegisterCodeFix(
                Title,
                c =>
                {
                    var newInitializersWithSeparators = anonymousObjectCreation.Initializers.GetWithSeparators()
                        .Select(item => GetNewSyntaxListItem(item));
                    var newAnonymousObjectCreation = anonymousObjectCreation
                        .WithInitializers(SyntaxFactory.SeparatedList<AnonymousObjectMemberDeclaratorSyntax>(newInitializersWithSeparators))
                        .WithTriviaFrom(anonymousObjectCreation);

                    var newRoot = root.ReplaceNode(
                        anonymousObjectCreation,
                        newAnonymousObjectCreation);
                    return Task.FromResult(context.Document.WithSyntaxRoot(newRoot));
                },
                context.Diagnostics);

            return Task.CompletedTask;
        }

        private static SyntaxNodeOrToken GetNewSyntaxListItem(SyntaxNodeOrToken item)
        {
            if (!item.IsNode)
            {
                return item;
            }

            var member = (AnonymousObjectMemberDeclaratorSyntax)item.AsNode();
            if (member.Expression is IdentifierNameSyntax identifier &&
                identifier.Identifier.ValueText == member.NameEquals.Name.Identifier.ValueText)
            {
                return SyntaxFactory.AnonymousObjectMemberDeclarator(member.Expression).WithTriviaFrom(member);
            }

            return item;
        }
    }
}
