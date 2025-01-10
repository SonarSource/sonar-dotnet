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
    public sealed class EquatableClassShouldBeSealed : SonarDiagnosticAnalyzer
    {
        internal const string DiagnosticId = "S4035";
        private const string MessageFormat = "Seal class '{0}' or implement 'IEqualityComparer<T>' instead.";
        private const string EqualsMethodName = nameof(object.Equals);

        private static readonly DiagnosticDescriptor Rule =
            DescriptorFactory.Create(DiagnosticId, MessageFormat);
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(Rule);

        protected override void Initialize(SonarAnalysisContext context) =>
            context.RegisterNodeAction(
                c =>
                {
                    var classDeclaration = (ClassDeclarationSyntax)c.Node;
                    var classSymbol = c.SemanticModel.GetDeclaredSymbol(classDeclaration);

                    if (classSymbol != null
                        && !classSymbol.IsSealed
                        && !classSymbol.IsStatic
                        && classSymbol.IsPubliclyAccessible()
                        && !classDeclaration.Identifier.IsMissing
                        && HasAnyInvalidIEquatableEqualsMethod(classSymbol))
                    {
                        c.ReportIssue(Rule, classDeclaration.Identifier, classDeclaration.Identifier.ValueText);
                    }
                },
                SyntaxKind.ClassDeclaration);

        private static bool HasAnyInvalidIEquatableEqualsMethod(INamedTypeSymbol classSymbol)
        {
            var equatableInterfacesByTypeName = classSymbol.Interfaces
                .Where(IsCompilableIEquatableTSymbol)
                .ToDictionary(nts => nts.TypeArguments[0].Name, nts => nts);

            var equalsMethodsByTypeName = classSymbol.GetMembers(EqualsMethodName)
                .OfType<IMethodSymbol>()
                .Where(IsIEquatableEqualsMethodCandidate)
                .ToDictionary(ms => ms.Parameters[0].Type.Name, ms => ms);

            // Checks whether any IEquatable<T> has no implementation OR a non-virtual non-abstract implementation
            var hasAnyConcreteImplementation = equatableInterfacesByTypeName
                .Select(iequatable => equalsMethodsByTypeName.GetValueOrDefault(iequatable.Key))
                .Any(associatedMethod => associatedMethod == null || !(associatedMethod.IsVirtual || associatedMethod.IsAbstract));
            if (hasAnyConcreteImplementation)
            {
                return true;
            }

            // For all Equals(T) not a IEquatable<T> implementation checks if any is non-virtual
            var unprocessedTypeNames = equalsMethodsByTypeName.Keys.Except(equatableInterfacesByTypeName.Keys);
            return unprocessedTypeNames.Any(typeName => !equalsMethodsByTypeName[typeName].IsVirtual);
        }

        private static bool IsCompilableIEquatableTSymbol(INamedTypeSymbol namedTypeSymbol) =>
            namedTypeSymbol.ConstructedFrom.Is(KnownType.System_IEquatable_T)
            && namedTypeSymbol.TypeArguments.Length == 1;

        private static bool IsIEquatableEqualsMethodCandidate(IMethodSymbol methodSymbol) =>
            methodSymbol.MethodKind == MethodKind.Ordinary
            && methodSymbol.Name == EqualsMethodName
            && methodSymbol.IsPubliclyAccessible()
            && !methodSymbol.IsOverride
            && methodSymbol.ReturnType.Is(KnownType.System_Boolean)
            && methodSymbol.Parameters.Length == 1;
    }
}
