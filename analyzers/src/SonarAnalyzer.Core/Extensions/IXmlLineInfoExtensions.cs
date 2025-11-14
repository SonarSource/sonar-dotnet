/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2014-2025 SonarSource Sàrl
 * mailto:info AT sonarsource DOT com
 * This program is free software; you can redistribute it and/or
 * modify it under the terms of the Sonar Source-Available License Version 1, as published by SonarSource Sàrl.
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

namespace SonarAnalyzer.Core.Extensions;

internal static class IXmlLineInfoExtensions
{
    public static Location CreateLocation(this IXmlLineInfo startPos, string path, XName name, XElement closestElement)
    {
        // IXmlLineInfo is 1-based, whereas Roslyn is zero-based
        if (startPos.HasLineInfo())
        {
            var prefix = closestElement.GetPrefixOfNamespace(name.Namespace);
            var length = name.LocalName.Length;
            if (prefix is not null)
            {
                length += prefix.Length + 1; // prefix:LocalName - +1 for ':'
            }
            var start = new LinePosition(startPos.LineNumber - 1, startPos.LinePosition - 1);
            var end = new LinePosition(startPos.LineNumber - 1, startPos.LinePosition - 1 + length);
            // We cannot properly compute the TextSpan as we only have the line information.
            // It should not break the analysis nor the reporting as we still have the LinePositionSpan.
            // Manual test has shown that the Sarif contains the correct information with only the LinePositionSpan.
            return Location.Create(path, new TextSpan(start.Line, length), new LinePositionSpan(start, end));
        }
        else
        {
            return null;
        }
    }
}
