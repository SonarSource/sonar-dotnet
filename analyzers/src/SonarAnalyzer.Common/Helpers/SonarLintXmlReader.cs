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

using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using Microsoft.CodeAnalysis.Text;

namespace SonarAnalyzer.Helpers;

public class SonarLintXmlReader
{
    public static readonly SonarLintXmlReader Empty = new(null);

    private readonly SonarLintXml sonarLintXml;

    public string[] Exclusions { get; }
    public string[] Inclusions { get; }
    public string[] GlobalExclusions { get; }
    public string[] TestExclusions { get; }
    public string[] TestInclusions { get; }
    public string[] GlobalTestExclusions { get; }
    public List<SonarLintXmlRule> ParametrizedRules { get; }
    private bool IgnoreHeaderCommentsCS { get; }
    private bool IgnoreHeaderCommentsVB { get; }
    private bool AnalyzeGeneratedCodeCS { get; }
    private bool AnalyzeGeneratedCodeVB { get; }

    public SonarLintXmlReader(SourceText sonarLintXml)
    {
        this.sonarLintXml = sonarLintXml == null ? SonarLintXml.Empty : ParseContent(sonarLintXml);

        var settings = SettingsToDictionary();
        Exclusions = ReadCommaSeparatedArray(settings.GetValueOrDefault("sonar.exclusions"));
        Inclusions = ReadCommaSeparatedArray(settings.GetValueOrDefault("sonar.inclusions"));
        GlobalExclusions = ReadCommaSeparatedArray(settings.GetValueOrDefault("sonar.global.exclusions"));
        TestExclusions = ReadCommaSeparatedArray(settings.GetValueOrDefault("sonar.test.exclusions"));
        TestInclusions = ReadCommaSeparatedArray(settings.GetValueOrDefault("sonar.test.inclusions"));
        GlobalTestExclusions = ReadCommaSeparatedArray(settings.GetValueOrDefault("sonar.global.test.exclusions"));
        ParametrizedRules = ReadRuleParameters();
        IgnoreHeaderCommentsCS = ReadBoolean(settings.GetValueOrDefault("sonar.cs.ignoreHeaderComments"));
        IgnoreHeaderCommentsVB = ReadBoolean(settings.GetValueOrDefault("sonar.vbnet.ignoreHeaderComments"));
        AnalyzeGeneratedCodeCS = ReadBoolean(settings.GetValueOrDefault("sonar.cs.analyzeGeneratedCode"));
        AnalyzeGeneratedCodeVB = ReadBoolean(settings.GetValueOrDefault("sonar.vbnet.analyzeGeneratedCode"));
    }

    public bool IgnoreHeaderComments(string language) =>
    language switch
    {
        LanguageNames.CSharp => IgnoreHeaderCommentsCS,
        LanguageNames.VisualBasic => IgnoreHeaderCommentsVB,
        _ => throw new UnexpectedLanguageException(language)
    };

    public bool AnalyzeGeneratedCode(string language) =>
        language switch
        {
            LanguageNames.CSharp => AnalyzeGeneratedCodeCS,
            LanguageNames.VisualBasic => AnalyzeGeneratedCodeVB,
            _ => throw new UnexpectedLanguageException(language)
        };

    public bool IsFileIncluded(string filePath, bool isTestProject) =>
        isTestProject
            ? IsFileIncluded(TestInclusions, TestExclusions, GlobalTestExclusions, filePath)
            : IsFileIncluded(Inclusions, Exclusions, GlobalExclusions, filePath);

    private static bool IsFileIncluded(string[] inclusions, string[] exclusions, string[] globalExclusions, string filePath) =>
        IsIncluded(inclusions, filePath)
        && !IsExcluded(exclusions, filePath)
        && !IsExcluded(globalExclusions, filePath);

    private static bool IsIncluded(string[] inclusions, string filePath) =>
        inclusions is { Length: 0 } || inclusions.Any(x => WildcardPatternMatcher.IsMatch(x, filePath, true));

    private static bool IsExcluded(string[] exclusions, string filePath) =>
        exclusions.Any(x => WildcardPatternMatcher.IsMatch(x, filePath, false));

    private static SonarLintXml ParseContent(SourceText sonarLintXml)
    {
        try
        {
            var serializer = new XmlSerializer(typeof(SonarLintXml));
            using var sr = new StringReader(sonarLintXml.ToString());
            return (SonarLintXml)serializer.Deserialize(sr);
        }
        catch
        {
            return SonarLintXml.Empty;
        }
    }

    private List<SonarLintXmlRule> ReadRuleParameters() =>
        sonarLintXml is { Rules: { } rules }
            ? rules.Where(x => x.Parameters.Any()).ToList()
            : new();

    private Dictionary<string, string> SettingsToDictionary() =>
    sonarLintXml is { Settings: { } settingsList }
        ? settingsList.ToDictionary(x => x.Key, x => x.Value)
        : new();

    private static string[] ReadCommaSeparatedArray(string str) =>
        string.IsNullOrEmpty(str) ? Array.Empty<string>() : str.Split(',');

    private static bool ReadBoolean(string str, bool defaultValue = false) =>
        bool.TryParse(str, out var propertyValue) ? propertyValue : defaultValue;
}
