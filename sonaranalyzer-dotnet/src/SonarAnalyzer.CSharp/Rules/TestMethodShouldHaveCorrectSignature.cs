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

        private static readonly ISet<KnownType> TrackedTestAttributes = ImmutableHashSet.Create(
            KnownType.Microsoft_VisualStudio_TestTools_UnitTesting_TestMethodAttribute,
            KnownType.NUnit_Framework_TestAttribute,
            KnownType.Xunit_FactAttribute);

        protected override void Initialize(SonarAnalysisContext context)
        {
            context.RegisterSyntaxNodeActionInNonGenerated(
                c =>
                {
                    if (!c.IsTest())
                    {
                        return;
                    }

                    var classDeclaration = (ClassDeclarationSyntax)c.Node;
                    var classSymbol = c.SemanticModel.GetDeclaredSymbol(classDeclaration);
                    if (classSymbol == null)
                    {
                        return;
                    }

                    var allFaultyMethods = classSymbol.GetMembers()
                        .OfType<IMethodSymbol>()
                        .Where(IsTestMethod)
                        .Select(method => new
                        {
                            Location = method.Locations.First(),
                            Message = GetFaults(method).ToSentence()
                        })
                        .Where(tuple => tuple.Message != null);

                    foreach (var faultyMethod in allFaultyMethods)
                    {
                        c.CheckReportDiagnostic(Diagnostic.Create(rule, faultyMethod.Location,
                            faultyMethod.Message));
                    }
                },
                SyntaxKind.ClassDeclaration);
        }

        private static bool IsTestMethod(IMethodSymbol method)
        {
            return method.GetAttributes().Any(attribute => attribute.AttributeClass.IsAny(TrackedTestAttributes));
        }

        private static IEnumerable<string> GetFaults(IMethodSymbol methodSymbol)
        {
            if (methodSymbol.DeclaredAccessibility != Accessibility.Public)
            {
                yield return MakePublicMessage;
            }

            if (methodSymbol.IsGenericMethod)
            {
                yield return MakeNotGenericMessage;
            }

            if (methodSymbol.IsAsync && methodSymbol.ReturnsVoid)
            {
                yield return MakeNonAsyncOrTaskMessage;
            }
        }
    }
}
