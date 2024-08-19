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
using Microsoft.CodeAnalysis.Editing;

namespace SonarAnalyzer.Rules.CSharp
{
    [ExportCodeFixProvider(LanguageNames.CSharp)]
    public sealed class GetHashCodeMutableCodeFix : SonarCodeFix
    {
        internal const string Title = "Make field 'readonly'";

        public override ImmutableArray<string> FixableDiagnosticIds => ImmutableArray.Create(GetHashCodeMutable.DiagnosticId);

        protected  override async Task RegisterCodeFixesAsync(SyntaxNode root, SonarCodeFixContext context)
        {
            var diagnostic = context.Diagnostics.First();

            var identifiersToFix = diagnostic.AdditionalLocations
                .Select(location => location.SourceSpan)
                .Select(diagnosticSpan => root.FindNode(diagnosticSpan, getInnermostNodeForTie: true) as IdentifierNameSyntax)
                .WhereNotNull()
                .ToList();

            if (identifiersToFix.Count == 0)
            {
                return;
            }

            var semanticModel = await context.Document
                .GetSemanticModelAsync(context.CancellationToken)
                .ConfigureAwait(false);
            var allFieldDeclarationTasks = identifiersToFix.Select(identifier =>
                GetFieldDeclarationSyntaxAsync(semanticModel, identifier, context.CancellationToken));
            var allFieldDeclarations = await Task.WhenAll(allFieldDeclarationTasks).ConfigureAwait(false);
            allFieldDeclarations = allFieldDeclarations.WhereNotNull().ToArray();

            context.RegisterCodeFix(
                Title,
                c => AddReadonlyToFieldDeclarationsAsync(context.Document, c, allFieldDeclarations),
                context.Diagnostics);
        }

        private static async Task<FieldDeclarationSyntax> GetFieldDeclarationSyntaxAsync(SemanticModel semanticModel, IdentifierNameSyntax identifierName, CancellationToken cancel)
        {
            if (!(semanticModel.GetSymbolInfo(identifierName).Symbol is IFieldSymbol fieldSymbol) ||
                !fieldSymbol.DeclaringSyntaxReferences.Any())
            {
                return null;
            }

            var reference = await fieldSymbol.DeclaringSyntaxReferences.First()
                .GetSyntaxAsync(cancel)
                .ConfigureAwait(false);
            var fieldDeclaration = (FieldDeclarationSyntax)reference.Parent.Parent;

            if (fieldDeclaration.Declaration.Variables.Count != 1)
            {
                return null;
            }

            return fieldDeclaration;
        }

        private static async Task<Document> AddReadonlyToFieldDeclarationsAsync(Document document, CancellationToken cancel, IEnumerable<FieldDeclarationSyntax> fieldDeclarations)
        {
            var editor = await DocumentEditor.CreateAsync(document, cancel);

            foreach (var fieldDeclaration in fieldDeclarations)
            {
                var readonlyToken = SyntaxFactory.Token(SyntaxKind.ReadOnlyKeyword).WithTrailingTrivia(SyntaxFactory.Space);
                var newFieldDeclaration = fieldDeclaration.AddModifiers(readonlyToken);
                editor.ReplaceNode(fieldDeclaration, newFieldDeclaration);
            }

            return editor.GetChangedDocument();
        }
    }
}
