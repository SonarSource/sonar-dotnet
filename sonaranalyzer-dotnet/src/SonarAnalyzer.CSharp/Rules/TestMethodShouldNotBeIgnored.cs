/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2019 SonarSource SA
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

using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using SonarAnalyzer.Common;
using SonarAnalyzer.Helpers;

namespace SonarAnalyzer.Rules.CSharp
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    [Rule(DiagnosticId)]
    public sealed class TestMethodShouldNotBeIgnored : SonarDiagnosticAnalyzer
    {
        internal const string DiagnosticId = "S1607";
        private const string MessageFormat = "Either remove this 'Ignore' attribute or add an explanation about why " +
            "this test is ignored.";

        private static readonly DiagnosticDescriptor rule =
            DiagnosticDescriptorBuilder.GetDescriptor(DiagnosticId, MessageFormat, RspecStrings.ResourceManager);
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(rule);

        private static readonly ImmutableArray<KnownType> TrackedTestIdentifierAttributes =
            // xUnit has it's own "ignore" mechanism (by providing a (Skip = "reason") string in
            // the attribute, so there is always an explanation for the test being skipped).
            UnitTestHelper.KnownTestMethodAttributes_MSTest
            .Concat(new[] { KnownType.Microsoft_VisualStudio_TestTools_UnitTesting_TestClassAttribute })
            .Concat(UnitTestHelper.KnownTestMethodAttributes_NUnit)
            .ToImmutableArray();

        protected override void Initialize(SonarAnalysisContext context)
        {
            context.RegisterSyntaxNodeActionInNonGenerated(
                c =>
                {
                    var attribute = (AttributeSyntax)c.Node;
                    if (HasReasonPhrase(attribute) ||
                        HasTrailingComment(attribute) ||
                        !IsKnownIgnoreAttribute(attribute, c.SemanticModel))
                    {
                        return;
                    }

                    var attributeTarget = attribute.Parent?.Parent;
                    if (attributeTarget == null)
                    {
                        return;
                    }

                    var attributes = GetAllAttributes(attributeTarget, c.SemanticModel);

                    if (attributes.Any(IsTestOrTestClassAttribute) &&
                        !attributes.Any(IsWorkItemAttribute))
                    {
                        c.ReportDiagnosticWhenActive(Diagnostic.Create(rule, attribute.GetLocation()));
                    }
                },
                SyntaxKind.Attribute);
        }

        private IEnumerable<AttributeData> GetAllAttributes(SyntaxNode syntaxNode, SemanticModel semanticModel)
        {
            var testMethodOrClass = semanticModel.GetDeclaredSymbol(syntaxNode);

            return testMethodOrClass == null
                ? Enumerable.Empty<AttributeData>()
                : testMethodOrClass.GetAttributes();
        }

        private bool HasReasonPhrase(AttributeSyntax ignoreAttributeSyntax) =>
            ignoreAttributeSyntax.ArgumentList?.Arguments.Count > 0; // Any ctor argument counts are reason phrase

        private static bool HasTrailingComment(SyntaxNode ignoreAttributeSyntax) =>
            ignoreAttributeSyntax.Parent.GetTrailingTrivia()
                .Any(SyntaxKind.SingleLineCommentTrivia);

        private static bool IsWorkItemAttribute(AttributeData a) =>
            a.AttributeClass.Is(KnownType.Microsoft_VisualStudio_TestTools_UnitTesting_WorkItemAttribute);

        private static bool IsKnownIgnoreAttribute(AttributeSyntax attributeSyntax, SemanticModel semanticModel)
        {
            var symbolInfo = semanticModel.GetSymbolInfo(attributeSyntax);

            var attributeConstructor = symbolInfo.Symbol ?? symbolInfo.CandidateSymbols.FirstOrDefault();

            return attributeConstructor != null
                && (attributeConstructor.ContainingType
                        .IsAny(UnitTestHelper.KnownIgnoreAttributes));
        }

        private static bool IsTestOrTestClassAttribute(AttributeData a) =>
            a.AttributeClass.IsAny(TrackedTestIdentifierAttributes);
    }
}
