/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2014-2025 SonarSource Sàrl
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

namespace SonarAnalyzer.Core.Syntax.Extensions;

public static class AttributeDataExtensions
{
    private static readonly ImmutableArray<KnownType> RouteTemplateProviders = ImmutableArray.Create(
        KnownType.Microsoft_AspNetCore_Mvc_Routing_IRouteTemplateProvider,
        KnownType.System_Web_Mvc_Routing_IRouteInfoProvider);

    public static bool HasName(this AttributeData attribute, string name) =>
        attribute is { AttributeClass.Name: { } attributeClassName } && attributeClassName == name;

    public static bool HasAnyName(this AttributeData attribute, params string[] names) =>
        names.Any(x => attribute.HasName(x));

    public static string GetAttributeRouteTemplate(this AttributeData attribute) =>
        attribute.AttributeClass.DerivesOrImplementsAny(RouteTemplateProviders)
        && attribute.TryGetAttributeValue<string>("template", out var template)
            ? template
            : null;

    public static bool TryGetAttributeValue<T>(this AttributeData attribute, string valueName, out T value)
    {
        // named arguments take precedence over constructor arguments of the same name. For [Attr(valueName: false, valueName = true)] "true" is returned.
        if (attribute.NamedArguments.IndexOf(x => x.Key.Equals(valueName, StringComparison.OrdinalIgnoreCase)) is var namedAgumentIndex and >= 0)
        {
            return TryConvertConstant(attribute.NamedArguments[namedAgumentIndex].Value, out value);
        }
        else if (attribute.AttributeConstructor.Parameters.IndexOf(x => x.Name.Equals(valueName, StringComparison.OrdinalIgnoreCase)) is var constructorParameterIndex and >= 0)
        {
            return TryConvertConstant(attribute.ConstructorArguments[constructorParameterIndex], out value);
        }
        else
        {
            value = default;
            return false;
        }
    }

    public static bool HasAttributeUsageInherited(this AttributeData attribute) =>
        attribute.AttributeClass.GetAttributes()
            .Where(x => x.AttributeClass.Is(KnownType.System_AttributeUsageAttribute))
            .SelectMany(x => x.NamedArguments.Where(x => x.Key == nameof(AttributeUsageAttribute.Inherited)))
            .Where(x => x.Value is { Kind: TypedConstantKind.Primitive, Type.SpecialType: SpecialType.System_Boolean })
            .Select(x => (bool?)x.Value.Value)
            .FirstOrDefault() ?? true; // Default value of Inherited is true

    private static bool TryConvertConstant<T>(TypedConstant constant, out T value)
    {
        value = default;
        if (constant.IsNull)
        {
            return true;
        }
        else if (constant.Value is T result)
        {
            value = result;
            return true;
        }
        else if (constant.Value is IConvertible)
        {
            try
            {
                value = (T)Convert.ChangeType(constant.Value, typeof(T));
                return true;
            }
            catch
            {
                return false;
            }
        }
        else
        {
            return false;
        }
    }
}
