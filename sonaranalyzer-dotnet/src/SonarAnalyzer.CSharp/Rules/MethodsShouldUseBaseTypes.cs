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
        private const string MessageFormat = "Use '{0}' here; it is a more general type than '{1}'.";

        private static readonly DiagnosticDescriptor rule =
            DiagnosticDescriptorBuilder.GetDescriptor(DiagnosticId, MessageFormat, RspecStrings.ResourceManager);
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(rule);

        protected override void Initialize(SonarAnalysisContext context)
        {
            context.RegisterSyntaxNodeActionInNonGenerated(c =>
            {
                var violations = FindViolations(c).ToList();
                violations.ForEach(c.ReportDiagnostic);
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

            var parametersToCheck = new Dictionary<string, ParameterData>();
            foreach (var parameter in methodSymbol.Parameters.Where(IsTrackedParameter))
            {
                parametersToCheck[parameter.Name] = new ParameterData(parameter);
            }

            var parameterUsesInMethod = context
                .Node
                .DescendantNodes()
                .OfType<IdentifierNameSyntax>()
                .Where(id => parametersToCheck.Values.Any(p => p.MatchesIdentifier(id, context.SemanticModel)))
                .ToArray();

            foreach (var idName in parameterUsesInMethod)
            {
                var key = idName?.Identifier.ValueText ?? "";

                ParameterData paramData;
                if (!parametersToCheck.TryGetValue(key, out paramData) ||
                    !paramData.ShouldReportOn)
                {
                    continue;
                }

                if (idName.Parent is EqualsValueClauseSyntax ||
                    idName.Parent is AssignmentExpressionSyntax)
                {
                    paramData.ShouldReportOn = false;
                    continue;
                }

                var symbolUsedAs = FindParameterUseAsType(idName, context.SemanticModel);
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
            var isExcluded =
                type.DerivesFrom(KnownType.System_Array) ||
                type.IsValueType ||
                type.Is(KnownType.System_String);

            return !isExcluded;
        }

        private ITypeSymbol FindParameterUseAsType(IdentifierNameSyntax idRef, SemanticModel semanticModel)
        {
            if (idRef.Parent is ConditionalAccessExpressionSyntax)
            {
                var cond = idRef.Parent as ConditionalAccessExpressionSyntax;
                var binding = cond.WhenNotNull as MemberBindingExpressionSyntax;

                if (binding != null)
                {
                    var name = binding.Name;
                    var accessedMember = semanticModel.GetSymbolInfo(name).Symbol;

                    var propertySymbol = accessedMember as IPropertySymbol;
                    if (propertySymbol != null)
                    {
                        return HandleProperty(idRef, propertySymbol);
                    }

                    return FindOriginatingSymbol(accessedMember);
                }
                else if (cond.WhenNotNull is InvocationExpressionSyntax)
                {
                    var inv = (cond.WhenNotNull as InvocationExpressionSyntax).Expression as MemberBindingExpressionSyntax;

                    if (inv != null)
                    {
                        var invocationSymbol = semanticModel.GetSymbolInfo(inv).Symbol;
                        return HandleInvocation(idRef, invocationSymbol, semanticModel);
                    }
                }
            }
            else if (idRef.Parent is MemberAccessExpressionSyntax)
            {
                var invocation = idRef.Parent.Parent as InvocationExpressionSyntax;
                if (invocation != null)
                {
                    var invocationSymbol = semanticModel.GetSymbolInfo(idRef.Parent.Parent).Symbol;
                    return HandleInvocation(idRef, invocationSymbol, semanticModel);
                }

                var accessedMember = semanticModel.GetSymbolInfo(idRef.Parent).Symbol;

                var propertySymbol = accessedMember as IPropertySymbol;
                if (propertySymbol != null)
                {
                    return HandleProperty(idRef, propertySymbol);
                }

                return FindOriginatingSymbol(accessedMember);
            }
            else if (idRef.Parent is ArgumentSyntax)
            {
                return semanticModel.GetTypeInfo(idRef).ConvertedType;
            }

            return null;
        }

        private ITypeSymbol HandleProperty(IdentifierNameSyntax idRef, IPropertySymbol propertySymbol)
        {
            return FindOriginatingSymbol(idRef.Parent.Parent is AssignmentExpressionSyntax
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

            if (methodSymbol.IsExtensionMethod)
            {
                return semanticModel.GetTypeInfo(invokedOn).ConvertedType;
            }

            return FindOriginatingSymbol(invocationSymbol);
        }

        private INamedTypeSymbol FindOriginatingSymbol(ISymbol accessedMember)
        {
            if (accessedMember == null)
            {
                return null;
            }

            var originatingInterface = accessedMember.ContainingType
                   .AllInterfaces
                   .Where(i => HasImplementationForMember(i, accessedMember))
                   .FirstOrDefault();

            if (originatingInterface != null)
            {
                return originatingInterface;
            }

            var overridenSymbol = SymbolHelper.GetOverriddenMember(accessedMember);
            return overridenSymbol != null
                ? overridenSymbol.ContainingType
                : accessedMember.ContainingType;
        }

        private static bool HasImplementationForMember(INamedTypeSymbol namedSymbol, ISymbol accessedMember)
        {
            return namedSymbol
                .GetMembers()
                .Any(member =>
                    accessedMember.Equals(
                        accessedMember.ContainingType.FindImplementationForInterfaceMember(member)));
        }

        private class ParameterData
        {
            public bool ShouldReportOn { get; set; } = true;

            private IParameterSymbol parameterSymbol;
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
                if (ShouldReportOn)
                {
                    var mostGeneralType = FindMostGeneralType();

                    if (!Equals(mostGeneralType, parameterSymbol.Type) &&
                        ShouldReportOnType(mostGeneralType.GetSymbolType()))
                    {
                        return Diagnostic.Create(rule,
                            parameterSymbol.Locations.First(),
                            mostGeneralType.ToDisplayString(), parameterSymbol.Type.ToDisplayString());
                    }
                }

                return null;
            }

            private static bool ShouldReportOnType(ITypeSymbol typeSymbol)
            {
                bool isExcluded =
                       typeSymbol.Is(KnownType.System_Object) ||
                       typeSymbol.Is(KnownType.System_ValueType) ||
                       typeSymbol.Name.StartsWith("_", StringComparison.InvariantCulture) ||
                       typeSymbol.Is(KnownType.System_Enum);

                return !isExcluded;
            }

            private ISymbol FindMostGeneralType()
            {
                var mostGeneralType = parameterSymbol.Type;

                if (usedAs.Count == 0)
                {
                    return mostGeneralType;
                }

                mostGeneralType = FindMostGeneralClass(mostGeneralType);
                mostGeneralType = FindMostGeneralInterface(mostGeneralType);
                return mostGeneralType;
            }

            private ITypeSymbol FindMostGeneralClass(ITypeSymbol mostGeneralType)
            {
                ITypeSymbol currentSymbol = mostGeneralType.BaseType;
                do
                {
                    if (DerivesOrImplementsAll(currentSymbol))
                    {
                        mostGeneralType = currentSymbol;
                    }

                    currentSymbol = currentSymbol?.BaseType;
                }
                while (currentSymbol != null);

                return mostGeneralType;
            }

            private ITypeSymbol FindMostGeneralInterface(ITypeSymbol mostGeneralType, int depth = 0)
            {
                if (depth > 10)
                {
                    return mostGeneralType;
                }

                foreach (var iface in mostGeneralType.Interfaces)
                {
                    if (DerivesOrImplementsAll(iface))
                    {
                        return FindMostGeneralInterface(iface, depth++);
                    }
                }

                return mostGeneralType;
            }

            private bool DerivesOrImplementsAll(ITypeSymbol type)
            {
                return type != null &&
                    usedAs.All(u => type.DerivesOrImplements(u));
            }
        }
    }
}
