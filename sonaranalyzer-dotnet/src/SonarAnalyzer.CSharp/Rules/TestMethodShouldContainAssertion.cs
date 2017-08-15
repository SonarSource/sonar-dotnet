/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2017 SonarSource SA
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
    public sealed class TestMethodShouldContainAssertion : SonarDiagnosticAnalyzer
    {
        internal const string DiagnosticId = "S2699";
        private const string MessageFormat = "Add at least one assertion to this test case.";

        private static readonly DiagnosticDescriptor rule =
            DiagnosticDescriptorBuilder.GetDescriptor(DiagnosticId, MessageFormat, RspecStrings.ResourceManager);
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(rule);

        private static readonly ISet<KnownType> TrackedTestAttributes = ImmutableHashSet.Create(
            KnownType.Microsoft_VisualStudio_TestTools_UnitTesting_TestMethodAttribute,
            KnownType.NUnit_Framework_TestAttribute,
            KnownType.Xunit_FactAttribute);

        private static readonly ISet<KnownType> ExpectedExceptionAttributes = ImmutableHashSet.Create(
            KnownType.Microsoft_VisualStudio_TestTools_UnitTesting_ExpectedExceptionAttribute,
            KnownType.NUnit_Framework_ExpectedExceptionAttribute);

        private static readonly ISet<KnownType> TrackedAssertions = ImmutableHashSet.Create(
            KnownType.Microsoft_VisualStudio_TestTools_UnitTesting_Assert,
            KnownType.NUnit_Framework_Assert,
            KnownType.Xunit_Assert);

        protected override void Initialize(SonarAnalysisContext context)
        {
            context.RegisterSyntaxNodeActionInNonGenerated(
                c =>
                {
                    var methodDeclaration = (MethodDeclarationSyntax)c.Node;
                    if (methodDeclaration.Identifier.IsMissing ||
                        methodDeclaration.Body == null ||
                        !c.IsTest())
                    {
                        return;
                    }

                    var methodSymbol = c.SemanticModel.GetDeclaredSymbol(methodDeclaration);
                    var attributes = methodSymbol?.GetAttributes();
                    if (methodSymbol == null ||
                        !IsTestMethod(attributes) ||
                        HasExpectedExceptionAttribute(attributes))
                    {
                        return;
                    }

                    var hasAnyAssertion = methodDeclaration.DescendantNodes()
                        .OfType<InvocationExpressionSyntax>()
                        .Any(invocation => IsAssertion(invocation, c.SemanticModel));
                    if (!hasAnyAssertion)
                    {
                        c.ReportDiagnostic(Diagnostic.Create(rule, methodDeclaration.Identifier.GetLocation()));
                    }
                }, SyntaxKind.MethodDeclaration);
        }

        private static bool IsTestMethod(IEnumerable<AttributeData> attributes) =>
            attributes.Any(attribute => attribute.AttributeClass.IsAny(TrackedTestAttributes));

        private static bool HasExpectedExceptionAttribute(IEnumerable<AttributeData> attributes) =>
            attributes.Any(attribute => attribute.AttributeClass.IsAny(ExpectedExceptionAttributes));

        private static bool IsAssertion(InvocationExpressionSyntax invocation, SemanticModel model)
        {
            var symbolInfo = model.GetSymbolInfo(invocation);

            // For some reason FluentAssertions is not loaded correctly in the test so we have to test
            // for CandidateSymbols.
            var symbol = symbolInfo.CandidateReason == CandidateReason.OverloadResolutionFailure
                ? symbolInfo.CandidateSymbols.FirstOrDefault()
                : symbolInfo.Symbol;

            if (symbol == null)
            {
                return false;
            }

            return symbol.ContainingType.ToString().StartsWith("FluentAssertions") ||
                symbol.ContainingType.IsAny(TrackedAssertions);
        }
    }
}
