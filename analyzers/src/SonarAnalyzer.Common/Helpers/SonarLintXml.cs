/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2024 SonarSource SA
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

using System.Xml.Serialization;

namespace SonarAnalyzer.Helpers;

/// <summary>
/// Data class to represent the SonarLint.xml for our analyzers.
/// </summary>
/// <remarks>
/// This class should not be used in this codebase. To get SonarLint.xml properties, use <see cref="SonarLintXmlReader"/>.
/// </remarks>
[XmlRoot(ElementName = "AnalysisInput")]
public class SonarLintXml
{
    public static readonly SonarLintXml Empty = new();

    [XmlArray("Settings")]
    [XmlArrayItem("Setting")]
    public List<SonarLintXmlKeyValuePair> Settings { get; set; }

    [XmlArray("Rules")]
    [XmlArrayItem("Rule")]
    public List<SonarLintXmlRule> Rules { get; set; }
}

public class SonarLintXmlRule
{
    [XmlElement("Key")]
    public string Key { get; set; }

    [XmlArray("Parameters")]
    [XmlArrayItem("Parameter")]
    public List<SonarLintXmlKeyValuePair> Parameters { get; set; }
}

public class SonarLintXmlKeyValuePair
{
    public string Key { get; set; }
    public string Value { get; set; }
}
