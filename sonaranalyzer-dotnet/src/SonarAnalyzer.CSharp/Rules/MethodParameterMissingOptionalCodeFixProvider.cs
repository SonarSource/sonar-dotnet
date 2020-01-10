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
using Microsoft.CodeAnalysis.Formatting;
using SonarAnalyzer.Common;
using SonarAnalyzer.Helpers;

namespace SonarAnalyzer.Rules.CSharp
{
    [ExportCodeFixProvider(LanguageNames.CSharp)]
    public sealed class MethodParameterMissingOptionalCodeFixProvider : SonarCodeFixProvider
    {
        private const string Title = "Add missing 'Optional' attribute";

        public override ImmutableArray<string> FixableDiagnosticIds => ImmutableArray.Create(MethodParameterMissingOptional.DiagnosticId);

        public override FixAllProvider GetFixAllProvider() => DocumentBasedFixAllProvider.Instance;

        protected override async Task RegisterCodeFixesAsync(SyntaxNode root, CodeFixContext context)
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
            var optionalAttribute = semanticModel?.Compilation?.GetTypeByMetadataName(
                KnownType.System_Runtime_InteropServices_OptionalAttribute.TypeName);
            if (optionalAttribute == null)
            {
                return;
            }

            context.RegisterCodeFix(
                CodeAction.Create(
                    Title,
                    c =>
                    {
                        var newRoot = root.ReplaceNode(
                            attributeList,
                            GetNewAttributeList(attributeList, optionalAttribute, semanticModel));
                        return Task.FromResult(context.Document.WithSyntaxRoot(newRoot));
                    }),
                context.Diagnostics);
        }

        private static AttributeListSyntax GetNewAttributeList(AttributeListSyntax attributeList, ITypeSymbol optionalAttribute,
            SemanticModel semanticModel)
        {
            var attributeName = optionalAttribute.ToMinimalDisplayString(semanticModel, attributeList.SpanStart);
            attributeName = attributeName.Remove(attributeName.IndexOf("Attribute", System.StringComparison.Ordinal));

            return attributeList.AddAttributes(
                SyntaxFactory.Attribute(
                    SyntaxFactory.ParseName(attributeName)))
                    .WithAdditionalAnnotations(Formatter.Annotation);
        }
    }
}
