/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2018 SonarSource SA
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
    public sealed class GenericTypeParameterInOut : SonarDiagnosticAnalyzer
    {
        internal const string DiagnosticId = "S3246";
        private const string MessageFormat = "Add the '{0}' keyword to parameter '{1}' to make it '{2}'.";

        private static readonly DiagnosticDescriptor rule =
            DiagnosticDescriptorBuilder.GetDescriptor(DiagnosticId, MessageFormat, RspecStrings.ResourceManager);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(rule);

        protected override void Initialize(SonarAnalysisContext context)
        {
            context.RegisterSyntaxNodeActionInNonGenerated(
                c => CheckInterfaceVariance((InterfaceDeclarationSyntax)c.Node, c),
                SyntaxKind.InterfaceDeclaration);

            context.RegisterSyntaxNodeActionInNonGenerated(
                c => CheckDelegateVariance((DelegateDeclarationSyntax)c.Node, c),
                SyntaxKind.DelegateDeclaration);
        }

        #region Top level

        private static void CheckInterfaceVariance(InterfaceDeclarationSyntax declaration, SyntaxNodeAnalysisContext context)
        {
            var interfaceType = context.SemanticModel.GetDeclaredSymbol(declaration);
            if (interfaceType == null)
            {
                return;
            }

            foreach (var typeParameter in interfaceType.TypeParameters
                .Where(typeParameter => typeParameter.Variance == VarianceKind.None))
            {
                var canBeIn = CheckTypeParameter(typeParameter, VarianceKind.In, interfaceType);
                var canBeOut = CheckTypeParameter(typeParameter, VarianceKind.Out, interfaceType);

                if (canBeIn ^ canBeOut)
                {
                    ReportIssue(typeParameter, canBeIn ? VarianceKind.In : VarianceKind.Out, context);
                }
            }
        }
        private static void CheckDelegateVariance(DelegateDeclarationSyntax declaration, SyntaxNodeAnalysisContext context)
        {
            var declaredSymbol = context.SemanticModel.GetDeclaredSymbol(declaration);
            if (declaredSymbol == null)
            {
                return;
            }

            var returnType = context.SemanticModel.GetTypeInfo(declaration.ReturnType).Type;
            if (returnType == null)
            {
                return;
            }

            var parameterSymbols = declaration.ParameterList == null
                ? ImmutableArray<IParameterSymbol>.Empty
                : declaration.ParameterList.Parameters
                    .Select(p => context.SemanticModel.GetDeclaredSymbol(p))
                    .ToImmutableArray();
            if (parameterSymbols.Any(parameter => parameter == null))
            {
                return;
            }

            foreach (var typeParameter in declaredSymbol.TypeParameters
                .Where(typeParameter => typeParameter.Variance == VarianceKind.None))
            {
                var canBeIn = CheckTypeParameter(typeParameter, VarianceKind.In, declaredSymbol, returnType, parameterSymbols);
                var canBeOut = CheckTypeParameter(typeParameter, VarianceKind.Out, declaredSymbol, returnType, parameterSymbols);

                if (canBeIn ^ canBeOut)
                {
                    ReportIssue(typeParameter, canBeIn ? VarianceKind.In : VarianceKind.Out, context);
                }
            }
        }

        #endregion

        #region Top level per type parameter

        private static bool CheckTypeParameter(ITypeParameterSymbol typeParameter, VarianceKind variance,
            INamedTypeSymbol delegateType, ITypeSymbol returnType, ImmutableArray<IParameterSymbol> parameters)
        {
            var canBe = CheckTypeParameterContraintsInSymbol(typeParameter, variance, delegateType);
            if (!canBe)
            {
                return false;
            }

            canBe = CanTypeParameterBeVariant(typeParameter, variance, returnType,
                true, false, delegateType);

            if (!canBe)
            {
                return false;
            }

            canBe = CheckTypeParameterInParameters(typeParameter, variance, parameters, delegateType);
            return canBe;
        }
        private static bool CheckTypeParameter(ITypeParameterSymbol typeParameter, VarianceKind variance,
            INamedTypeSymbol interfaceType)
        {
            if (typeParameter.Variance != VarianceKind.None)
            {
                return false;
            }

            foreach (var baseInterface in interfaceType.AllInterfaces)
            {
                var canBeVariant = CanTypeParameterBeVariant(
                    typeParameter, variance,
                    baseInterface,
                    true,
                    false,
                    baseInterface);

                if (!canBeVariant)
                {
                    return false;
                }
            }

            foreach (var member in interfaceType.GetMembers())
            {
                bool canBeVariant;

                if (member.Kind == SymbolKind.Method)
                {
                    canBeVariant = CheckTypeParameterInMethod(typeParameter, variance, (IMethodSymbol)member);
                    if (!canBeVariant)
                    {
                        return false;
                    }
                }
                else if (member.Kind == SymbolKind.Event)
                {
                    canBeVariant = CheckTypeParameterInEvent(typeParameter, variance, (IEventSymbol)member);
                    if (!canBeVariant)
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        #endregion

        private static void ReportIssue(ITypeParameterSymbol typeParameter, VarianceKind variance, SyntaxNodeAnalysisContext context)
        {
            if (!typeParameter.DeclaringSyntaxReferences.Any())
            {
                return;
            }

            var location = typeParameter.DeclaringSyntaxReferences.First().GetSyntax().GetLocation();

            if (variance == VarianceKind.In)
            {
                context.ReportDiagnosticWhenActive(Diagnostic.Create(rule, location, "in", typeParameter.Name, "contravariant"));
                return;
            }

            if (variance == VarianceKind.Out)
            {
                context.ReportDiagnosticWhenActive(Diagnostic.Create(rule, location, "out", typeParameter.Name, "covariant"));
            }
        }

        #region Check type parameters method/event/parameters

        private static bool CheckTypeParameterInMethod(ITypeParameterSymbol typeParameter, VarianceKind variance,
            IMethodSymbol method)
        {
            var canBe = CheckTypeParameterContraintsInSymbol(typeParameter, variance, method);
            if (!canBe)
            {
                return false;
            }

            canBe = CanTypeParameterBeVariant(
                typeParameter, variance,
                method.ReturnType,
                true,
                false,
                method);

            if (!canBe)
            {
                return false;
            }

            return CheckTypeParameterInParameters(typeParameter, variance, method.Parameters, method);
        }

        private static bool CheckTypeParameterInEvent(ITypeParameterSymbol typeParameter, VarianceKind variance,
            IEventSymbol @event)
        {
            return CanTypeParameterBeVariant(
                typeParameter, variance,
                @event.Type,
                false,
                true,
                @event);
        }

        private static bool CheckTypeParameterInParameters(ITypeParameterSymbol typeParameter, VarianceKind variance,
            ImmutableArray<IParameterSymbol> parameters, ISymbol context)
        {
            foreach (var param in parameters)
            {
                var canBe = CanTypeParameterBeVariant(
                    typeParameter, variance,
                    param.Type,
                    param.RefKind != RefKind.None,
                    true,
                    context);

                if (!canBe)
                {
                    return false;
                }
            }
            return true;
        }

        private static bool CheckTypeParameterContraintsInSymbol(ITypeParameterSymbol typeParameter, VarianceKind variance,
            ISymbol context)
        {
            foreach (var constraintType in typeParameter.ConstraintTypes)
            {
                var canBe = CanTypeParameterBeVariant(
                    typeParameter,
                    variance,
                    constraintType,
                    false,
                    true,
                    context);

                if (!canBe)
                {
                    return false;
                }
            }
            return true;
        }

        #endregion

        #region Check type parameter variance low level

        private static bool CanTypeParameterBeVariant(
            ITypeParameterSymbol parameter,
            VarianceKind variance,
            ITypeSymbol type,
            bool requireOutputSafety,
            bool requireInputSafety,
            ISymbol context)
        {
            switch (type.Kind)
            {
                case SymbolKind.TypeParameter:
                    var typeParam = (ITypeParameterSymbol)type;
                    if (!typeParam.Equals(parameter))
                    {
                        return true;
                    }

                    return !((requireInputSafety && requireOutputSafety && variance != VarianceKind.None) ||
                        (requireOutputSafety && variance == VarianceKind.In) ||
                        (requireInputSafety && variance == VarianceKind.Out));
                case SymbolKind.ArrayType:
                    return CanTypeParameterBeVariant(parameter, variance, ((IArrayTypeSymbol)type).ElementType, requireOutputSafety, requireInputSafety, context);
                case SymbolKind.ErrorType:
                case SymbolKind.NamedType:
                    return CanTypeParameterBeVariant(parameter, variance, (INamedTypeSymbol)type, requireOutputSafety, requireInputSafety, context);
                default:
                    return true;
            }
        }

        private static bool CanTypeParameterBeVariant(
            ITypeParameterSymbol parameter,
            VarianceKind variance,
            INamedTypeSymbol namedType,
            bool requireOutputSafety,
            bool requireInputSafety,
            ISymbol context)
        {

            switch (namedType.TypeKind)
            {
                case TypeKind.Class:
                case TypeKind.Struct:
                case TypeKind.Enum:
                case TypeKind.Interface:
                case TypeKind.Delegate:
                case TypeKind.Error:
                    break;
                default:
                    return true;
            }

            var currentNamedType = namedType;
            while (currentNamedType != null)
            {
                for (var i = 0; i < currentNamedType.Arity; i++)
                {
                    var typeParam = currentNamedType.TypeParameters[i];
                    var typeArg = currentNamedType.TypeArguments[i];

                    if (!typeArg.Equals(parameter))
                    {
                        return false;
                    }

                    bool requireOut;
                    bool requireIn;

                    switch (typeParam.Variance)
                    {
                        case VarianceKind.Out:
                            requireOut = requireOutputSafety;
                            requireIn = requireInputSafety;
                            break;
                        case VarianceKind.In:
                            requireOut = requireInputSafety;
                            requireIn = requireOutputSafety;
                            break;
                        case VarianceKind.None:
                            requireIn = true;
                            requireOut = true;
                            break;
                        default:
                            throw new NotSupportedException();
                    }

                    if (!CanTypeParameterBeVariant(parameter, variance, typeArg, requireOut, requireIn, context))
                    {
                        return false;
                    }
                }

                currentNamedType = currentNamedType.ContainingType;
            }

            return true;
        }

        #endregion
    }
}
