/*
 * SonarAnalyzer for .NET
 * Copyright (C) SonarSource Sàrl
 * mailto:info AT sonarsource DOT com
 *
 * You can redistribute and/or modify this program under the terms of
 * the Sonar Source-Available License Version 1, as published by SonarSource Sàrl.
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
        // Builds symbol => ImmutableArray.CreateRange(symbol.TypeArgumentNullableAnnotations, x => (Sonar.NullableAnnotation)x)
        // INamedTypeSymbol.TypeArgumentNullableAnnotations was added after NullableAnnotation itself, so a host can have one without the other.
        if (Type.GetType("Microsoft.CodeAnalysis.NullableAnnotation, Microsoft.CodeAnalysis") is { } originalNullableAnnotationType
            && typeof(INamedTypeSymbol).GetProperty(nameof(TypeArgumentNullableAnnotations)) is { } typeArgumentNullableAnnotationsProperty
            && typeof(ImmutableArray).GetMember(nameof(ImmutableArray.CreateRange), MemberTypes.Method, BindingFlags.Public | BindingFlags.Static)
                .OfType<MethodInfo>()
                .FirstOrDefault(x =>
                    x.GetParameters() is { Length: 2 } parameters // CreateRange<TSource,TResult>(ImmutableArray<TSource> items, Func<TSource,TResult> selector)
                    && parameters[0].Name == "items"              // see also https://stackoverflow.com/a/4036187
                    && parameters[1].Name == "selector"
                    && x.GetGenericArguments() is { Length: 2 } typeArguments
                    && typeArguments[0].Name == "TSource"
                    && typeArguments[1].Name == "TResult") is { } createRange)
        {
            var sonarNullableAnnotationType = typeof(NullableAnnotation);
            var createRangeT = createRange.MakeGenericMethod(originalNullableAnnotationType, sonarNullableAnnotationType); // CreateRange<Roslyn.NullableAnnotation, Sonar.NullableAnnotation>
            var delegateType = typeof(Func<,>).MakeGenericType(originalNullableAnnotationType, sonarNullableAnnotationType); // Func<Roslyn.NullableAnnotation, Sonar.NullableAnnotation>

            var originalNullableAnnotationParameter = Parameter(originalNullableAnnotationType, "x");
            var conversion = Lambda(delegateType, Convert(originalNullableAnnotationParameter, sonarNullableAnnotationType), originalNullableAnnotationParameter); // (originalNullableAnnotationType x) => (sonarNullableAnnotationType)x;

            var symbolParameter = Parameter(typeof(INamedTypeSymbol), "symbol");
            return Lambda<Func<INamedTypeSymbol, ImmutableArray<NullableAnnotation>>>(
                Call(createRangeT, Property(symbolParameter, typeArgumentNullableAnnotationsProperty), conversion), // ImmutableArray.CreateRange(symbol.TypeArgumentNullableAnnotations, conversion)
                symbolParameter).Compile();
        }
        // Callers may rely on the fact that TypeArgumentNullableAnnotations() has the same length as TypeArguments.
        return static x => Enumerable.Repeat(NullableAnnotation.None, x.TypeArguments.Length).ToImmutableArray();
    }
}
