﻿/*
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

using System.IO;
using System.Xml.Serialization;
using Microsoft.CodeAnalysis.Text;

namespace SonarAnalyzer.Helpers;

public class SonarLintXmlReader
{
    public static readonly SonarLintXmlReader Empty = new(null, LanguageNames.CSharp);

    private readonly SonarLintXml sonarLintXml;
    private readonly string propertyLanguage;

    private SonarLintXmlSettingsReader settings;
    public SonarLintXmlSettingsReader Settings => settings ??= new(sonarLintXml, propertyLanguage);

    public SonarLintXmlReader(SourceText originalXml, string language)
    {
        sonarLintXml = originalXml == null ? SonarLintXml.Empty : ParseContent(originalXml);
        propertyLanguage = language;
    }

    private static SonarLintXml ParseContent(SourceText sonarProjectConfig)
    {
        try
        {
            var serializer = new XmlSerializer(typeof(SonarLintXml));
            using var sr = new StringReader(sonarProjectConfig.ToString());
            return (SonarLintXml)serializer.Deserialize(sr);
        }
        catch (Exception)
        {
            return SonarLintXml.Empty;
        }
    }
}

public class SonarLintXmlSettingsReader
{
    private readonly SonarLintXml sonarLintXml;
    private readonly string propertyLanguage;

    private bool? ignoreHeaderComments;
    public bool? IgnoreHeaderComments => ignoreHeaderComments ??= ReadBoolean(ReadProperty($"sonar.{propertyLanguage}.ignoreHeaderComments"));

    private bool? analyzeGeneratedCode;
    public bool AnalyzeGeneratedCode => analyzeGeneratedCode ??= ReadBoolean(ReadProperty($"sonar.{propertyLanguage}.analyzeGeneratedCode"));

    private bool? ignoreIssues;
    public bool IgnoreIssues => ignoreIssues ??= ReadBoolean(ReadProperty($"sonar.{propertyLanguage}.roslyn.ignoreIssues"));

    private string suffixes;
    public string Suffixes => suffixes ??= ReadString(ReadProperty($"sonar.{propertyLanguage}.file.suffixes"));

    private string[] exclusions;
    public string[] Exclusions => exclusions ??= ReadCommaSeparatedArray(ReadProperty("sonar.exclusions"));

    private string[] inclusions;
    public string[] Inclusions => inclusions ??= ReadCommaSeparatedArray(ReadProperty("sonar.inclusions"));

    private string[] globalExclusions;
    public string[] GlobalExclusions => globalExclusions ??= ReadCommaSeparatedArray(ReadProperty("sonar.global.exclusions"));

    private string[] testExclusions;
    public string[] TestExclusions => testExclusions ??= ReadCommaSeparatedArray(ReadProperty("sonar.test.exclusions"));

    private string[] testInclusions;
    public string[] TestInclusions => testInclusions ??= ReadCommaSeparatedArray(ReadProperty("sonar.test.inclusions"));

    public SonarLintXmlSettingsReader(SonarLintXml originalXml, string language)
    {
        sonarLintXml = originalXml;
        propertyLanguage = language == LanguageNames.CSharp ? "cs" : "vbnet";
    }

    private string ReadProperty(string property) =>
        sonarLintXml is { Settings: { } settings }
        ? settings.Where(x => x.Key.Equals(property)).Select(x => x.Value).FirstOrDefault()
        : string.Empty;

    private string ReadString(string str) =>
        string.IsNullOrEmpty(str) ? string.Empty : str;

    private static string[] ReadCommaSeparatedArray(string str) =>
        string.IsNullOrEmpty(str) ? Array.Empty<string>() : str.Split(',');

    private static bool ReadBoolean(string str, bool defaultValue = false) =>
        bool.TryParse(str, out var propertyValue) ? propertyValue : defaultValue;
}
