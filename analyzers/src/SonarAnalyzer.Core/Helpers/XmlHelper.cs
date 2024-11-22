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

using System.Xml;
using System.Xml.Linq;
using Microsoft.CodeAnalysis.Text;

namespace SonarAnalyzer.Helpers;

public static class XmlHelper
{
    public static XDocument ParseXDocument(string text)
    {
        try
        {
            return XDocument.Parse(text, LoadOptions.SetLineInfo);
        }
        catch
        {
            return null;
        }
    }

    public static XAttribute GetAttributeIfBoolValueIs(this XElement element, string attributeName, bool value) =>
        element.Attribute(attributeName) is { } attribute
            && attribute.Value.Equals(value.ToString(), StringComparison.OrdinalIgnoreCase)
            ? attribute
            : null;

    public static Location CreateLocation(this XAttribute attribute, string path) =>
        CreateLocation(attribute, path, attribute.Name);

    public static Location CreateLocation(this XElement element, string path) =>
        CreateLocation(element, path, element.Name);

    private static Location CreateLocation(IXmlLineInfo startPos, string path, XName name)
    {
        // IXmlLineInfo is 1-based, whereas Roslyn is zero-based
        if (startPos.HasLineInfo())
        {
            // LoadOptions.PreserveWhitespace doesn't preserve whitespace inside nodes and attributes => there's no easy way to find full length of a XAttribute.
            var length = name.ToString().Length;
            var start = new LinePosition(startPos.LineNumber - 1, startPos.LinePosition - 1);
            var end = new LinePosition(startPos.LineNumber - 1, startPos.LinePosition - 1 + length);
            return Location.Create(path, new TextSpan(start.Line, length), new LinePositionSpan(start, end));
        }
        else
        {
            return null;
        }
    }
}
