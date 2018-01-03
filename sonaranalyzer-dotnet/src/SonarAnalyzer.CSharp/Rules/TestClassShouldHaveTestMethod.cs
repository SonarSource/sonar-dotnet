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
using SonarAnalyzer;
using SonarAnalyzer.Helpers;

namespace SonarAnalyzer.Rules.CSharp
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    [Rule(DiagnosticId)]
    public sealed class TestClassShouldHaveTestMethod : SonarDiagnosticAnalyzer
    {
        internal const string DiagnosticId = "S2187";
        private const string MessageFormat = "Add some tests to this class.";

        private static readonly DiagnosticDescriptor rule =
            DiagnosticDescriptorBuilder.GetDescriptor(DiagnosticId, MessageFormat, RspecStrings.ResourceManager);
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(rule);

        private static readonly ISet<KnownType> HandledTestClassAttributes = ImmutableHashSet.Create(
            KnownType.Microsoft_VisualStudio_TestTools_UnitTesting_TestClassAttribute,
            KnownType.NUnit_Framework_TestFixtureAttribute);

        private static readonly ISet<KnownType> HandledTestMethodAttributes = ImmutableHashSet.Create(
            KnownType.Microsoft_VisualStudio_TestTools_UnitTesting_TestMethodAttribute,
            KnownType.Microsoft_VisualStudio_TestTools_UnitTesting_DataTestMethodAttribute,
            KnownType.NUnit_Framework_TestAttribute,
            KnownType.NUnit_Framework_TestCaseAttribute,
            KnownType.NUnit_Framework_TestCaseSourceAttribute,
            KnownType.NUnit_Framework_TheoryAttribute);

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

                    if (classSymbol == null ||
                        classSymbol.IsAbstract ||
                        !classSymbol.GetAttributes().Any(a => a.AttributeClass.IsAny(HandledTestClassAttributes)))
                    {
                        return;
                    }

                    var hasAnyTestMethod = classSymbol.GetMembers()
                        .OfType<IMethodSymbol>()
                        .Any(m => m.GetAttributes().Any(a => a.AttributeClass.IsAny(HandledTestMethodAttributes)));
                    if (!hasAnyTestMethod)
                    {
                        c.ReportDiagnosticWhenActive(Diagnostic.Create(rule, classDeclaration.Identifier.GetLocation()));
                    }
                }, SyntaxKind.ClassDeclaration);
        }
    }
}
