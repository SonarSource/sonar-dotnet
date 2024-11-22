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

using static System.Linq.Expressions.Expression;

namespace StyleCop.Analyzers.Lightup;

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
        if (property is null)
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
