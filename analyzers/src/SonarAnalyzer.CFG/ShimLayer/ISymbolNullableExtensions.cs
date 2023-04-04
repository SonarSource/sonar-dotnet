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

        /// <summary>
        /// Nullable annotation associated with the type, or < see cref="NullableAnnotation.None" /> if there are none.
        /// </summary>
        public static NullableAnnotation NullableAnnotation(this ITypeSymbol type) => TypeSymbolAccessor(type);

        /// <summary>
        /// Gets the top-level nullability of the parameter.
        /// </summary>
        public static NullableAnnotation NullableAnnotation(this IParameterSymbol parameter) => ParameterSymbolAccessor(parameter);

        /// <summary>
        /// Gets the top-level nullability of this local variable.
        /// </summary>
        public static NullableAnnotation NullableAnnotation(this ILocalSymbol local) => LocalSymbolAccessor(local);

        /// <summary>
        /// Gets the top-level nullability of this property.
        /// </summary>
        public static NullableAnnotation NullableAnnotation(this IPropertySymbol property) => PropertySymbolAccessor(property);

        /// <summary>
        /// Gets the top-level nullability of this field.
        /// </summary>
        public static NullableAnnotation NullableAnnotation(this IFieldSymbol field) => FieldSymbolAccessor(field);

        /// <summary>
        /// The top-level nullability of the event.
        /// </summary>
        public static NullableAnnotation NullableAnnotation(this IEventSymbol eventSymbol) => EventSymbolAccessor(eventSymbol);

        /// <summary>
        /// Gets the top-level nullability of the elements stored in the array.
        /// </summary>
        public static NullableAnnotation ElementNullableAnnotation(this IArrayTypeSymbol arrayType) => ArrayTypeSymbolElementAccessor(arrayType);

        /// <summary>
        /// If <see cref="ITypeParameterSymbol.HasReferenceTypeConstraint"/> is <see langword="true" />, returns the top-level nullability of the
        /// class constraint that was specified for the type parameter. If there was no class constraint, this returns <see cref="NullableAnnotation.None"/>.
        /// </summary>
        public static NullableAnnotation ReferenceTypeConstraintNullableAnnotation(this ITypeParameterSymbol eventSymbol) => TypeParameterSymbolReferenceTypeConstraintAccessor(eventSymbol);

        /// <summary>
        /// If this method can be applied to an object, returns the top-level nullability of the object it is applied to.
        /// </summary>
        public static NullableAnnotation ReceiverNullableAnnotation(this IMethodSymbol method) => MethodSymbolReceiverAccessor(method);

        /// <summary>
        /// Gets the top-level nullability of the return type of the method.
        /// </summary>
        public static NullableAnnotation ReturnNullableAnnotation(this IMethodSymbol method) => MethodSymbolReturnAccessor(method);

        private static Func<T, NullableAnnotation> CreateSymbolNullableAnnotationAccessor<T>(string propertyName = nameof(NullableAnnotation)) where T : ISymbol
            => LightupHelpers.CreateSyntaxPropertyAccessor<T, NullableAnnotation>(typeof(T), propertyName);
    }
}
