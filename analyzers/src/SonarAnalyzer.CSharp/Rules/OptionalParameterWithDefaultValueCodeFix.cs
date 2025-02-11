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

namespace SonarAnalyzer.CSharp.Rules
{
    [ExportCodeFixProvider(LanguageNames.CSharp)]
    public sealed class OptionalParameterWithDefaultValueCodeFix : SonarCodeFix
    {
        private const string Title = "Change to '[DefaultParameterValue]'";
        public override ImmutableArray<string> FixableDiagnosticIds =>
            ImmutableArray.Create(OptionalParameterWithDefaultValue.DiagnosticId);

        protected override async Task RegisterCodeFixesAsync(SyntaxNode root, SonarCodeFixContext context)
        {
            var diagnostic = context.Diagnostics.First();
            var diagnosticSpan = diagnostic.Location.SourceSpan;
            var attribute = root.FindNode(diagnosticSpan) as AttributeSyntax;

            if (attribute?.ArgumentList == null ||
                attribute.ArgumentList.Arguments.Count != 1)
            {
                return;
            }

            var semanticModel = await context.Document.GetSemanticModelAsync().ConfigureAwait(false);

            var defaultParameterValueAttributeType = semanticModel?.Compilation.GetTypeByMetadataName(KnownType.System_Runtime_InteropServices_DefaultParameterValueAttribute);
            if (defaultParameterValueAttributeType == null)
            {
                return;
            }

            context.RegisterCodeFix(
                Title,
                _ =>
                {
                    var attributeName = defaultParameterValueAttributeType.ToMinimalDisplayString(semanticModel, attribute.SpanStart);
                    attributeName = attributeName.Remove(attributeName.IndexOf("Attribute", System.StringComparison.Ordinal));

                    var newAttribute = attribute.WithName(SyntaxFactory.ParseName(attributeName));
                    var newRoot = root.ReplaceNode(attribute, newAttribute);
                    return Task.FromResult(context.Document.WithSyntaxRoot(newRoot));
                },
                context.Diagnostics);
        }
    }
}
