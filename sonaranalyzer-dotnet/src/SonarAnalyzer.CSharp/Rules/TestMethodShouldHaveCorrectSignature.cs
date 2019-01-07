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
using Microsoft.CodeAnalysis.Diagnostics;
using SonarAnalyzer.Common;
using SonarAnalyzer.Helpers;

namespace SonarAnalyzer.Rules.CSharp
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    [Rule(DiagnosticId)]
    public sealed class TestMethodShouldHaveCorrectSignature : SonarDiagnosticAnalyzer
    {
        internal const string DiagnosticId = "S3433";
        private const string MessageFormat = "Make this test method {0}.";
        private const string MakePublicMessage = "'public'";
        private const string MakeNonAsyncOrTaskMessage = "non-'async' or return 'Task'";
        private const string MakeNotGenericMessage = "non-generic";

        private static readonly DiagnosticDescriptor rule =
            DiagnosticDescriptorBuilder.GetDescriptor(DiagnosticId, MessageFormat, RspecStrings.ResourceManager);
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(rule);

        /// <summary>
        /// Validation method. Checks the supplied method and returns the error message,
        /// or null if there is no issue.
        /// </summary>
        private delegate string SignatureValidator(IMethodSymbol method);

        private static readonly SignatureValidator NullValidator = m => null;

        // We currently support three test framework, each of which supports multiple test method attribute markers, and each of which
        // has differing constraints (public/private, generic/non-generic).
        // Rather than writing lots of conditional code, we're using a simple table-driven approach.
        // Currently we use the same validation method for all method types, but we could have a
        // different validation method for each type in future if necessary.
        private static readonly Dictionary<KnownType, SignatureValidator> attributeToConstraintsMap = new Dictionary<KnownType, SignatureValidator>
        {
            // MSTest
            {
                KnownType.Microsoft_VisualStudio_TestTools_UnitTesting_TestMethodAttribute,
                m => GetFaultMessage(m, publicOnly: true, allowGenerics: false)
            },
            {
                KnownType.Microsoft_VisualStudio_TestTools_UnitTesting_DataTestMethodAttribute,
                m => GetFaultMessage(m, publicOnly: true, allowGenerics: false)
            },

            // NUnit
            {
                KnownType.NUnit_Framework_TestAttribute,
                m => GetFaultMessage(m, publicOnly: true, allowGenerics: false)
            },
            {
                KnownType.NUnit_Framework_TestCaseAttribute,
                m => GetFaultMessage(m, publicOnly: true, allowGenerics: true)
            },
            {
                KnownType.NUnit_Framework_TestCaseSourceAttribute,
                m => GetFaultMessage(m, publicOnly: true, allowGenerics: true)
            },
            {
                KnownType.NUnit_Framework_TheoryAttribute,
                m => GetFaultMessage(m, publicOnly: true, allowGenerics: false)
            },

            // XUnit
            {
                KnownType.Xunit_FactAttribute,
                m => GetFaultMessage(m, publicOnly: false, allowGenerics: false)
            },
            {
                KnownType.Xunit_TheoryAttribute,
                m => GetFaultMessage(m, publicOnly: false, allowGenerics: true)
            },
            {
                KnownType.LegacyXunit_TheoryAttribute,
                m => GetFaultMessage(m, publicOnly: false, allowGenerics: true)
            }
        };

        protected override void Initialize(SonarAnalysisContext context) =>
            context.RegisterSyntaxNodeActionInNonGenerated(AnalyzeMethod, SyntaxKind.MethodDeclaration);

        private void AnalyzeMethod(SyntaxNodeAnalysisContext c)
        {
            if (!(c.SemanticModel.GetDeclaredSymbol(c.Node) is IMethodSymbol methodSymbol))
            {
                return;
            }

            var validator = GetValidator(methodSymbol);
            var message = validator(methodSymbol);
            if (message != null)
            {
                c.ReportDiagnosticWhenActive(Diagnostic.Create(rule, methodSymbol.Locations.First(), message));
            }
        }

        private static SignatureValidator GetValidator(IMethodSymbol method)
        {
            // Find the first matching attribute type in the table
            var attributeKnownType = method.FindFirstTestMethodType();
            if (attributeKnownType == null)
            {
                return NullValidator;
            }

            return attributeToConstraintsMap.GetValueOrDefault(attributeKnownType);
        }

        private static string GetFaultMessage(IMethodSymbol methodSymbol, bool publicOnly, bool allowGenerics) =>
            GetFaultMessageParts(methodSymbol, publicOnly, allowGenerics).ToSentence();

        private static IEnumerable<string> GetFaultMessageParts(IMethodSymbol methodSymbol, bool publicOnly, bool allowGenerics)
        {
            if (methodSymbol.DeclaredAccessibility != Accessibility.Public && publicOnly)
            {
                yield return MakePublicMessage;
            }

            if (methodSymbol.IsGenericMethod && !allowGenerics)
            {
                yield return MakeNotGenericMessage;
            }

            // Invariant - applies to all test methods
            if (methodSymbol.IsAsync && methodSymbol.ReturnsVoid)
            {
                yield return MakeNonAsyncOrTaskMessage;
            }
        }
    }
}
