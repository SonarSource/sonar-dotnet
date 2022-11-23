// Copyright (c) Tunnel Vision Laboratories, LLC. All Rights Reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace StyleCop.Analyzers.Lightup
{
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
            var property = typeof(TypeInfo).GetProperty(propertyName);
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
}
