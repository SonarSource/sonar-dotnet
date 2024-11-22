/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2014-2024 SonarSource SA
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

using System.Reflection;
using static System.Linq.Expressions.Expression;

namespace StyleCop.Analyzers.Lightup;

public static class INamedTypeSymbolExtensionsSonar
{
    private static readonly Func<INamedTypeSymbol, ImmutableArray<NullableAnnotation>> TypeArgumentNullableAnnotationsAccessor;

    static INamedTypeSymbolExtensionsSonar()
    {
        TypeArgumentNullableAnnotationsAccessor = CreateTypeArgumentNullableAnnotationsAccessor();
    }

    public static ImmutableArray<NullableAnnotation> TypeArgumentNullableAnnotations(this INamedTypeSymbol symbol) => TypeArgumentNullableAnnotationsAccessor(symbol);

    private static Func<INamedTypeSymbol, ImmutableArray<NullableAnnotation>> CreateTypeArgumentNullableAnnotationsAccessor()
    {
        // INamedTypeSymbol.TypeArgumentNullableAnnotations is ImmutableArray<Roslyn.NullableAnnotation>
        // The generated code is symbol => ImmutableArray.CreateRange(symbol.TypeArgumentNullableAnnotations, x => (Sonar.NullableAnnotationType)x);

        // Callers may rely on the fact that symbol.TypeArgumentNullableAnnotations is supposed to have the same length as symbol.TypeArguments
        var fallback = static (INamedTypeSymbol x) => Enumerable.Repeat(NullableAnnotation.None, x.TypeArguments.Length).ToImmutableArray();
        if (OriginalNullableAnnotationType() is not { } originalNullableAnnotationType)
        {
            return fallback;
        }

        if (typeof(ImmutableArray).GetMethods(BindingFlags.Public | BindingFlags.Static).FirstOrDefault(x =>
            x.Name == nameof(ImmutableArray.CreateRange)     // https://learn.microsoft.com/en-us/dotnet/api/system.collections.immutable.immutablearray.createrange
            && x.GetParameters() is { Length: 2 } parameters // CreateRange<TSource,TResult>(ImmutableArray<TSource> items, Func<TSource,TResult> selector)
            && parameters[0].Name == "items"                 // see also https://stackoverflow.com/a/4036187
            && parameters[1].Name == "selector"
            && x.GetGenericArguments() is { Length: 2 } typeArguments
            && typeArguments[0].Name == "TSource"
            && typeArguments[1].Name == "TResult") is not { } createRange)
        {
            return fallback;
        }
        var sonarNullableAnnotationType = typeof(NullableAnnotation);
        var createRangeT = createRange.MakeGenericMethod(originalNullableAnnotationType, sonarNullableAnnotationType);
        var delegateType = typeof(Func<,>).MakeGenericType(originalNullableAnnotationType, sonarNullableAnnotationType);

        var originalNullableAnnotationParameter = Parameter(originalNullableAnnotationType, "x");
        var conversion = Lambda(delegateType, Convert(originalNullableAnnotationParameter, sonarNullableAnnotationType), originalNullableAnnotationParameter); // (originalNullableAnnotationType x) => (sonarNullableAnnotationType)x;

        var symbolParameter = Parameter(typeof(INamedTypeSymbol), "symbol");
        return Lambda<Func<INamedTypeSymbol, ImmutableArray<NullableAnnotation>>>(
            Call(createRangeT, Property(symbolParameter, nameof(TypeArgumentNullableAnnotations)), conversion), // ImmutableArray.CreateRange(symbol.TypeArgumentNullableAnnotations, conversion)
            symbolParameter).Compile();
    }

    private static Type OriginalNullableAnnotationType()
    {
        try
        {
            return Type.GetType("Microsoft.CodeAnalysis.NullableAnnotation, Microsoft.CodeAnalysis");
        }
        catch
        {
            return null;
        }
    }
}
