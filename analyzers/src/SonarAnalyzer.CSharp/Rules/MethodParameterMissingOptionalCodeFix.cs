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
    public sealed class MethodParameterMissingOptionalCodeFix : SonarCodeFix
    {
        private const string Title = "Add missing 'Optional' attribute";

        public override ImmutableArray<string> FixableDiagnosticIds => ImmutableArray.Create(MethodParameterMissingOptional.DiagnosticId);

        protected override async Task RegisterCodeFixesAsync(SyntaxNode root, SonarCodeFixContext context)
        {
            var diagnostic = context.Diagnostics.First();
            var diagnosticSpan = diagnostic.Location.SourceSpan;
            var attribute = root.FindNode(diagnosticSpan, getInnermostNodeForTie: true) as AttributeSyntax;
            var attributeList = attribute?.Parent as AttributeListSyntax;
            if (attribute == null ||
                attributeList == null)
            {
                return;
            }

            var semanticModel = await context.Document.GetSemanticModelAsync().ConfigureAwait(false);
            var optionalAttribute = semanticModel?.Compilation.GetTypeByMetadataName(KnownType.System_Runtime_InteropServices_OptionalAttribute);
            if (optionalAttribute == null)
            {
                return;
            }

            context.RegisterCodeFix(
                Title,
                _ =>
                {
                    var newRoot = root.ReplaceNode(attributeList, GetNewAttributeList(attributeList, optionalAttribute, semanticModel));
                    return Task.FromResult(context.Document.WithSyntaxRoot(newRoot));
                },
                context.Diagnostics);
        }

        private static AttributeListSyntax GetNewAttributeList(AttributeListSyntax attributeList, ISymbol optionalAttribute, SemanticModel semanticModel)
        {
            var attributeName = optionalAttribute.ToMinimalDisplayString(semanticModel, attributeList.SpanStart);
            attributeName = attributeName.Remove(attributeName.IndexOf("Attribute", System.StringComparison.Ordinal));

            return attributeList
                .AddAttributes(SyntaxFactory.Attribute(SyntaxFactory.ParseName(attributeName)))
                .WithAdditionalAnnotations(Formatter.Annotation);
        }
    }
}
