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

using System;
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
    public sealed class MethodsShouldUseBaseTypes : SonarDiagnosticAnalyzer
    {
        internal const string DiagnosticId = "S3242";
        private const string MessageFormat = "Consider using more general type '{0}' instead of '{1}'.";

        private static readonly DiagnosticDescriptor rule =
            DiagnosticDescriptorBuilder.GetDescriptor(DiagnosticId, MessageFormat, RspecStrings.ResourceManager);
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(rule);

        protected override void Initialize(SonarAnalysisContext context)
        {
            context.RegisterSyntaxNodeActionInNonGenerated(c =>
            {
                FindViolations(c)
                    .ToList()
                    .ForEach(d => c.ReportDiagnosticWhenActive(d));
            },
            SyntaxKind.MethodDeclaration);
        }

        public IEnumerable<Diagnostic> FindViolations(SyntaxNodeAnalysisContext context)
        {
            var methodSymbol = context.SemanticModel.GetDeclaredSymbol(context.Node) as IMethodSymbol;

            if (methodSymbol == null ||
                methodSymbol.Parameters.Length == 0 ||
                methodSymbol.IsOverride ||
                methodSymbol.GetInterfaceMember() != null ||
                IsEventHandlerImplementation(methodSymbol))
            {
                return Enumerable.Empty<Diagnostic>();
            }

            var methodAccessibility = methodSymbol.GetEffectiveAccessibility();
            // The GroupBy is useless in most of the cases but safe-guard in case of 2+ parameters with same name (invalid code).
            // In this case we analyze only the first parameter (a new analysis will be triggered after fixing the names).
            var parametersToCheck = methodSymbol.Parameters
                .Where(IsTrackedParameter)
                .GroupBy(p => p.Name)
                .ToDictionary(p => p.Key, p => new ParameterData(p.First(), methodAccessibility));

            var parameterUsesInMethod = context
                .Node
                .DescendantNodes()
                .OfType<IdentifierNameSyntax>()
                .Where(id => parametersToCheck.Values.Any(p => p.MatchesIdentifier(id, context.SemanticModel)));

            foreach (var identifierReference in parameterUsesInMethod)
            {
                var key = identifierReference.Identifier.ValueText ?? "";

                ParameterData paramData;
                if (!parametersToCheck.TryGetValue(key, out paramData) ||
                    !paramData.ShouldReportOn)
                {
                    continue;
                }

                if (identifierReference.Parent is EqualsValueClauseSyntax ||
                    identifierReference.Parent is AssignmentExpressionSyntax)
                {
                    paramData.ShouldReportOn = false;
                    continue;
                }

                var symbolUsedAs = FindParameterUseAsType(identifierReference, context.SemanticModel);
                if (symbolUsedAs != null)
                {
                    paramData.AddUsage(symbolUsedAs);
                }
            }

            return parametersToCheck
                .Values
                .Select(p => p.GetRuleViolation())
                .WhereNotNull();
        }

        private static bool IsTrackedParameter(IParameterSymbol parameterSymbol)
        {
            var type = parameterSymbol.Type;

            return !type.DerivesFrom(KnownType.System_Array) &&
                   !type.IsValueType &&
                   !type.Is(KnownType.System_String);
        }

        private SyntaxNode GetNextUnparenthesizedParent(SyntaxNode node)
        {
            var expression = node as ExpressionSyntax;
            if (expression == null)
            {
                return node;
            }

            var topmostParent = SyntaxHelper.GetSelfOrTopParenthesizedExpression(expression);
            return topmostParent.Parent;
        }

        private ITypeSymbol FindParameterUseAsType(IdentifierNameSyntax identifier, SemanticModel semanticModel)
        {
            var identifierParent = GetNextUnparenthesizedParent(identifier);

            if (identifierParent is ConditionalAccessExpressionSyntax)
            {
                var conditionalAccess = (ConditionalAccessExpressionSyntax)identifierParent;
                var binding = conditionalAccess.WhenNotNull as MemberBindingExpressionSyntax;

                if (binding != null)
                {
                    var name = binding.Name;
                    if (name == null)
                    {
                        return null;
                    }

                    var accessedMember = semanticModel.GetSymbolInfo(name).Symbol;
                    return HandlePropertyOrField(identifier, accessedMember);
                }

                var invocationExpression = conditionalAccess.WhenNotNull as InvocationExpressionSyntax;
                if (invocationExpression != null)
                {
                    var memberBinding = invocationExpression.Expression as MemberBindingExpressionSyntax;
                    if (memberBinding != null)
                    {
                        var invocationSymbol = semanticModel.GetSymbolInfo(memberBinding).Symbol;
                        return HandleInvocation(identifier, invocationSymbol, semanticModel);
                    }
                }
            }
            else if (identifierParent is MemberAccessExpressionSyntax)
            {
                var invocationExpression = GetNextUnparenthesizedParent(identifierParent) as InvocationExpressionSyntax;
                if (invocationExpression != null)
                {
                    var invocationSymbol = semanticModel.GetSymbolInfo(invocationExpression).Symbol;
                    return HandleInvocation(identifier, invocationSymbol, semanticModel);
                }

                var accessedMember = semanticModel.GetSymbolInfo(identifierParent).Symbol;
                return HandlePropertyOrField(identifier, accessedMember);
            }
            else if (identifierParent is ArgumentSyntax)
            {
                return semanticModel.GetTypeInfo(identifier).ConvertedType;
            }
            else if (identifierParent is ElementAccessExpressionSyntax)
            {
                var accessedMember = semanticModel.GetSymbolInfo(identifierParent).Symbol;
                return HandlePropertyOrField(identifier, accessedMember);
            }
            else
            {
                // nothing to do
            }

            return null;
        }

        private ITypeSymbol HandlePropertyOrField(IdentifierNameSyntax identifier, ISymbol symbol)
        {
            var propertySymbol = symbol as IPropertySymbol;

            if (propertySymbol == null)
            {
                return FindOriginatingSymbol(symbol);
            }

            var parent = GetNextUnparenthesizedParent(identifier);
            var grandParent = GetNextUnparenthesizedParent(parent);

            return FindOriginatingSymbol(grandParent is AssignmentExpressionSyntax
                    ? propertySymbol.SetMethod
                    : propertySymbol.GetMethod);
        }

        private ITypeSymbol HandleInvocation(IdentifierNameSyntax invokedOn, ISymbol invocationSymbol,
            SemanticModel semanticModel)
        {
            var methodSymbol = invocationSymbol as IMethodSymbol;
            if (methodSymbol == null)
            {
                return null;
            }

            return methodSymbol.IsExtensionMethod
                ? semanticModel.GetTypeInfo(invokedOn).ConvertedType
                : FindOriginatingSymbol(invocationSymbol);
        }

        private INamedTypeSymbol FindOriginatingSymbol(ISymbol accessedMember)
        {
            if (accessedMember == null)
            {
                return null;
            }

            var originatingInterface = accessedMember.GetInterfaceMember()?.ContainingType;
            if (originatingInterface != null)
            {
                return originatingInterface;
            }

            var overridenSymbol = SymbolHelper.GetOverriddenMember(accessedMember);
            return overridenSymbol != null
                ? overridenSymbol.ContainingType
                : accessedMember.ContainingType;
        }

        private bool IsEventHandlerImplementation(IMethodSymbol methodSymbol)
        {
            // Cannot find a way to precisely tell whether this method is a event handler implementation
            // without doing too much browsing in all trees.
            // Let's go for a simple heuristic.
            return methodSymbol.ReturnsVoid &&
                methodSymbol.Parameters.Length == 2 &&
                methodSymbol.Parameters[0].Name == "sender" &&
                methodSymbol.Parameters[0].Type.Is(KnownType.System_Object);
        }

        private class ParameterData
        {
            public bool ShouldReportOn { get; set; } = true;

            private readonly IParameterSymbol parameterSymbol;
            private readonly Accessibility methodAccessibility;
            private readonly Dictionary<ITypeSymbol, int> usedAs = new Dictionary<ITypeSymbol, int>();

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

            public bool MatchesIdentifier(IdentifierNameSyntax id, SemanticModel semanticModel)
            {
                var symbol = semanticModel.GetSymbolInfo(id).Symbol;
                return Equals(parameterSymbol, symbol);
            }

            public Diagnostic GetRuleViolation()
            {
                if (!ShouldReportOn)
                {
                    return null;
                }

                var mostGeneralType = FindMostGeneralType();

                if (!Equals(mostGeneralType, parameterSymbol.Type) &&
                    CanSuggestBaseType(mostGeneralType.GetSymbolType()))
                {
                    return Diagnostic.Create(rule,
                        parameterSymbol.Locations.First(),
                        mostGeneralType.ToDisplayString(), parameterSymbol.Type.ToDisplayString());
                }

                return null;
            }

            private static bool CanSuggestBaseType(ITypeSymbol typeSymbol)
            {
                return
                    !typeSymbol.Is(KnownType.System_Object) &&
                    !typeSymbol.Is(KnownType.System_ValueType) &&
                    !typeSymbol.Name.StartsWith("_", StringComparison.Ordinal) &&
                    !typeSymbol.Is(KnownType.System_Enum) &&
                    !IsCollectionKvp(typeSymbol);
            }

            private static bool IsCollectionKvp(ITypeSymbol typeSymbol)
            {
                var namedType = typeSymbol as INamedTypeSymbol;
                var firstGenericType = namedType?.TypeArguments.FirstOrDefault() as INamedTypeSymbol;

                return namedType != null &&
                    firstGenericType != null &&
                    namedType.ConstructedFrom.Is(KnownType.System_Collections_Generic_ICollection_T) &&
                    firstGenericType.ConstructedFrom.Is(KnownType.System_Collections_Generic_KeyValuePair_TKey_TValue);
            }

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
            }

            private static bool HasMultipleUseOfIEnumerable(KeyValuePair<ITypeSymbol, int> kvp)
            {
                return kvp.Value > 1 &&
                    (kvp.Key.OriginalDefinition.Is(KnownType.System_Collections_Generic_IEnumerable_T) ||
                     kvp.Key.Is(KnownType.System_Collections_IEnumerable));
            }

            private ITypeSymbol FindMostGeneralAccessibleClassOrSelf(ITypeSymbol mostGeneralType)
            {
                ITypeSymbol currentSymbol = mostGeneralType.BaseType;

                while (currentSymbol != null)
                {
                    if (DerivesOrImplementsAll(currentSymbol))
                    {
                        mostGeneralType = currentSymbol;
                    }

                    currentSymbol = currentSymbol?.BaseType;
                }

                return mostGeneralType;
            }

            private ITypeSymbol FindMostGeneralAccessibleInterfaceOrSelf(ITypeSymbol mostGeneralType)
            {
                foreach (var @interface in mostGeneralType.Interfaces)
                {
                    if (DerivesOrImplementsAll(@interface))
                    {
                        return FindMostGeneralAccessibleInterfaceOrSelf(@interface);
                    }
                }

                return mostGeneralType;
            }

            private bool DerivesOrImplementsAll(ITypeSymbol type)
            {
                return type != null &&
                    usedAs.Keys.All(type.DerivesOrImplements) &&
                    IsConsistentAccessibility(type.GetEffectiveAccessibility());
            }

            private bool IsConsistentAccessibility(Accessibility baseTypeAccessibility)
            {
                switch (this.methodAccessibility)
                {
                    case Accessibility.NotApplicable:
                        return false;

                    case Accessibility.Private:
                        return true;

                    case Accessibility.Protected:
                    case Accessibility.Internal:
                        return baseTypeAccessibility == Accessibility.Public ||
                            baseTypeAccessibility == this.methodAccessibility;

                    case Accessibility.ProtectedAndInternal:
                    case Accessibility.Public:
                        return baseTypeAccessibility == Accessibility.Public;

                    default:
                        return false;
                }
            }
        }
    }
}
