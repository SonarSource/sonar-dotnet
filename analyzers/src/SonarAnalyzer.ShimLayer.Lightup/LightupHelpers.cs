// Copyright (c) Tunnel Vision Laboratories, LLC. All Rights Reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

#nullable disable

using System.Linq.Expressions;
using System.Reflection;

namespace StyleCop.Analyzers.Lightup;

public static class LightupHelpers
{
    internal static Func<TProperty> CreateStaticPropertyAccessor<TProperty>(Type type, string propertyName)
    {
        static TProperty FallbackAccessor()
        {
            return default;
        }

        if (type == null)
        {
            return FallbackAccessor;
        }

        var property = type.GetTypeInfo().GetDeclaredProperty(propertyName);
        if (property == null)
        {
            return FallbackAccessor;
        }

        if (!typeof(TProperty).GetTypeInfo().IsAssignableFrom(property.PropertyType.GetTypeInfo()))
        {
            throw new InvalidOperationException();
        }

        Expression<Func<TProperty>> expression =
            Expression.Lambda<Func<TProperty>>(
                Expression.Call(null, property.GetMethod));
        return expression.Compile();
    }
}
