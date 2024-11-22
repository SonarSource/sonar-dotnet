/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2014-2024 SonarSource SA
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
    public sealed class TestClassShouldHaveTestMethod : SonarDiagnosticAnalyzer
    {
        private const string DiagnosticId = "S2187";
        private const string MessageFormat = "Add some tests to this {0}.";

        private static readonly ImmutableArray<KnownType> HandledSetupAndCleanUpAttributes =
            ImmutableArray.Create(
                // Only applies to MSTest.
                // NUnit has equivalent attributes, but they can only be applied to classes
                // marked with [SetupFixture], which cannot contain tests.
                KnownType.Microsoft_VisualStudio_TestTools_UnitTesting_AssemblyInitializeAttribute,
                KnownType.Microsoft_VisualStudio_TestTools_UnitTesting_AssemblyCleanupAttribute);

        private static readonly DiagnosticDescriptor Rule =
            DescriptorFactory.Create(DiagnosticId, MessageFormat);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(Rule);

        protected override void Initialize(SonarAnalysisContext context) =>
            context.RegisterNodeAction(c =>
                {
                    var typeDeclaration = (TypeDeclarationSyntax)c.Node;
                    if (!c.IsRedundantPositionalRecordContext()
                        && !typeDeclaration.Identifier.IsMissing
                        && c.SemanticModel.GetDeclaredSymbol(typeDeclaration) is { } typeSymbol
                        && IsViolatingRule(typeSymbol)
                        && !IsExceptionToTheRule(typeSymbol))
                    {
                        c.ReportIssue(Rule, typeDeclaration.Identifier, typeDeclaration.GetDeclarationTypeName());
                    }
                },
                SyntaxKind.ClassDeclaration,
                SyntaxKindEx.RecordDeclaration);

        private static bool HasAnyTestMethod(INamespaceOrTypeSymbol symbol) =>
            symbol.GetMembers().OfType<IMethodSymbol>().Any(m => m.IsTestMethod());

        private static bool IsViolatingRule(INamedTypeSymbol symbol) =>
            symbol.IsTestClass()
            && !HasAnyTestMethod(symbol);

        private static bool IsExceptionToTheRule(ITypeSymbol symbol) =>
            symbol.IsAbstract
            || symbol.GetSelfAndBaseTypes().Any(HasAnyTestMethod)
            || HasSetupOrCleanupAttributes(symbol);

        private static bool HasSetupOrCleanupAttributes(INamespaceOrTypeSymbol symbol) =>
            symbol.GetMembers().OfType<IMethodSymbol>().Any(m => m.GetAttributes(HandledSetupAndCleanUpAttributes).Any());
    }
}
