/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2018 SonarSource SA
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
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(rule);

        protected override void Initialize(SonarAnalysisContext context) =>
            context.RegisterSyntaxNodeActionInNonGenerated(AnalyzeMethod, SyntaxKind.MethodDeclaration);

        private void AnalyzeMethod(SyntaxNodeAnalysisContext c)
        {
            var methodSymbol = c.SemanticModel.GetDeclaredSymbol(c.Node) as IMethodSymbol;
            if (methodSymbol == null)
            {
                return;
            }

            var descriptor = FindConstraints(methodSymbol);
            if (descriptor == null)
            {
                return;
            }

            var message = GetFaults(methodSymbol, descriptor).ToSentence();
            if (message != null)
            {
                c.ReportDiagnosticWhenActive(Diagnostic.Create(rule, methodSymbol.Locations.First(), message));
            }
        }

        private static Constraints FindConstraints(IMethodSymbol method)
        {
            // Find the first matching attribute type in the table
            var type = method.FindTestAttribute();
            if (type == null)
            {
                return null;
            }

            attributeToConstraintsMap.TryGetValue(type, out Constraints descriptor);
            return descriptor;
        }

        private static IEnumerable<string> GetFaults(IMethodSymbol methodSymbol, Constraints descriptor)
        {
            if (methodSymbol.DeclaredAccessibility != Accessibility.Public && descriptor.PublicOnly)
            {
                yield return MakePublicMessage;
            }

            if (methodSymbol.IsGenericMethod && !descriptor.AllowsGeneric)
            {
                yield return MakeNotGenericMessage;
            }

            // Invariant - applies to all test methods
            if (methodSymbol.IsAsync && methodSymbol.ReturnsVoid)
            {
                yield return MakeNonAsyncOrTaskMessage;
            }
        }

        // We currently support three test framework, each of which supports multiple test method attribute markers, and each of which
        // has differing constraints (public/private, generic/non-generic).
        // Rather than writing lots of conditional code, we're using a simple table-driven approach.
        private static readonly Dictionary<KnownType, Constraints> attributeToConstraintsMap = new Dictionary<KnownType, Constraints>()
        {
            // MSTest
            {KnownType.Microsoft_VisualStudio_TestTools_UnitTesting_TestMethodAttribute, new Constraints(publicOnly: true, allowsGeneric: false) },
            {KnownType.Microsoft_VisualStudio_TestTools_UnitTesting_DataTestMethodAttribute, new Constraints(publicOnly: true, allowsGeneric: false) },

            // NUnit
            {KnownType.NUnit_Framework_TestAttribute, new Constraints(publicOnly: true, allowsGeneric: false) },
            {KnownType.NUnit_Framework_TestCaseAttribute, new Constraints(publicOnly: true, allowsGeneric: true) },
            {KnownType.NUnit_Framework_TestCaseSourceAttribute, new Constraints(publicOnly: true, allowsGeneric: true) },
            {KnownType.NUnit_Framework_TheoryAttribute, new Constraints(publicOnly: true, allowsGeneric: false) },

            // XUnit
            {KnownType.Xunit_FactAttribute, new Constraints(publicOnly: false, allowsGeneric: false) },
            {KnownType.Xunit_TheoryAttribute, new Constraints(publicOnly: false, allowsGeneric: true) },
            {KnownType.LegacyXunit_TheoryAttribute, new Constraints(publicOnly: false, allowsGeneric: true) },
        };

        private class Constraints
        {
            public Constraints(bool publicOnly, bool allowsGeneric)
            {
                PublicOnly = publicOnly;
                AllowsGeneric = allowsGeneric;
            }

            public bool PublicOnly { get; }
            public bool AllowsGeneric { get; }
        }
    }
}
