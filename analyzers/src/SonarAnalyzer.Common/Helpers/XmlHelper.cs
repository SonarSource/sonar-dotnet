/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2021 SonarSource SA
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
using System.Xml;
using System.Xml.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace SonarAnalyzer.Helpers
{
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

        public static Location CreateLocation(this XAttribute attribute, string path)
        {
            // IXmlLineInfo is 1-based, whereas Roslyn is zero-based
            var startPos = (IXmlLineInfo)attribute;
            if (startPos.HasLineInfo())
            {
                // LoadOptions.PreserveWhitespace doesn't preserve whitespace inside nodes and attributes => there's no easy way to find full length of a XAttribute.
                var length = attribute.Name.ToString().Length;
                var start = new LinePosition(startPos.LineNumber - 1, startPos.LinePosition - 1);
                var end = new LinePosition(startPos.LineNumber - 1, startPos.LinePosition - 1 + length);
                return Location.Create(path, new TextSpan(start.Line, length), new LinePositionSpan(start, end));
            }
            return null;
        }
    }
}
