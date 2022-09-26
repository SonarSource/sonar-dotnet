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

        public static bool TryGetAttributeValue<T>(this AttributeData attribute, string namedArgument, out T value)
        {
            value = default;
            return attribute.NamedArguments.IndexOf(x => x.Key.Equals(namedArgument, StringComparison.OrdinalIgnoreCase)) is var index and >= 0
                && attribute.NamedArguments[index].Value is var constant
                && ConvertConstant(constant, out value);
        }

        public static bool TryGetAttributeValue<T>(this AttributeData attribute, Func<IMethodSymbol, IParameterSymbol> constructorParameter, out T value)
        {
            value = default;
            return constructorParameter(attribute.AttributeConstructor) is { } parameter
                && attribute.AttributeConstructor.Parameters.IndexOf(parameter) is var index and >= 0
                && attribute.ConstructorArguments[index] is var constant
                && ConvertConstant(constant, out value);
        }

        private static bool ConvertConstant<T>(TypedConstant constant, out T value)
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
                catch
                {
                    return false;
                }
                return true;
            }
            return false;
        }
    }
}
