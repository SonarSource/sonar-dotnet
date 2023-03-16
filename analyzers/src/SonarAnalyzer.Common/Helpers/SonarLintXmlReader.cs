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
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using Microsoft.CodeAnalysis.Text;

namespace SonarAnalyzer.Helpers;

public class SonarLintXmlReader
{
    public static readonly SonarLintXmlReader Empty = new(null);

    private readonly SonarLintXml sonarLintXml;

    public string[] Exclusions { get => Exclusions; init => ReadCommaSeparatedArray(ReadSettingsProperty("sonar.exclusions")); }
    public string[] Inclusions { get => Inclusions;  init => ReadCommaSeparatedArray(ReadSettingsProperty("sonar.inclusions")); }
    public string[] GlobalExclusions { get => GlobalExclusions; init => ReadCommaSeparatedArray(ReadSettingsProperty("sonar.global.exclusions")); }
    public string[] TestExclusions { get => TestExclusions; init => ReadCommaSeparatedArray(ReadSettingsProperty("sonar.test.exclusions")); }
    public string[] TestInclusions { get => TestInclusions; init => ReadCommaSeparatedArray(ReadSettingsProperty("sonar.test.inclusions")); }
    public string[] GlobalTestExclusions { get => GlobalTestExclusions; init => ReadCommaSeparatedArray(ReadSettingsProperty("sonar.global.test.exclusions")); }
    public List<SonarLintXmlRule> ParametrizedRules { get => ParametrizedRules; init => ReadRuleParameters(); }

    private bool IgnoreHeaderCommentsCS { get => IgnoreHeaderCommentsCS; init => ReadBoolean(ReadSettingsProperty("sonar.cs.ignoreHeaderComments")); }
    private bool IgnoreHeaderCommentsVB { get => IgnoreHeaderCommentsVB; init => ReadBoolean(ReadSettingsProperty("sonar.vbnet.ignoreHeaderComments")); }
    private bool AnalyzeGeneratedCodeCS { get => AnalyzeGeneratedCodeCS; init => ReadBoolean(ReadSettingsProperty("sonar.cs.analyzeGeneratedCode")); }
    private bool AnalyzeGeneratedCodeVB { get => AnalyzeGeneratedCodeVB; init => ReadBoolean(ReadSettingsProperty("sonar.vbnet.analyzeGeneratedCode")); }

    public SonarLintXmlReader(SourceText sonarLintXml)
    {
        this.sonarLintXml = sonarLintXml == null ? SonarLintXml.Empty : ParseContent(sonarLintXml);
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
        inclusions.Length == 0 || inclusions.Any(x => WildcardPatternMatcher.IsMatch(x, filePath, true));

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

    private string ReadSettingsProperty(string property) =>
        sonarLintXml is { Settings: { } settings }
            ? settings.Where(x => x.Key.Equals(property)).Select(x => x.Value).FirstOrDefault()
            : null;

    private static string[] ReadCommaSeparatedArray(string str) =>
        string.IsNullOrEmpty(str) ? Array.Empty<string>() : str.Split(',');

    private static bool ReadBoolean(string str, bool defaultValue = false) =>
        bool.TryParse(str, out var propertyValue) ? propertyValue : defaultValue;
}
