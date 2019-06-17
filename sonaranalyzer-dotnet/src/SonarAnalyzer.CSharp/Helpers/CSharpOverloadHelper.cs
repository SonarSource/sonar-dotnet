/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2019 SonarSource SA
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

namespace SonarAnalyzer.Helpers.CSharp
{
    internal static class CSharpOverloadHelper
    {
        public static bool HasOverloadWithType(InvocationExpressionSyntax invocation, SemanticModel semanticModel, ImmutableArray<KnownType> types) =>
            semanticModel.GetMemberGroup(invocation.Expression)
                .OfType<IMethodSymbol>()
                .Where(m => !m.GetAttributes(KnownType.System_ObsoleteAttribute).Any())
                .Where(method => IsCompatibleOverload(invocation, method))
                .Any(methodSignature => SameParametersExceptWantedType(methodSignature, GetInvocationParameters(invocation, semanticModel), types));

        private static bool SameParametersExceptWantedType(IMethodSymbol possibleOverload, IMethodSymbol invocationMethodSymbol, ImmutableArray<KnownType> types)
        {
            var withTypeParam = possibleOverload;
            if (possibleOverload.IsGenericMethod && invocationMethodSymbol.IsGenericMethod)
            {
                // attempt to create the possibleOverload method symbol with same type arguments as the invocation method
                withTypeParam = ConstructTypedPossibleOverload(possibleOverload, invocationMethodSymbol);
            }

            var invocationParameters = invocationMethodSymbol.GetParameters();
            var parametersWithoutWantedType = withTypeParam.GetParameters().Where(p => !p.Type.IsAny(types)).ToList();
            var parametersWithoutWantedTypeCount = parametersWithoutWantedType.Count;

            if (parametersWithoutWantedTypeCount == possibleOverload.GetParameters().Count())
            {
                return false;
            }

            var invocationParametersCount = invocationParameters.Count();
            if (parametersWithoutWantedTypeCount <= invocationParametersCount &&
                parametersWithoutWantedTypeCount > 0 &&
                parametersWithoutWantedType.Last().IsParams)
            {
                // check whether has a parameter array argument which matches the invocationParameters
                var lastIndex = parametersWithoutWantedType.Count - 1;
                return VerifyCompatibility(invocationParameters.ToList(), parametersWithoutWantedType, parametersWithoutWantedType[lastIndex]);
            }
            else if (invocationParametersCount == parametersWithoutWantedTypeCount)
            {
                // parameters must have the same type
                return invocationParameters
                    .Select((p, index) => p.Type.DerivesOrImplements(parametersWithoutWantedType[index].Type))
                    .All(isCompatible => isCompatible);

            }
            return false;
        }

        private static IMethodSymbol ConstructTypedPossibleOverload(IMethodSymbol possibleOverload, IMethodSymbol invocationMethodSymbol)
        {
            if (possibleOverload.TypeParameters.Count() == invocationMethodSymbol.TypeArguments.Count())
            {
                return possibleOverload.Construct(invocationMethodSymbol.TypeArguments.ToArray());
            }

            return possibleOverload;
        }

        // must have same number of arguments + 1 (the argument that should be added) OR is params argument
        public static bool IsCompatibleOverload(InvocationExpressionSyntax invocation, IMethodSymbol m) =>
                (m.GetParameters().Count() - invocation.ArgumentList.Arguments.Count == 1) ||
                (m.GetParameters().Any() && m.GetParameters().Last().IsParams);

        public static IMethodSymbol GetInvocationParameters(InvocationExpressionSyntax invocation, SemanticModel semanticModel) =>
            semanticModel.GetSymbolInfo(invocation.Expression).Symbol as IMethodSymbol;

        /**
         * Verifies the compatibility between the invocation parameters and the parameters of a possible overload that
         * has the last parameter of 'params' type (variable length parameter).
         */
        private static bool VerifyCompatibility(IList<IParameterSymbol> invocationParameters, IList<IParameterSymbol> overloadCandidateParameters, IParameterSymbol paramsParameter)
        {
            if (!(paramsParameter.Type is IArrayTypeSymbol))
            {
                return false;
            }
            var i = 0;
            // check parameters before the last parameter
            for (; i < overloadCandidateParameters.Count - 1; i++)
            {
                if (!invocationParameters[i].Type.DerivesOrImplements(overloadCandidateParameters[i].Type))
                {
                    return false;
                }
            }
            // make sure the rest of the invocation parameters match with the 'params' type
            var paramsType = GetParamsElementType(paramsParameter.Type);
            for (; i < invocationParameters.Count - 1; i++)
            {
                if (!invocationParameters[i].Type.DerivesOrImplements(paramsType))
                {
                    return false;
                }
            }

            var lastInvocationParameter = invocationParameters[invocationParameters.Count - 1];
            if (lastInvocationParameter.IsParams)
            {
                return GetParamsElementType(lastInvocationParameter.Type).DerivesOrImplements(paramsType);
            }
            else
            {
                return lastInvocationParameter.Type.DerivesOrImplements(paramsType);
            }

            ITypeSymbol GetParamsElementType(ITypeSymbol typeSymbol) => (typeSymbol as IArrayTypeSymbol).ElementType;
        }
    }
}
