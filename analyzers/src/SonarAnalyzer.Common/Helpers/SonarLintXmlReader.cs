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

using System.IO;
using System.Xml.Serialization;
using Microsoft.CodeAnalysis.Text;

namespace SonarAnalyzer.Helpers;

public class SonarLintXmlReader
{
    private readonly SonarLintXml sonarLintXml;
    private readonly Lazy<SonarLintXmlSettingsReader> settings;

    public static readonly SonarLintXmlReader Empty = new(null, LanguageNames.CSharp);

    public SonarLintXmlSettingsReader Settings => settings.Value;

    public SonarLintXmlReader(SourceText originalXml, string language)
    {
        sonarLintXml = originalXml == null ? SonarLintXml.Empty : ParseContent(originalXml);
        settings = new Lazy<SonarLintXmlSettingsReader>(() => new(sonarLintXml, language));
    }

    private static SonarLintXml ParseContent(SourceText sonarProjectConfig)
    {
        try
        {
            var serializer = new XmlSerializer(typeof(SonarLintXml));
            using var sr = new StringReader(sonarProjectConfig.ToString());
            return (SonarLintXml)serializer.Deserialize(sr);
        }
        catch
        {
            return SonarLintXml.Empty;
        }
    }
}

public class SonarLintXmlSettingsReader
{
    private readonly Lazy<bool> ignoreHeaderComments;
    private readonly Lazy<bool> analyzeGeneratedCode;
    private readonly Lazy<bool> ignoreIssues;
    private readonly Lazy<string> suffixes;
    private readonly Lazy<string> relativeRootFromSonarLintXml;
    private readonly Lazy<string[]> exclusions;
    private readonly Lazy<string[]> inclusions;
    private readonly Lazy<string[]> globalExclusions;
    private readonly Lazy<string[]> testExclusions;
    private readonly Lazy<string[]> testInclusions;
    public bool IgnoreHeaderComments => ignoreHeaderComments.Value;
    public bool AnalyzeGeneratedCode => analyzeGeneratedCode.Value;
    public bool IgnoreIssues => ignoreIssues.Value;
    public string Suffixes => suffixes.Value;
    public string RelativeRootFromSonarLintXml => relativeRootFromSonarLintXml.Value;
    public string[] Exclusions => exclusions.Value;
    public string[] Inclusions => inclusions.Value;
    public string[] GlobalExclusions => globalExclusions.Value;
    public string[] TestExclusions => testExclusions.Value;
    public string[] TestInclusions => testInclusions.Value;

    public SonarLintXmlSettingsReader(SonarLintXml sonarLintXml, string language)
    {
        var propertyLanguage = language == LanguageNames.CSharp ? "cs" : "vbnet";
        ignoreHeaderComments = new Lazy<bool>(() => ReadBoolean(ReadProperty($"sonar.{propertyLanguage}.ignoreHeaderComments")));
        analyzeGeneratedCode = new Lazy<bool>(() => ReadBoolean(ReadProperty($"sonar.{propertyLanguage}.analyzeGeneratedCode")));
        ignoreIssues = new Lazy<bool>(() => ReadBoolean(ReadProperty($"sonar.{propertyLanguage}.roslyn.ignoreIssues")));
        suffixes = new Lazy<string>(() => ReadString(ReadProperty($"sonar.{propertyLanguage}.file.suffixes")));
        relativeRootFromSonarLintXml = new Lazy<string>(() => ReadString(ReadProperty("sonar.relativeRootFromSonarLintXml")));
        exclusions = new Lazy<string[]>(() => ReadCommaSeparatedArray(ReadProperty("sonar.exclusions")));
        inclusions = new Lazy<string[]>(() => ReadCommaSeparatedArray(ReadProperty("sonar.inclusions")));
        globalExclusions = new Lazy<string[]>(() => ReadCommaSeparatedArray(ReadProperty("sonar.global.exclusions")));
        testExclusions = new Lazy<string[]>(() => ReadCommaSeparatedArray(ReadProperty("sonar.test.exclusions")));
        testInclusions = new Lazy<string[]>(() => ReadCommaSeparatedArray(ReadProperty("sonar.test.inclusions")));

        string ReadProperty(string property) =>
            sonarLintXml.Settings.Where(x => x.Key.Equals(property)).Select(x => x.Value).FirstOrDefault();
    }

    private static string ReadString(string str) =>
        string.IsNullOrEmpty(str) ? string.Empty : str;

    private static string[] ReadCommaSeparatedArray(string str) =>
        string.IsNullOrEmpty(str) ? Array.Empty<string>() : str.Split(',');

    private static bool ReadBoolean(string str, bool defaultValue = false) =>
        bool.TryParse(str, out var propertyValue) ? propertyValue : defaultValue;
}
