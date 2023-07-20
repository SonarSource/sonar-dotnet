/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2023 SonarSource SA
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

namespace SonarAnalyzer.Rules.CSharp
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class MethodsShouldUseBaseTypes : SonarDiagnosticAnalyzer
    {
        private const string DiagnosticId = "S3242";
        private const string MessageFormat = "Consider using more general type '{0}' instead of '{1}'.";

        private static readonly DiagnosticDescriptor Rule = DescriptorFactory.Create(DiagnosticId, MessageFormat);
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(Rule);

        protected override void Initialize(SonarAnalysisContext context) =>
            context.RegisterNodeAction(
                c => FindViolations((BaseMethodDeclarationSyntax)c.Node, c.SemanticModel).ForEach(d => c.ReportIssue(d)),
                SyntaxKind.MethodDeclaration);

        private static List<Diagnostic> FindViolations(BaseMethodDeclarationSyntax methodDeclaration, SemanticModel semanticModel)
        {
            if (semanticModel.GetDeclaredSymbol(methodDeclaration) is not { } methodSymbol
                || methodSymbol.Parameters.Length == 0
                || methodSymbol.IsOverride
                || methodSymbol.IsVirtual
                || methodSymbol.IsControllerMethod()
                || methodSymbol.GetInterfaceMember() != null
                || methodSymbol.IsEventHandler())
            {
                return Enumerable.Empty<Diagnostic>().ToList();
            }

            var methodAccessibility = methodSymbol.GetEffectiveAccessibility();
            // The GroupBy is useless in most of the cases but safe-guard in case of 2+ parameters with same name (invalid code).
            // In this case we analyze only the first parameter (a new analysis will be triggered after fixing the names).
            var parametersToCheck = methodSymbol.Parameters
                .Where(IsTrackedParameter)
                .GroupBy(p => p.Name)
                .ToDictionary(p => p.Key, p => new ParameterData(p.First(), methodAccessibility));

            var parameterUsesInMethod = methodDeclaration
                .DescendantNodes()
                .OfType<IdentifierNameSyntax>()
                .Where(id => parametersToCheck.Values.Any(p => p.MatchesIdentifier(id, semanticModel)));

            foreach (var identifierReference in parameterUsesInMethod)
            {
                var key = identifierReference.Identifier.ValueText ?? string.Empty;
                if (!parametersToCheck.TryGetValue(key, out var paramData) || !paramData.ShouldReportOn)
                {
                    continue;
                }

                if (identifierReference.Parent is EqualsValueClauseSyntax or AssignmentExpressionSyntax)
                {
                    paramData.ShouldReportOn = false;
                    continue;
                }

                var symbolUsedAs = FindParameterUseAsType(identifierReference, semanticModel);
                if (symbolUsedAs != null
                    && !IsNestedGeneric(symbolUsedAs)) // In order to avoid triggering "S4017: Refactor this method to remove the nested type argument."
                {
                    paramData.AddUsage(symbolUsedAs);
                }
            }

            return parametersToCheck.Values
                .Select(p => p.GetRuleViolation())
                .WhereNotNull()
                .ToList();
        }

        private static bool IsNestedGeneric(ISymbol symbol) =>
            symbol is INamedTypeSymbol { IsGenericType: true } namedTypeSymbol
            && namedTypeSymbol.TypeArguments.Any(argument => argument is INamedTypeSymbol { IsGenericType: true });

        private static bool IsTrackedParameter(IParameterSymbol parameterSymbol)
        {
            var type = parameterSymbol.Type;

            return !type.DerivesFrom(KnownType.System_Array)
                   && !type.IsValueType
                   && !type.Is(KnownType.System_String);
        }

        private static SyntaxNode GetFirstNonParenthesizedParent(SyntaxNode node) =>
            node is ExpressionSyntax expression ? expression.GetFirstNonParenthesizedParent() : node;

        private static ITypeSymbol FindParameterUseAsType(SyntaxNode identifier, SemanticModel semanticModel)
        {
            var callSite = semanticModel.GetEnclosingSymbol(identifier.SpanStart)?.ContainingAssembly;
            var identifierParent = GetFirstNonParenthesizedParent(identifier);

            return identifierParent switch
                   {
                       ConditionalAccessExpressionSyntax conditionalAccess => HandleConditionalAccess(conditionalAccess, identifier, semanticModel, callSite),
                       MemberAccessExpressionSyntax => GetFirstNonParenthesizedParent(identifierParent) is InvocationExpressionSyntax invocationExpression
                                                           ? HandleInvocation(identifier, semanticModel.GetSymbolInfo(invocationExpression).Symbol, semanticModel, callSite)
                                                           : HandlePropertyOrField(identifier, semanticModel.GetSymbolInfo(identifierParent).Symbol, callSite),
                       ArgumentSyntax => semanticModel.GetTypeInfo(identifier).ConvertedType,
                       ElementAccessExpressionSyntax => HandlePropertyOrField(identifier, semanticModel.GetSymbolInfo(identifierParent).Symbol, callSite),
                       _ => null
                   };
        }

        private static ITypeSymbol HandleConditionalAccess(ConditionalAccessExpressionSyntax conditionalAccess, SyntaxNode identifier, SemanticModel semanticModel, IAssemblySymbol callSite)
        {
            var conditionalAccessExpression = conditionalAccess.WhenNotNull is ConditionalAccessExpressionSyntax subsequentConditionalAccess
                ? subsequentConditionalAccess.Expression
                : conditionalAccess.WhenNotNull;

            return conditionalAccessExpression switch
                   {
                       MemberBindingExpressionSyntax { Name: { } } binding => HandlePropertyOrField(identifier, semanticModel.GetSymbolInfo(binding.Name).Symbol, callSite),
                       InvocationExpressionSyntax { Expression: MemberBindingExpressionSyntax memberBinding } => HandleInvocation(identifier, semanticModel.GetSymbolInfo(memberBinding).Symbol,
                           semanticModel, callSite),
                       _ => null
                   };
        }

        private static ITypeSymbol HandlePropertyOrField(SyntaxNode identifier, ISymbol symbol, IAssemblySymbol callSite)
        {
            if (symbol is not IPropertySymbol propertySymbol)
            {
                return FindOriginatingSymbol(symbol, callSite);
            }

            var parent = GetFirstNonParenthesizedParent(identifier);
            var grandParent = GetFirstNonParenthesizedParent(parent);

            var propertyAccessor = grandParent is AssignmentExpressionSyntax
                    ? propertySymbol.SetMethod
                    : propertySymbol.GetMethod;

            return FindOriginatingSymbol(propertyAccessor, callSite);
        }

        private static ITypeSymbol HandleInvocation(SyntaxNode invokedOn, ISymbol invocationSymbol, SemanticModel semanticModel, IAssemblySymbol callSite)
        {
            if (invocationSymbol is not IMethodSymbol methodSymbol)
            {
                return null;
            }

            return methodSymbol.IsExtensionMethod
                ? semanticModel.GetTypeInfo(invokedOn).ConvertedType
                : FindOriginatingSymbol(invocationSymbol, callSite);
        }

        private static INamedTypeSymbol FindOriginatingSymbol(ISymbol accessedMember, ISymbol usageSite)
        {
            if (accessedMember == null)
            {
                return null;
            }

            var originatingInterface = accessedMember.GetInterfaceMember()?.ContainingType;
            if (originatingInterface != null && IsNotInternalOrSameAssembly(originatingInterface))
            {
                return originatingInterface;
            }

            var originatingType = accessedMember.GetOverriddenMember()?.ContainingType;
            return originatingType != null && IsNotInternalOrSameAssembly(originatingType)
                ? originatingType
                : accessedMember.ContainingType;

            // Do not suggest internal types that are declared in an assembly different than
            // the one that's declaring the parameter. Such types should not be suggested at
            // all if there is no InternalsVisibleTo attribute present in the compilation.
            // Since the check for the attribute must be done in CompilationEnd thus making
            // the rule unusable in Visual Studio, we will not suggest such classes and will
            // generate some False Negatives.
            bool IsNotInternalOrSameAssembly(ISymbol namedTypeSymbol) =>
                namedTypeSymbol.ContainingAssembly.Equals(usageSite)
                || namedTypeSymbol.GetEffectiveAccessibility() != Accessibility.Internal;
        }

        private sealed class ParameterData
        {
            public bool ShouldReportOn { get; set; } = true;

            private readonly IParameterSymbol parameterSymbol;
            private readonly Accessibility methodAccessibility;
            private readonly Dictionary<ITypeSymbol, int> usedAs = new();

            public ParameterData(IParameterSymbol parameterSymbol, Accessibility methodAccessibility)
            {
                this.parameterSymbol = parameterSymbol;
                this.methodAccessibility = methodAccessibility;
            }

            public void AddUsage(ITypeSymbol symbolUsedAs)
            {
                if (usedAs.ContainsKey(symbolUsedAs))
                {
                    usedAs[symbolUsedAs]++;
                }
                else
                {
                    usedAs[symbolUsedAs] = 1;
                }
            }

            public bool MatchesIdentifier(ExpressionSyntax identifier, SemanticModel semanticModel)
            {
                var symbol = semanticModel.GetSymbolInfo(identifier).Symbol;
                return Equals(parameterSymbol, symbol);
            }

            public Diagnostic GetRuleViolation()
            {
                if (!ShouldReportOn)
                {
                    return null;
                }

                var mostGeneralType = FindMostGeneralType();

                return Equals(mostGeneralType, parameterSymbol.Type) || IsIgnoredBaseType(mostGeneralType.GetSymbolType())
                    ? null
                    : CreateDiagnostic(Rule, parameterSymbol.Locations.First(), mostGeneralType.ToDisplayString(), parameterSymbol.Type.ToDisplayString());
            }

            private static bool IsIgnoredBaseType(ITypeSymbol typeSymbol) =>
                typeSymbol.IsAny(KnownType.System_Object, KnownType.System_ValueType, KnownType.System_Enum)
                || typeSymbol.Name.StartsWith("_", StringComparison.Ordinal)
                || IsCollectionOfKeyValuePair(typeSymbol);

            private static bool IsCollectionOfKeyValuePair(ITypeSymbol typeSymbol) =>
                typeSymbol is INamedTypeSymbol namedType
                && namedType.TypeArguments.FirstOrDefault() is INamedTypeSymbol firstGenericType
                && namedType.ConstructedFrom.Is(KnownType.System_Collections_Generic_ICollection_T)
                && firstGenericType.ConstructedFrom.Is(KnownType.System_Collections_Generic_KeyValuePair_TKey_TValue);

            private ISymbol FindMostGeneralType()
            {
                var mostGeneralType = parameterSymbol.Type;

                var multipleEnumerableCalls = usedAs.Where(HasMultipleUseOfIEnumerable).ToList();
                foreach (var v in multipleEnumerableCalls)
                {
                    usedAs.Remove(v.Key);
                }

                if (usedAs.Count == 0)
                {
                    return mostGeneralType;
                }

                mostGeneralType = FindMostGeneralAccessibleClassOrSelf(mostGeneralType);
                mostGeneralType = FindMostGeneralAccessibleInterfaceOrSelf(mostGeneralType);
                return mostGeneralType;

                static bool HasMultipleUseOfIEnumerable(KeyValuePair<ITypeSymbol, int> kvp) =>
                    kvp.Value > 1
                    && (kvp.Key.OriginalDefinition.Is(KnownType.System_Collections_Generic_IEnumerable_T) || kvp.Key.Is(KnownType.System_Collections_IEnumerable));

                ITypeSymbol FindMostGeneralAccessibleClassOrSelf(ITypeSymbol typeSymbol)
                {
                    var currentSymbol = typeSymbol.BaseType;

                    while (currentSymbol != null)
                    {
                        if (DerivesOrImplementsAll(currentSymbol))
                        {
                            typeSymbol = currentSymbol;
                        }

                        currentSymbol = currentSymbol?.BaseType;
                    }

                    return typeSymbol;
                }

                ITypeSymbol FindMostGeneralAccessibleInterfaceOrSelf(ITypeSymbol typeSymbol) =>
                    typeSymbol.Interfaces.FirstOrDefault(DerivesOrImplementsAll) is { } @interface
                        ? FindMostGeneralAccessibleInterfaceOrSelf(@interface)
                        : typeSymbol;
            }

            private bool DerivesOrImplementsAll(ITypeSymbol type)
            {
                return usedAs.Keys.All(type.DerivesOrImplements)
                       && IsConsistentAccessibility(type.GetEffectiveAccessibility());

                bool IsConsistentAccessibility(Accessibility baseTypeAccessibility) =>
                    methodAccessibility switch
                    {
                        Accessibility.Private => true,
                        // ProtectedAndInternal corresponds to `private protected`.
                        Accessibility.ProtectedAndInternal => baseTypeAccessibility is not Accessibility.Private,
                        // ProtectedOrInternal corresponds to `protected internal`.
                        Accessibility.ProtectedOrInternal => baseTypeAccessibility is Accessibility.Public or Accessibility.Internal or Accessibility.ProtectedOrInternal,
                        Accessibility.Protected => baseTypeAccessibility == Accessibility.Public || baseTypeAccessibility == methodAccessibility,
                        Accessibility.Internal => baseTypeAccessibility == Accessibility.Public || baseTypeAccessibility == methodAccessibility,
                        Accessibility.Public => baseTypeAccessibility == Accessibility.Public,
                        _ => false
                    };
            }
        }
    }
}
