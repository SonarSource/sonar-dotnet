/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2014-2025 SonarSource Sàrl
 * mailto:info AT sonarsource DOT com
 * This program is free software; you can redistribute it and/or
 * modify it under the terms of the Sonar Source-Available License Version 1, as published by SonarSource Sàrl.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
 * See the Sonar Source-Available License for more details.
 *
 * You should have received a copy of the Sonar Source-Available License
 * along with this program; if not, see https://sonarsource.com/license/ssal/
 */

namespace SonarAnalyzer.CSharp.Rules
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class ReferenceEqualityCheckWhenEqualsExists : SonarDiagnosticAnalyzer
    {
        internal const string DiagnosticId = "S1698";
        private const string MessageFormat = "Consider using 'Equals' if value comparison was intended.";
        private const string EqualsName = nameof(Equals);

        private static readonly DiagnosticDescriptor Rule =
            DescriptorFactory.Create(DiagnosticId, MessageFormat);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(Rule);

        private static readonly ImmutableArray<KnownType> AllowedTypes =
            ImmutableArray.Create(
                KnownType.System_Type,
                KnownType.System_Reflection_Assembly,
                KnownType.System_Reflection_MemberInfo,
                KnownType.System_Reflection_Module,
                KnownType.System_Data_Common_CommandTrees_DbExpression,
                KnownType.System_Object);

        private static readonly ImmutableArray<KnownType> AllowedTypesWithAllDerived =
            ImmutableArray.Create(KnownType.System_Windows_DependencyObject);

        protected override void Initialize(SonarAnalysisContext context) =>
            context.RegisterCompilationStartAction(
                compilationStartContext =>
                {
                    var allInterfacesWithImplementationsOverriddenEquals =
                        compilationStartContext.Compilation.GlobalNamespace
                            .GetAllNamedTypes()
                            .Where(t => t.AllInterfaces.Any() && HasEqualsOverride(t))
                            .SelectMany(t => t.AllInterfaces)
                            .ToHashSet();

                    compilationStartContext.RegisterNodeAction(
                        c =>
                        {
                            var binary = (BinaryExpressionSyntax)c.Node;
                            if (!IsBinaryCandidateForReporting(binary, c.Model))
                            {
                                return;
                            }

                            var typeLeft = c.Model.GetTypeInfo(binary.Left).Type;
                            var typeRight = c.Model.GetTypeInfo(binary.Right).Type;
                            if (IsAllowedTypeOrNull(typeLeft) || IsAllowedTypeOrNull(typeRight))
                            {
                                return;
                            }

                            if (MightOverrideEquals(typeLeft, allInterfacesWithImplementationsOverriddenEquals) ||
                                MightOverrideEquals(typeRight, allInterfacesWithImplementationsOverriddenEquals))
                            {
                                c.ReportIssue(Rule, binary.OperatorToken);
                            }
                        },
                        SyntaxKind.EqualsExpression,
                        SyntaxKind.NotEqualsExpression);
                });

        private static bool MightOverrideEquals(ITypeSymbol type, ISet<INamedTypeSymbol> allInterfacesWithImplementationsOverriddenEquals) =>
            HasEqualsOverride(type)
            || allInterfacesWithImplementationsOverriddenEquals.Contains(type)
            || HasTypeConstraintsWhichMightOverrideEquals(type, allInterfacesWithImplementationsOverriddenEquals);

        private static bool HasTypeConstraintsWhichMightOverrideEquals(ITypeSymbol type, ISet<INamedTypeSymbol> allInterfacesWithImplementationsOverriddenEquals) =>
            type is ITypeParameterSymbol genericParameter
            && genericParameter.ConstraintTypes.Any(x => MightOverrideEquals(x, allInterfacesWithImplementationsOverriddenEquals));

        private static bool IsAllowedTypeOrNull(ITypeSymbol type) =>
            type is null || type.IsAny(AllowedTypes) || HasAllowedBaseType(type);

        private static bool HasAllowedBaseType(ITypeSymbol type) =>
            type.GetSelfAndBaseTypes().Any(t => t.IsAny(AllowedTypesWithAllDerived));

        private static bool IsBinaryCandidateForReporting(BinaryExpressionSyntax binary, SemanticModel semanticModel) =>
            semanticModel.GetSymbolInfo(binary).Symbol is IMethodSymbol equalitySymbol
            && equalitySymbol.IsInType(KnownType.System_Object)
            && !IsInEqualsOverride(semanticModel.GetEnclosingSymbol(binary.SpanStart) as IMethodSymbol);

        private static bool HasEqualsOverride(ITypeSymbol type) =>
            GetEqualsOverrides(type).Any(x => x.OverriddenMethod.IsInType(KnownType.System_Object));

        private static IEnumerable<IMethodSymbol> GetEqualsOverrides(ITypeSymbol type)
        {
            var candidateEqualsMethods = new HashSet<IMethodSymbol>();

            foreach (var currentType in type.GetSelfAndBaseTypes().TakeWhile(tp => !tp.Is(KnownType.System_Object)))
            {
                candidateEqualsMethods.UnionWith(currentType
                    .GetMembers(EqualsName)
                    .OfType<IMethodSymbol>()
                    .Where(method => method.IsOverride && method.OverriddenMethod != null));
            }

            return candidateEqualsMethods;
        }

        private static bool IsInEqualsOverride(IMethodSymbol method)
        {
            var currentMethod = method;
            while (currentMethod != null)
            {
                if (currentMethod.Name == EqualsName &&
                    currentMethod.IsInType(KnownType.System_Object))
                {
                    return true;
                }
                currentMethod = currentMethod.OverriddenMethod;
            }
            return false;
        }
    }
}
