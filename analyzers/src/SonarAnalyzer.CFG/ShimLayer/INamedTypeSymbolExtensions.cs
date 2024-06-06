// Copyright (c) Tunnel Vision Laboratories, LLC. All Rights Reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.


namespace StyleCop.Analyzers.Lightup
{
    using System.Reflection;
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
            var originalNullableAnnotationType = Type.GetType("Microsoft.CodeAnalysis.NullableAnnotation, Microsoft.CodeAnalysis, Version=4.9.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35");
            var immutableArrayType = typeof(ImmutableArray);
            var immutableArrayTpe = typeof(ImmutableArray<NullableAnnotation>);
            var builderType = typeof(ImmutableArray<NullableAnnotation>.Builder);
            var originalType = typeof(ImmutableArray<>).MakeGenericType(originalNullableAnnotationType);

            var builderCreateT = immutableArrayType.GetMethod(nameof(ImmutableArray.CreateBuilder), BindingFlags.Static | BindingFlags.Public, null, [typeof(int)], null); // Ctor with capacity
            var builderCreate = builderCreateT.MakeGenericMethod(typeof(NullableAnnotation));
            var builderAdd = builderType.GetMethod(nameof(ImmutableArray<int>.Builder.Add));
            var builderToImmutable = builderType.GetMethod(nameof(ImmutableArray<int>.Builder.ToImmutable));

            var symbol = Parameter(typeof(INamedTypeSymbol), "symbol");
            var original = Parameter(originalType, "original");
            var builder = Parameter(builderType, "builder");
            var i = Parameter(typeof(int), "i");

            var exit = Label(immutableArrayTpe);
            var body = Block([builder, original, i],
                Assign(original, Property(symbol, nameof(TypeArgumentNullableAnnotations))), // original = symbol.TypeArgumentNullableAnnotations;
                Assign(builder, Call(builderCreate, Property(original, nameof(ImmutableArray<int>.Length)))), // builder = ImmutableArray.CreateBuilder<NullableAnnotation>(original.Length);
                Assign(i, Constant(0)), // i = 0;
                Loop(
                    IfThenElse(LessThan(i, Property(original, "Length")), // if (i < original.Length)
                    ifTrue: Block(
                        Call(builder, builderAdd, Convert(Property(original, "Item", i), typeof(NullableAnnotation))), // builder.Add((NullableAnnotation)original[i]);
                        AddAssign(i, Constant(1))), // i += 1;
                    ifFalse: Return(exit, Call(builder, builderToImmutable))), // return builder.ToImmutable();
                    exit));
            return Lambda<Func<INamedTypeSymbol, ImmutableArray<NullableAnnotation>>>(body, symbol).Compile();
        }
    }
}
