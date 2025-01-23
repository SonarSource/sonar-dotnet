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
    public sealed class ExceptionsNeedStandardConstructors : SonarDiagnosticAnalyzer
    {
        internal const string DiagnosticId = "S4027";
        private const string MessageFormat = "Implement the missing constructors for this exception.";

        private static readonly DiagnosticDescriptor rule =
            DescriptorFactory.Create(DiagnosticId, MessageFormat);
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(rule);

        protected override void Initialize(SonarAnalysisContext context)
        {
            context.RegisterNodeAction(c =>
            {
                var classDeclaration = c.Node as ClassDeclarationSyntax;
                var classSymbol = c.Model.GetDeclaredSymbol(classDeclaration);

                if (!classDeclaration.Identifier.IsMissing &&
                    classSymbol.DerivesFrom(KnownType.System_Exception) &&
                    !HasStandardConstructors(classSymbol))
                {
                    c.ReportIssue(rule, classDeclaration.Identifier);
                }
            },
            SyntaxKind.ClassDeclaration);
        }

        private static bool HasStandardConstructors(INamedTypeSymbol classSymbol)
        {
            var ctors = classSymbol.Constructors;

            return HasConstructor(ctors, Accessibility.Public) &&
                   HasConstructor(ctors, Accessibility.Public, KnownType.System_String) &&
                   HasConstructor(ctors, Accessibility.Public, KnownType.System_String, KnownType.System_Exception);
        }

        private static bool HasConstructor(ImmutableArray<IMethodSymbol> constructors,
            Accessibility accessibility, params KnownType[] expectedParameterTypes)
        {
            return constructors.Any(c => IsMatchingConstructor(c, accessibility, expectedParameterTypes));
        }

        private static bool IsMatchingConstructor(IMethodSymbol constructor, Accessibility accessibility,
            KnownType[] expectedParameterTypes)
        {
            return constructor.DeclaredAccessibility == accessibility &&
                IEnumerableExtensions.Equals(constructor.Parameters, expectedParameterTypes, (p1, p2) => p1.Type.Is(p2));
        }
    }
}
