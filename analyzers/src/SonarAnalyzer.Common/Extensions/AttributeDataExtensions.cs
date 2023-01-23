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

        /// <summary>
        /// Returns the <see cref="AttributeUsageAttribute.Inherited"/> setting for the attribute associated with <paramref name="attribute"/>.
        /// The returned value is in line with the runtime behavior of <see cref="System.Reflection.MemberInfo.GetCustomAttributes(bool)"/>.
        /// </summary>
        /// <param name="attribute">The <see cref="AttributeData"/> of an <see cref="Attribute"/>.</param>
        /// <returns>
        /// The <see cref="AttributeUsageAttribute.Inherited"/> value of the <see cref="AttributeUsageAttribute"/> applied to the <paramref name="attribute"/>.
        /// Returns <see langword="true"/> if no <see cref="AttributeUsageAttribute"/> is applied to the <see cref="Attribute"/> associated with <paramref name="attribute"/>
        /// as this is the default behavior of <see cref="System.Reflection.MemberInfo.GetCustomAttributes(bool)"/>.
        /// </returns>
        public static bool AttributeUsageInherited(this AttributeData attribute) =>
            attribute.AttributeClass.GetAttributes()
                .Where(x => x.AttributeClass is { Name: nameof(AttributeUsageAttribute), ContainingNamespace.Name: "System" })
                .SelectMany(x => x.NamedArguments.Where(x => x.Key == nameof(AttributeUsageAttribute.Inherited)))
                .Select(x => x.Value)
                .Where(x => x is { Kind: TypedConstantKind.Primitive, Type.SpecialType: SpecialType.System_Boolean })
                .Select(x => (bool?)x.Value)
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
}
