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

using Microsoft.CodeAnalysis.CodeFixes;

namespace SonarAnalyzer.Rules.CSharp
{
    [ExportCodeFixProvider(LanguageNames.CSharp)]
    public sealed class NonFlagsEnumInBitwiseOperationCodeFix : SonarCodeFix
    {
        private const string Title = "Add [Flags] to enum declaration";

        public override ImmutableArray<string> FixableDiagnosticIds => ImmutableArray.Create(NonFlagsEnumInBitwiseOperation.DiagnosticId);

        protected override async Task RegisterCodeFixesAsync(SyntaxNode root, SonarCodeFixContext context)
        {
            var diagnostic = context.Diagnostics.First();
            var diagnosticSpan = diagnostic.Location.SourceSpan;
            var node = root.FindNode(diagnosticSpan, getInnermostNodeForTie: true);

            var semanticModel = await context.Document.GetSemanticModelAsync(context.CancellationToken).ConfigureAwait(false);
            var operation = semanticModel.GetSymbolInfo(node).Symbol as IMethodSymbol;

            if (!(operation?.ReturnType?.DeclaringSyntaxReferences.FirstOrDefault()?.GetSyntax(context.CancellationToken) is EnumDeclarationSyntax enumDeclaration))
            {
                return;
            }

            if (enumDeclaration.AttributeLists.GetAttributes(KnownType.System_FlagsAttribute, semanticModel).Any()) // FixAllProvider already added it from another issue
            {
                return;
            }

            var flagsAttributeType = semanticModel.Compilation.GetTypeByMetadataName(KnownType.System_FlagsAttribute);
            if (flagsAttributeType == null)
            {
                return;
            }

            var currentSolution = context.Document.Project.Solution;
            var documentId = currentSolution.GetDocumentId(enumDeclaration.SyntaxTree);

            if (documentId == null)
            {
                return;
            }

            context.RegisterCodeFix(
                Title,
                async c =>
                {
                    var enumDeclarationRoot = await currentSolution.GetDocument(documentId).GetSyntaxRootAsync(c).ConfigureAwait(false);

                    var flagsAttributeName = flagsAttributeType.ToMinimalDisplayString(semanticModel, enumDeclaration.SpanStart);
                    flagsAttributeName = flagsAttributeName.Remove(flagsAttributeName.IndexOf("Attribute", System.StringComparison.Ordinal));

                    var attributes = enumDeclaration.AttributeLists.Add(
                        SyntaxFactory.AttributeList(SyntaxFactory.SeparatedList(new[] {
                            SyntaxFactory.Attribute(SyntaxFactory.ParseName(flagsAttributeName)) })));

                    var newDeclaration = enumDeclaration.WithAttributeLists(attributes);
                    var newRoot = enumDeclarationRoot.ReplaceNode(
                        enumDeclaration,
                        newDeclaration);
                    return currentSolution.WithDocumentSyntaxRoot(documentId, newRoot);
                },
                context.Diagnostics);
        }
    }
}
