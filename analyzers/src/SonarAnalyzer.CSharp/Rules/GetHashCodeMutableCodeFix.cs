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

using Microsoft.CodeAnalysis.Editing;

namespace SonarAnalyzer.CSharp.Rules
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
                .GetSemanticModelAsync(context.Cancel)
                .ConfigureAwait(false);
            var allFieldDeclarationTasks = identifiersToFix.Select(identifier =>
                GetFieldDeclarationSyntaxAsync(semanticModel, identifier, context.Cancel));
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
