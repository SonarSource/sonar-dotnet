// Copyright (c) Tunnel Vision Laboratories, LLC. All Rights Reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace StyleCop.Analyzers.Lightup
{
    public static class ISymbolNullableExtensions
    {
        private static readonly Func<ITypeSymbol, NullableAnnotation> TypeSymbolAccessor = CreateSymbolNullableAnnotationAccessor<ITypeSymbol>();
        private static readonly Func<IArrayTypeSymbol, NullableAnnotation> ArrayTypeSymbolElementAccessor = CreateSymbolNullableAnnotationAccessor<IArrayTypeSymbol>("ElementNullableAnnotation");
        private static readonly Func<IParameterSymbol, NullableAnnotation> ParameterSymbolAccessor = CreateSymbolNullableAnnotationAccessor<IParameterSymbol>();
        private static readonly Func<ILocalSymbol, NullableAnnotation> LocalSymbolAccessor = CreateSymbolNullableAnnotationAccessor<ILocalSymbol>();
        private static readonly Func<IPropertySymbol, NullableAnnotation> PropertySymbolAccessor = CreateSymbolNullableAnnotationAccessor<IPropertySymbol>();
        private static readonly Func<IFieldSymbol, NullableAnnotation> FieldSymbolAccessor = CreateSymbolNullableAnnotationAccessor<IFieldSymbol>();
        private static readonly Func<IEventSymbol, NullableAnnotation> EventSymbolAccessor = CreateSymbolNullableAnnotationAccessor<IEventSymbol>();
        private static readonly Func<ITypeParameterSymbol, NullableAnnotation> TypeParameterSymbolReferenceTypeConstraintAccessor =
            CreateSymbolNullableAnnotationAccessor<ITypeParameterSymbol>("ReferenceTypeConstraintNullableAnnotation"); // ConstraintNullableAnnotations is not yet supported
        private static readonly Func<IMethodSymbol, NullableAnnotation> MethodSymbolReceiverAccessor =
            CreateSymbolNullableAnnotationAccessor<IMethodSymbol>("ReceiverNullableAnnotation");
        private static readonly Func<IMethodSymbol, NullableAnnotation> MethodSymbolReturnAccessor =
            CreateSymbolNullableAnnotationAccessor<IMethodSymbol>("ReturnNullableAnnotation"); // TypeArgumentNullableAnnotations is not yet supported

        public static NullableAnnotation NullableAnnotation(this ITypeSymbol type) => TypeSymbolAccessor(type);
        public static NullableAnnotation NullableAnnotation(this IParameterSymbol parameter) => ParameterSymbolAccessor(parameter);
        public static NullableAnnotation NullableAnnotation(this ILocalSymbol local) => LocalSymbolAccessor(local);
        public static NullableAnnotation NullableAnnotation(this IPropertySymbol property) => PropertySymbolAccessor(property);
        public static NullableAnnotation NullableAnnotation(this IFieldSymbol field) => FieldSymbolAccessor(field);
        public static NullableAnnotation NullableAnnotation(this IEventSymbol eventSymbol) => EventSymbolAccessor(eventSymbol);
        public static NullableAnnotation ElementNullableAnnotation(this IArrayTypeSymbol arrayType) => ArrayTypeSymbolElementAccessor(arrayType);
        public static NullableAnnotation ReferenceTypeConstraintNullableAnnotation(this ITypeParameterSymbol eventSymbol) => TypeParameterSymbolReferenceTypeConstraintAccessor(eventSymbol);
        public static NullableAnnotation ReceiverNullableAnnotation(this IMethodSymbol method) => MethodSymbolReceiverAccessor(method);
        public static NullableAnnotation ReturnNullableAnnotation(this IMethodSymbol method) => MethodSymbolReturnAccessor(method);

        private static Func<T, NullableAnnotation> CreateSymbolNullableAnnotationAccessor<T>(string propertyName = nameof(NullableAnnotation)) where T : ISymbol
            => LightupHelpers.CreateSyntaxPropertyAccessor<T, NullableAnnotation>(typeof(T), propertyName);
    }
}
