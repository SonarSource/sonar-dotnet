/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2014-2025 SonarSource Sàrl
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

using Microsoft.CodeAnalysis.Formatting;

namespace SonarAnalyzer.CSharp.Rules
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
