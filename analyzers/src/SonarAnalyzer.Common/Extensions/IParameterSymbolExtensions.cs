/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2023 SonarSource SA
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

namespace SonarAnalyzer.Extensions;

public static class IParameterSymbolExtensions
{
    public static bool HasNotNullAttribute(this IParameterSymbol parameter) =>
        parameter.GetAttributes() is { Length: > 0 } attributes && attributes.Any(IsNotNullAttribute);

    // https://docs.microsoft.com/dotnet/api/microsoft.validatednotnullattribute
    // https://docs.microsoft.com/dotnet/csharp/language-reference/attributes/nullable-analysis#postconditions-maybenull-and-notnull
    // https://www.jetbrains.com/help/resharper/Reference__Code_Annotation_Attributes.html#NotNullAttribute
    private static bool IsNotNullAttribute(AttributeData attribute) =>
        attribute.HasAnyName("ValidatedNotNullAttribute", "NotNullAttribute");
}
