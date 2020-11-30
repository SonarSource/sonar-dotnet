/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2020 SonarSource SA
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

using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SonarAnalyzer.Helpers;

namespace SonarAnalyzer.Rules.CSharp
{
    [ExportCodeFixProvider(LanguageNames.CSharp)]
    public sealed class NonFlagsEnumInBitwiseOperationCodeFixProvider : SonarCodeFixProvider
    {
        private const string Title = "Add [Flags] to enum declaration";

        public override ImmutableArray<string> FixableDiagnosticIds => ImmutableArray.Create(NonFlagsEnumInBitwiseOperation.DiagnosticId);

        public override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

        protected override async Task RegisterCodeFixesAsync(SyntaxNode root, CodeFixContext context)
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

            var flagsAttributeType = semanticModel.Compilation.GetTypeByMetadataName(KnownType.System_FlagsAttribute.TypeName);
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
                CodeAction.Create(
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
                    }),
                context.Diagnostics);
        }
    }
}
