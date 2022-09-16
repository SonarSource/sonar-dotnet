// Copyright (c) Tunnel Vision Laboratories, LLC. All Rights Reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace StyleCop.Analyzers.Lightup
{
    using System;
    using Microsoft.CodeAnalysis;
    using static System.Linq.Expressions.Expression;

    public static class TypeInfoExtensions
    {
        private static readonly Func<TypeInfo, NullabilityInfo> ConvertedNullabilityAccessor = CreateNullabilityAccessor(nameof(ConvertedNullability));
        private static readonly Func<TypeInfo, NullabilityInfo> NullabilityAccessor = CreateNullabilityAccessor(nameof(Nullability));

        public static NullabilityInfo ConvertedNullability(this TypeInfo typeInfo) =>
            ConvertedNullabilityAccessor(typeInfo);

        public static NullabilityInfo Nullability(this TypeInfo typeInfo) =>
            NullabilityAccessor(typeInfo);

        private static Func<TypeInfo, NullabilityInfo> CreateNullabilityAccessor(string propertyName)
        {
            var typeInfoType = typeof(TypeInfo);
            var property = typeInfoType.GetProperty(propertyName);
            if (property == null)
            {
                return static _ => default;
            }

            Type nullableAnnotationType = typeof(NullableAnnotation), nullableFlowStateType = typeof(NullableFlowState);
            var typeInfoParameter = Parameter(typeof(TypeInfo), "typeInfo");
            var intermediateResult = Variable(property.PropertyType); // local variable which holds the Roslyn NullabilityInfo

            // intermediateResult = typeInfo.{propertyName};
            // return new Lightup.NullabilityInfo((Lightup.NullableAnnotation)intermediateResult.Annotation, (Lightup.NullableFlowState)intermediateResult.FlowState);
            var body = Block(variables: new[] { intermediateResult },
                Assign(intermediateResult, Property(typeInfoParameter, propertyName)),
                New(typeof(NullabilityInfo).GetConstructor(new[] { nullableAnnotationType, nullableFlowStateType }),
                    Convert(Property(intermediateResult, "Annotation"), nullableAnnotationType), // enum to enum conversion
                    Convert(Property(intermediateResult, "FlowState"), nullableFlowStateType)));
            var expression = Lambda<Func<TypeInfo, NullabilityInfo>>(body, typeInfoParameter);
            return expression.Compile();
        }
    }

    public readonly record struct NullabilityInfo
    {
        public NullabilityInfo(NullableAnnotation annotation, NullableFlowState flowState)
        {
            Annotation = annotation;
            FlowState = flowState;
        }

        /// <summary>
        /// The nullable annotation of the expression represented by the syntax node. This represents
        /// the nullability of expressions that can be assigned to this expression, if this expression
        /// can be used as an lvalue.
        /// </summary>
        public NullableAnnotation Annotation { get; }

        /// <summary>
        /// The nullable flow state of the expression represented by the syntax node. This represents
        /// the compiler's understanding of whether this expression can currently contain null, if
        /// this expression can be used as an rvalue.
        /// </summary>
        public NullableFlowState FlowState { get; }
    }

    public enum NullableAnnotation
    {
        /// <summary>
        /// The expression has not been analyzed, or the syntax is not an expression (such as a statement).
        /// </summary>
        None = 0,

        /// <summary>
        /// The expression is not annotated (does not have a ?).
        /// </summary>
        NotAnnotated = 1,

        /// <summary>
        /// The expression is annotated (does have a ?).
        /// </summary>
        Annotated = 2,
    }

    public enum NullableFlowState
    {
        /// <summary>
        /// Syntax is not an expression, or was not analyzed.
        /// </summary>
        None = 0,

        /// <summary>
        /// Expression is not null.
        /// </summary>
        NotNull = 1,

        /// <summary>
        /// Expression may be null.
        /// </summary>
        MaybeNull = 2,
    }
}
