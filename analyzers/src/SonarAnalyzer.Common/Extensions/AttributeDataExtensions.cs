/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2022 SonarSource SA
 * mailto: contact AT sonarsource DOT com
 *
 * This program is free software; you can redistribute it and/or
 * modify it under the terms of the GNU Lesser General Public
 * License as published by the Free Software Foundation; either
 * version 3 of the License, or (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
 * Lesser General Public License for more details.
 *
 * You should have received a copy of the GNU Lesser General Public License
 * along with this program; if not, write to the Free Software Foundation,
 * Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02110-1301, USA.
 */

using System;
using System.Linq;
using Microsoft.CodeAnalysis;
using SonarAnalyzer.Helpers;

namespace SonarAnalyzer.Extensions
{
    public static class AttributeDataExtensions
    {
        public static bool HasName(this AttributeData attribute, string name) =>
            attribute is { AttributeClass.Name: { } attributeClassName } && attributeClassName == name;

        public static bool HasAnyName(this AttributeData attribute, params string[] names) =>
            names.Any(x => attribute.HasName(x));

        public static bool TryGetAttributeValue<T>(this AttributeData attribute, string valueName, out T value)
        {
            value = default;
            // named arguments take precedence over constructor arguments of the same name. For [Attr(valueName: false, valueName = true)] "true" is returned.
            if (attribute.NamedArguments.IndexOf(x => x.Key.Equals(valueName, StringComparison.OrdinalIgnoreCase)) is var namedAgumentIndex and >= 0)
            {
                return ConvertConstant(attribute.NamedArguments[namedAgumentIndex].Value, out value);
            }
            if (attribute.AttributeConstructor.Parameters.IndexOf(x => x.Name.Equals(valueName, StringComparison.OrdinalIgnoreCase)) is var constructorParameterIndex and >= 0)
            {
                return ConvertConstant(attribute.ConstructorArguments[constructorParameterIndex], out value);
            }
            return false;

            static bool ConvertConstant(TypedConstant constant, out T value)
            {
                value = default;
                if (constant.IsNull)
                {
                    return true;
                }
                if (constant.Value is T result)
                {
                    value = result;
                    return true;
                }
                if (constant.Value is IConvertible)
                {
                    try
                    {
                        value = (T)Convert.ChangeType(constant.Value, typeof(T));
                    }
                    catch (Exception ex) when (ex is FormatException or OverflowException)
                    {
                        return false;
                    }
                    return true;
                }
                return false;
            }
        }
    }
}
