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
                    .ForEach(c.ReportDiagnostic);
            },
            SyntaxKind.MethodDeclaration);
        }

        public IEnumerable<Diagnostic> FindViolations(SyntaxNodeAnalysisContext context)
        {
            var methodSymbol = context.SemanticModel.GetDeclaredSymbol(context.Node) as IMethodSymbol;

            if (methodSymbol == null ||
                methodSymbol.Parameters.Length == 0 ||
                methodSymbol.IsOverride)
            {
                return Enumerable.Empty<Diagnostic>();
            }

            var parametersToCheck = methodSymbol.Parameters
                .Where(IsTrackedParameter)
                .ToDictionary(p => p.Name, p => new ParameterData(p));

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

        private class ParameterData
        {
            public bool ShouldReportOn { get; set; } = true;

            private readonly IParameterSymbol parameterSymbol;
            private readonly HashSet<ITypeSymbol> usedAs = new HashSet<ITypeSymbol>();

            public ParameterData(IParameterSymbol parameterSymbol)
            {
                this.parameterSymbol = parameterSymbol;
            }

            public void AddUsage(ITypeSymbol symbolUsedAs)
            {
                usedAs.Add(symbolUsedAs);
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
                    ShouldReportOnType(mostGeneralType.GetSymbolType()))
                {
                    return Diagnostic.Create(rule,
                        parameterSymbol.Locations.First(),
                        mostGeneralType.ToDisplayString(), parameterSymbol.Type.ToDisplayString());
                }

                return null;
            }

            private static bool ShouldReportOnType(ITypeSymbol typeSymbol)
            {
                return
                       !typeSymbol.Is(KnownType.System_Object) &&
                       !typeSymbol.Is(KnownType.System_ValueType) &&
                       !typeSymbol.Name.StartsWith("_", StringComparison.Ordinal) &&
                       !typeSymbol.Is(KnownType.System_Enum);
            }

            private ISymbol FindMostGeneralType()
            {
                var mostGeneralType = parameterSymbol.Type;

                if (usedAs.Count == 0)
                {
                    return mostGeneralType;
                }

                mostGeneralType = FindMostGeneralClassOrSelf(mostGeneralType);
                mostGeneralType = FindMostGeneralInterfaceOrSelf(mostGeneralType);
                return mostGeneralType;
            }

            private ITypeSymbol FindMostGeneralClassOrSelf(ITypeSymbol mostGeneralType)
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

            private ITypeSymbol FindMostGeneralInterfaceOrSelf(ITypeSymbol mostGeneralType)
            {
                foreach (var @interface in mostGeneralType.Interfaces)
                {
                    if (DerivesOrImplementsAll(@interface))
                    {
                        return FindMostGeneralInterfaceOrSelf(@interface);
                    }
                }

                return mostGeneralType;
            }

            private bool DerivesOrImplementsAll(ITypeSymbol type)
            {
                return type != null && usedAs.All(type.DerivesOrImplements);
            }
        }
    }
}
