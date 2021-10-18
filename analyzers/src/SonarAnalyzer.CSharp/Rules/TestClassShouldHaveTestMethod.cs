/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2021 SonarSource SA
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
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using SonarAnalyzer.Common;
using SonarAnalyzer.Extensions;
using SonarAnalyzer.Helpers;
using StyleCop.Analyzers.Lightup;

namespace SonarAnalyzer.Rules.CSharp
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    [Rule(DiagnosticId)]
    public sealed class TestClassShouldHaveTestMethod : SonarDiagnosticAnalyzer
    {
        private const string DiagnosticId = "S2187";
        private const string MessageFormat = "Add some tests to this {0}.";

        private static readonly ImmutableArray<KnownType> HandledGlobalSetupAndCleanUpAttributes =
            ImmutableArray.Create(
                // Only applies to MSTest.
                // NUnit has equivalent attributes, but they can only be applied to classes
                // marked with [SetupFixture], which cannot contain tests.
                KnownType.Microsoft_VisualStudio_TestTools_UnitTesting_AssemblyInitializeAttribute,
                KnownType.Microsoft_VisualStudio_TestTools_UnitTesting_AssemblyCleanupAttribute);

        private static readonly DiagnosticDescriptor Rule =
            DiagnosticDescriptorBuilder.GetDescriptor(DiagnosticId, MessageFormat, RspecStrings.ResourceManager);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(Rule);

        protected override void Initialize(SonarAnalysisContext context) =>
            context.RegisterSyntaxNodeActionInNonGenerated(
                c =>
                {
                    if (c.ContainingSymbol.Kind != SymbolKind.NamedType)
                    {
                        return;
                    }

                    var typeDeclaration = (TypeDeclarationSyntax)c.Node;
                    if (typeDeclaration.Identifier.IsMissing)
                    {
                        return;
                    }

                    var typeSymbol = c.SemanticModel.GetDeclaredSymbol(typeDeclaration);

                    if (typeSymbol != null
                        && IsViolatingRule(typeSymbol)
                        && !IsExceptionToTheRule(typeSymbol))
                    {
                        c.ReportIssue(Diagnostic.Create(Rule, typeDeclaration.Identifier.GetLocation(), typeDeclaration.GetDeclarationTypeName()));
                    }
                },
                SyntaxKind.ClassDeclaration,
                SyntaxKindEx.RecordDeclaration);

        private static bool HasAnyTestMethod(INamedTypeSymbol classSymbol) =>
            classSymbol.GetMembers().OfType<IMethodSymbol>().Any(m => m.IsTestMethod());

        private static bool IsViolatingRule(INamedTypeSymbol classSymbol) =>
            classSymbol.IsTestClass()
            && !HasAnyTestMethod(classSymbol);

        private static bool IsExceptionToTheRule(INamedTypeSymbol classSymbol) =>
            classSymbol.IsAbstract
            || (classSymbol.BaseType.IsAbstract && HasAnyTestMethod(classSymbol.BaseType))
            || HasSetupOrCleanupAttributes(classSymbol);

        private static bool HasSetupOrCleanupAttributes(INamedTypeSymbol classSymbol) =>
            classSymbol.GetMembers().OfType<IMethodSymbol>().Any(m => m.GetAttributes(HandledGlobalSetupAndCleanUpAttributes).Any());
    }
}
