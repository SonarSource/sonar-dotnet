/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2014-2025 SonarSource SA
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

using System.Xml.Linq;

namespace SonarAnalyzer.Core.Extensions;

public static class XElementExtensions
{
    public static XAttribute GetAttributeIfBoolValueIs(this XElement element, string attributeName, bool value) =>
        element.Attribute(attributeName) is { } attribute
            && attribute.Value.Equals(value.ToString(), StringComparison.OrdinalIgnoreCase)
            ? attribute
            : null;

    public static Location CreateLocation(this XElement element, string path) =>
        element.CreateLocation(path, element.Name);
}
