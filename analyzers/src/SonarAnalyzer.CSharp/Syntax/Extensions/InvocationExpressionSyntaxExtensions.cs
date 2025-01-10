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

namespace SonarAnalyzer.CSharp.Syntax.Extensions;

internal static class InvocationExpressionSyntaxExtensions
{
    public static bool HasOverloadWithType(this InvocationExpressionSyntax invocation, SemanticModel model, ImmutableArray<KnownType> types) =>
        model.GetMemberGroup(invocation.Expression)
            .OfType<IMethodSymbol>()
            .Where(x => !x.HasAttribute(KnownType.System_ObsoleteAttribute))
            .Where(x => IsCompatibleOverload(invocation, x))
            .Any(x => SameParametersExceptWantedType(x, InvocationParameters(invocation, model), types));

    // must have same number of arguments + 1 (the argument that should be added) OR is params argument
    private static bool IsCompatibleOverload(InvocationExpressionSyntax invocation, IMethodSymbol m)
    {
        var parameters = m.GetParameters().ToArray();
        return parameters.Length - invocation.ArgumentList.Arguments.Count == 1
            || (parameters.Length != 0 && parameters[parameters.Length - 1].IsParams);
    }

    private static IMethodSymbol InvocationParameters(InvocationExpressionSyntax invocation, SemanticModel model) =>
        model.GetSymbolInfo(invocation.Expression).Symbol as IMethodSymbol;

    private static bool SameParametersExceptWantedType(IMethodSymbol possibleOverload, IMethodSymbol invocationMethodSymbol, ImmutableArray<KnownType> types)
    {
        var withTypeParam = possibleOverload.IsGenericMethod && invocationMethodSymbol.IsGenericMethod
            // attempt to create the possibleOverload method symbol with same type arguments as the invocation method
            ? ConstructTypedPossibleOverload(possibleOverload, invocationMethodSymbol)
            : possibleOverload;
        var invocationParameters = invocationMethodSymbol.GetParameters().ToArray();
        var parametersWithoutWantedType = withTypeParam.GetParameters().Where(x => !x.Type.IsAny(types)).ToArray();
        if (parametersWithoutWantedType.Length == possibleOverload.GetParameters().Count())
        {
            return false;
        }

        if (parametersWithoutWantedType.Length > 0
            && parametersWithoutWantedType.Length <= invocationParameters.Length
            && parametersWithoutWantedType[parametersWithoutWantedType.Length - 1].IsParams)
        {
            // check whether has a parameter array argument which matches the invocationParameters
            return VerifyCompatibility(invocationParameters, parametersWithoutWantedType, parametersWithoutWantedType[parametersWithoutWantedType.Length - 1]);
        }
        else if (invocationParameters.Length == parametersWithoutWantedType.Length)
        {
            // parameters must have the same type
            return invocationParameters.Select((x, index) => x.Type.DerivesOrImplements(parametersWithoutWantedType[index].Type)).All(x => x);
        }
        else
        {
            return false;
        }
    }

    private static IMethodSymbol ConstructTypedPossibleOverload(IMethodSymbol possibleOverload, IMethodSymbol invocationMethodSymbol) =>
        possibleOverload.TypeParameters.Length == invocationMethodSymbol.TypeArguments.Length
            ? possibleOverload.ConstructedFrom.Construct(invocationMethodSymbol.TypeArguments.ToArray())
            : possibleOverload;

    /**
     * Verifies the compatibility between the invocation parameters and the parameters of a possible overload that
     * has the last parameter of 'params' type (variable length parameter).
     */
    private static bool VerifyCompatibility(IParameterSymbol[] invocationParameters, IParameterSymbol[] overloadCandidateParameters, IParameterSymbol paramsParameter)
    {
        if (paramsParameter.Type is not IArrayTypeSymbol)
        {
            return false;
        }
        var i = 0;
        // check parameters before the last parameter
        for (; i < overloadCandidateParameters.Length - 1; i++)
        {
            if (!invocationParameters[i].Type.DerivesOrImplements(overloadCandidateParameters[i].Type))
            {
                return false;
            }
        }
        // make sure the rest of the invocation parameters match with the 'params' type
        var paramsType = ParamsElementType(paramsParameter.Type);
        for (; i < invocationParameters.Length - 1; i++)
        {
            if (!invocationParameters[i].Type.DerivesOrImplements(paramsType))
            {
                return false;
            }
        }

        var lastInvocationParameter = invocationParameters[invocationParameters.Length - 1];
        return lastInvocationParameter.IsParams
            ? ParamsElementType(lastInvocationParameter.Type).DerivesOrImplements(paramsType)
            : lastInvocationParameter.Type.DerivesOrImplements(paramsType);

        static ITypeSymbol ParamsElementType(ITypeSymbol type) =>
            type is IArrayTypeSymbol symbol ? symbol.ElementType : null;
    }
}
