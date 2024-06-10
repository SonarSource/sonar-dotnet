// Copyright (c) Tunnel Vision Laboratories, LLC. All Rights Reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.


namespace StyleCop.Analyzers.Lightup
{
    using System.Reflection;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using static System.Linq.Expressions.Expression;

    public static class INamedTypeSymbolExtensions
    {
        private static readonly Func<INamedTypeSymbol, INamedTypeSymbol> TupleUnderlyingTypeAccessor;
        private static readonly Func<INamedTypeSymbol, ImmutableArray<IFieldSymbol>> TupleElementsAccessor;
        private static readonly Func<INamedTypeSymbol, bool> IsSerializableAccessor;
        private static readonly Func<INamedTypeSymbol, ImmutableArray<NullableAnnotation>> TypeArgumentNullableAnnotationsAccessor;

        static INamedTypeSymbolExtensions()
        {
            TupleUnderlyingTypeAccessor = LightupHelpers.CreateSyntaxPropertyAccessor<INamedTypeSymbol, INamedTypeSymbol>(typeof(INamedTypeSymbol), nameof(TupleUnderlyingType));
            TupleElementsAccessor = LightupHelpers.CreateSyntaxPropertyAccessor<INamedTypeSymbol, ImmutableArray<IFieldSymbol>>(typeof(INamedTypeSymbol), nameof(TupleElements));
            IsSerializableAccessor = LightupHelpers.CreateSyntaxPropertyAccessor<INamedTypeSymbol, bool>(typeof(INamedTypeSymbol), nameof(IsSerializable));
            TypeArgumentNullableAnnotationsAccessor = CreateTypeArgumentNullableAnnotationsAccessor();
        }

        public static INamedTypeSymbol TupleUnderlyingType(this INamedTypeSymbol symbol) => TupleUnderlyingTypeAccessor(symbol);

        public static ImmutableArray<IFieldSymbol> TupleElements(this INamedTypeSymbol symbol) => TupleElementsAccessor(symbol);

        public static bool IsSerializable(this INamedTypeSymbol symbol) => IsSerializableAccessor(symbol);

        public static ImmutableArray<NullableAnnotation> TypeArgumentNullableAnnotations(this INamedTypeSymbol symbol) => TypeArgumentNullableAnnotationsAccessor(symbol);

        private static Func<INamedTypeSymbol, ImmutableArray<NullableAnnotation>> CreateTypeArgumentNullableAnnotationsAccessor()
        {
            // INamedTypeSymbol.TypeArgumentNullableAnnotations is ImmutableArray<Roslyn.NullableAnnotation>
            // The generated code is symbol => ImmutableArray.CreateRange(symbol.TypeArgumentNullableAnnotations, x => (Sonar.NullableAnnotationType)x);
            var fallback = static (INamedTypeSymbol x) => Enumerable.Repeat(NullableAnnotation.None, x.TypeArguments.Length).ToImmutableArray();
            if (OriginalNullableAnnotationType() is not { } originalNullableAnnotationType)
            {
                // Callers may rely on the fact that symbol.TypeArgumentNullableAnnotations is supposed to have the same length as symbol.TypeArguments
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
}
