/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2020 SonarSource SA
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
using SonarAnalyzer.Common;
using SonarAnalyzer.Helpers;

namespace SonarAnalyzer.Rules.CSharp
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    [Rule(DiagnosticId)]
    public sealed class ReferenceEqualityCheckWhenEqualsExists : SonarDiagnosticAnalyzer
    {
        internal const string DiagnosticId = "S1698";
        private const string MessageFormat = "Consider using 'Equals' if value comparison was intended.";

        private static readonly DiagnosticDescriptor rule =
            DiagnosticDescriptorBuilder.GetDescriptor(DiagnosticId, MessageFormat, RspecStrings.ResourceManager);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(rule);

        private const string EqualsName = "Equals";

        private static readonly ImmutableArray<KnownType> AllowedTypes =
            ImmutableArray.Create(
                KnownType.System_Type,
                KnownType.System_Reflection_Assembly,
                KnownType.System_Reflection_MemberInfo,
                KnownType.System_Reflection_Module,
                KnownType.System_Data_Common_CommandTrees_DbExpression,
                KnownType.System_Object
            );

        private static readonly ImmutableArray<KnownType> AllowedTypesWithAllDerived =
            ImmutableArray.Create(KnownType.System_Windows_DependencyObject);

        protected override void Initialize(SonarAnalysisContext context)
        {
            context.RegisterCompilationStartAction(
                compilationStartContext =>
                {
                    var allNamedTypeSymbols = compilationStartContext.Compilation.GlobalNamespace.GetAllNamedTypes();
                    var allInterfacesWithImplementationsOverriddenEquals =
                        allNamedTypeSymbols
                            .Where(t => t.AllInterfaces.Any() && HasEqualsOverride(t))
                            .SelectMany(t => t.AllInterfaces)
                            .ToHashSet();

                    compilationStartContext.RegisterSyntaxNodeActionInNonGenerated(
                        c =>
                        {
                            var binary = (BinaryExpressionSyntax)c.Node;
                            if (!IsBinaryCandidateForReporting(binary, c.SemanticModel))
                            {
                                return;
                            }

                            var typeLeft = c.SemanticModel.GetTypeInfo(binary.Left).Type;
                            var typeRight = c.SemanticModel.GetTypeInfo(binary.Right).Type;
                            if (typeLeft == null ||
                                typeRight == null ||
                                IsAllowedType(typeLeft) ||
                                IsAllowedType(typeRight))
                            {
                                return;
                            }

                            if (MightOverrideEquals(typeLeft, allInterfacesWithImplementationsOverriddenEquals) ||
                                MightOverrideEquals(typeRight, allInterfacesWithImplementationsOverriddenEquals))
                            {
                                c.ReportDiagnosticWhenActive(Diagnostic.Create(rule, binary.OperatorToken.GetLocation()));
                            }
                        },
                        SyntaxKind.EqualsExpression,
                        SyntaxKind.NotEqualsExpression);
                });
        }

        private static bool MightOverrideEquals(ITypeSymbol type, ISet<INamedTypeSymbol> allInterfacesWithImplementationsOverriddenEquals)
        {
            return HasEqualsOverride(type) ||
                allInterfacesWithImplementationsOverriddenEquals.Contains(type) ||
                HasTypeConstraintsWhichMightOverrideEquals(type, allInterfacesWithImplementationsOverriddenEquals);
        }

        private static bool HasTypeConstraintsWhichMightOverrideEquals(ITypeSymbol type, ISet<INamedTypeSymbol> allInterfacesWithImplementationsOverriddenEquals)
        {
            if (type.TypeKind != TypeKind.TypeParameter)
            {
                return false;
            }

            var typeParameter = (ITypeParameterSymbol)type;
            return typeParameter.ConstraintTypes.Any(t => MightOverrideEquals(t, allInterfacesWithImplementationsOverriddenEquals));
        }

        private static bool IsAllowedType(ITypeSymbol type)
        {
            return type.IsAny(AllowedTypes) || HasAllowedBaseType(type);
        }

        private static bool HasAllowedBaseType(ITypeSymbol type)
        {
            var currentType = type;
            while (currentType != null)
            {
                if (currentType.IsAny(AllowedTypesWithAllDerived))
                {
                    return true;
                }
                currentType = currentType.BaseType;
            }
            return false;
        }

        private static bool IsBinaryCandidateForReporting(BinaryExpressionSyntax binary, SemanticModel semanticModel)
        {
            var equalitySymbol = semanticModel.GetSymbolInfo(binary).Symbol as IMethodSymbol;

            return equalitySymbol.IsInType(KnownType.System_Object) &&
                !IsInEqualsOverride(semanticModel.GetEnclosingSymbol(binary.SpanStart) as IMethodSymbol);
        }

        private static bool HasEqualsOverride(ITypeSymbol type)
        {
            return GetEqualsOverrides(type).Any(m => m.OverriddenMethod.IsInType(KnownType.System_Object));
        }

        private static IEnumerable<IMethodSymbol> GetEqualsOverrides(ITypeSymbol type)
        {
            if (type == null)
            {
                return Enumerable.Empty<IMethodSymbol>();
            }

            var candidateEqualsMethods = new HashSet<IMethodSymbol>();

            var currentType = type;
            while (currentType != null &&
                !currentType.Is(KnownType.System_Object))
            {
                candidateEqualsMethods.UnionWith(currentType.GetMembers(EqualsName)
                    .OfType<IMethodSymbol>()
                    .Where(method => method.IsOverride && method.OverriddenMethod != null));

                currentType = currentType.BaseType;
            }

            return candidateEqualsMethods;
        }

        private static bool IsInEqualsOverride(IMethodSymbol method)
        {
            if (method == null)
            {
                return false;
            }

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
