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
    public sealed class GenericTypeParameterInOut : SonarDiagnosticAnalyzer
    {
        private const string DiagnosticId = "S3246";
        private const string MessageFormat = "Add the '{0}' keyword to parameter '{1}' to make it '{2}'.";

        private static readonly DiagnosticDescriptor Rule = DescriptorFactory.Create(DiagnosticId, MessageFormat);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(Rule);

        protected override void Initialize(SonarAnalysisContext context)
        {
            context.RegisterNodeAction(c => CheckInterfaceVariance(c, (InterfaceDeclarationSyntax)c.Node), SyntaxKind.InterfaceDeclaration);
            context.RegisterNodeAction(c => CheckDelegateVariance(c, (DelegateDeclarationSyntax)c.Node), SyntaxKind.DelegateDeclaration);
        }

        #region Top level

        private static void CheckInterfaceVariance(SonarSyntaxNodeReportingContext context, InterfaceDeclarationSyntax declaration)
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
                    ReportIssue(context, typeParameter, canBeIn ? VarianceKind.In : VarianceKind.Out);
                }
            }
        }
        private static void CheckDelegateVariance(SonarSyntaxNodeReportingContext context, DelegateDeclarationSyntax declaration)
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
                var canBeIn = CheckTypeParameter(typeParameter, VarianceKind.In, returnType, parameterSymbols);
                var canBeOut = CheckTypeParameter(typeParameter, VarianceKind.Out, returnType, parameterSymbols);

                if (canBeIn ^ canBeOut)
                {
                    ReportIssue(context, typeParameter, canBeIn ? VarianceKind.In : VarianceKind.Out);
                }
            }
        }

        #endregion

        #region Top level per type parameter

        private static bool CheckTypeParameter(ITypeParameterSymbol typeParameter,
                                               VarianceKind variance,
                                               ITypeSymbol returnType,
                                               ImmutableArray<IParameterSymbol> parameters)
        {
            var canBe = CheckTypeParameterConstraintsInSymbol(typeParameter, variance);
            if (!canBe)
            {
                return false;
            }

            canBe = CanTypeParameterBeVariant(typeParameter, variance, returnType,
                true, false);

            if (!canBe)
            {
                return false;
            }

            canBe = CheckTypeParameterInParameters(typeParameter, variance, parameters);
            return canBe;
        }

        private static bool CheckTypeParameter(ITypeParameterSymbol typeParameter, VarianceKind variance, ITypeSymbol interfaceType)
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
                    false);

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

        private static void ReportIssue(SonarSyntaxNodeReportingContext context, ITypeParameterSymbol typeParameter, VarianceKind variance)
        {
            if (!typeParameter.DeclaringSyntaxReferences.Any())
            {
                return;
            }

            var location = typeParameter.DeclaringSyntaxReferences.First().GetSyntax().GetLocation();

            if (variance == VarianceKind.In)
            {
                context.ReportIssue(CreateDiagnostic(Rule, location, "in", typeParameter.Name, "contravariant"));
                return;
            }

            if (variance == VarianceKind.Out)
            {
                context.ReportIssue(CreateDiagnostic(Rule, location, "out", typeParameter.Name, "covariant"));
            }
        }

        #region Check type parameters method/event/parameters

        private static bool CheckTypeParameterInMethod(ITypeParameterSymbol typeParameter, VarianceKind variance, IMethodSymbol method)
        {
            var canBe = CheckTypeParameterConstraintsInSymbol(typeParameter, variance);
            if (!canBe)
            {
                return false;
            }


            canBe = CanTypeParameterBeVariant(
                typeParameter, variance,
                method.ReturnType,
                true,
                false);

            if (!canBe)
            {
                return false;
            }

            return CheckTypeParameterInParameters(typeParameter, variance, method.Parameters);
        }

        private static bool CheckTypeParameterInEvent(ITypeParameterSymbol typeParameter, VarianceKind variance, IEventSymbol @event) =>
            CanTypeParameterBeVariant(typeParameter, variance, @event.Type, false, true);

        private static bool CheckTypeParameterInParameters(ITypeParameterSymbol typeParameter, VarianceKind variance, ImmutableArray<IParameterSymbol> parameters)
        {
            foreach (var param in parameters)
            {
                var canBe = CanTypeParameterBeVariant(typeParameter, variance, param.Type, param.RefKind != RefKind.None, true);
                if (!canBe)
                {
                    return false;
                }
            }
            return true;
        }

        private static bool CheckTypeParameterConstraintsInSymbol(ITypeParameterSymbol typeParameter, VarianceKind variance)
        {
            foreach (var constraintType in typeParameter.ConstraintTypes)
            {
                var canBe = CanTypeParameterBeVariant(typeParameter, variance, constraintType, false, true);
                if (!canBe)
                {
                    return false;
                }
            }
            return true;
        }

        #endregion

        #region Check type parameter variance low level

        private static bool CanTypeParameterBeVariant(ITypeParameterSymbol parameter,
                                                      VarianceKind variance,
                                                      ITypeSymbol type,
                                                      bool requireOutputSafety,
                                                      bool requireInputSafety)
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
                    return CanTypeParameterBeVariant(parameter, variance, ((IArrayTypeSymbol)type).ElementType, requireOutputSafety, requireInputSafety);
                case SymbolKind.ErrorType:
                case SymbolKind.NamedType:
                    return CanTypeParameterBeVariant(parameter, variance, (INamedTypeSymbol)type, requireOutputSafety, requireInputSafety);
                default:
                    return true;
            }
        }

        private static bool CanTypeParameterBeVariant(ITypeParameterSymbol parameter,
                                                      VarianceKind variance,
                                                      INamedTypeSymbol namedType,
                                                      bool requireOutputSafety,
                                                      bool requireInputSafety)
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

            if (namedType.IsTupleType())
            {
                return false;
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

                    if (!CanTypeParameterBeVariant(parameter, variance, typeArg, requireOut, requireIn))
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
