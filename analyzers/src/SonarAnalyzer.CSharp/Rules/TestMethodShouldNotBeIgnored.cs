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

namespace SonarAnalyzer.Rules.CSharp
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class TestMethodShouldNotBeIgnored : SonarDiagnosticAnalyzer
    {
        private const string DiagnosticId = "S1607";
        private const string MessageFormat = "Either remove this 'Ignore' attribute or add an explanation about why this test is ignored.";

        private static readonly DiagnosticDescriptor Rule = DescriptorFactory.Create(DiagnosticId, MessageFormat);
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(Rule);

        private static readonly ImmutableArray<KnownType> TrackedTestIdentifierAttributes =
            // xUnit has it's own "ignore" mechanism (by providing a (Skip = "reason") string in
            // the attribute, so there is always an explanation for the test being skipped).
            UnitTestHelper.KnownTestMethodAttributesOfMSTest
            .Concat(new[] { KnownType.Microsoft_VisualStudio_TestTools_UnitTesting_TestClassAttribute })
            .Concat(UnitTestHelper.KnownTestMethodAttributesOfNUnit)
            .ToImmutableArray();

        protected override void Initialize(SonarAnalysisContext context) =>
            context.RegisterNodeAction(
                c =>
                {
                    var attribute = (AttributeSyntax)c.Node;
                    if (HasReasonPhrase(attribute)
                        || HasTrailingComment(attribute)
                        || !IsKnownIgnoreAttribute(attribute, c.Model)
                        || attribute.Parent?.Parent is not { } attributeTarget)
                    {
                        return;
                    }

                    var attributes = GetAllAttributes(attributeTarget, c.Model);

                    if (attributes.Any(IsTestOrTestClassAttribute)
                        && !attributes.Any(IsWorkItemAttribute))
                    {
                        c.ReportIssue(Rule, attribute);
                    }
                },
                SyntaxKind.Attribute);

        private static IEnumerable<AttributeData> GetAllAttributes(SyntaxNode syntaxNode, SemanticModel semanticModel) =>
            semanticModel.GetDeclaredSymbol(syntaxNode) is { } testMethodOrClass
                ? testMethodOrClass.GetAttributes()
                : Enumerable.Empty<AttributeData>();

        private static bool HasReasonPhrase(AttributeSyntax ignoreAttributeSyntax) =>
            ignoreAttributeSyntax.ArgumentList?.Arguments.Count > 0; // Any ctor argument counts are reason phrase

        private static bool HasTrailingComment(SyntaxNode ignoreAttributeSyntax) =>
            ignoreAttributeSyntax.Parent
                .GetTrailingTrivia()
                .Any(SyntaxKind.SingleLineCommentTrivia);

        private static bool IsWorkItemAttribute(AttributeData a) =>
            a.AttributeClass.Is(KnownType.Microsoft_VisualStudio_TestTools_UnitTesting_WorkItemAttribute);

        private static bool IsKnownIgnoreAttribute(AttributeSyntax attributeSyntax, SemanticModel semanticModel)
        {
            var symbolInfo = semanticModel.GetSymbolInfo(attributeSyntax);

            var attributeConstructor = symbolInfo.Symbol ?? symbolInfo.CandidateSymbols.FirstOrDefault();

            return attributeConstructor != null && attributeConstructor.ContainingType.DerivesFromAny(UnitTestHelper.KnownIgnoreAttributes);
        }

        private static bool IsTestOrTestClassAttribute(AttributeData a) =>
            a.AttributeClass.DerivesFromAny(TrackedTestIdentifierAttributes);
    }
}
