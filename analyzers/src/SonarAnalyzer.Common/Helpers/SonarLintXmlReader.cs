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

    private bool? ignoreHeaderCommentsCS;
    private bool? ignoreHeaderCommentsVB;
    public bool IgnoreHeaderComments(string language) =>
        language switch
        {
            LanguageNames.CSharp => ignoreHeaderCommentsCS ??= ReadBoolean(ReadSettingsProperty("sonar.cs.ignoreHeaderComments")),
            LanguageNames.VisualBasic => ignoreHeaderCommentsVB ??= ReadBoolean(ReadSettingsProperty("sonar.vbnet.ignoreHeaderComments")),
            _ => throw new UnexpectedLanguageException(language)
        };

    private bool? analyzeGeneratedCodeCS;
    private bool? analyzeGeneratedCodeVB;
    public bool AnalyzeGeneratedCode(string language) =>
        language switch
        {
            LanguageNames.CSharp => analyzeGeneratedCodeCS ??= ReadBoolean(ReadSettingsProperty("sonar.cs.analyzeGeneratedCode")),
            LanguageNames.VisualBasic => analyzeGeneratedCodeVB ??= ReadBoolean(ReadSettingsProperty("sonar.vbnet.analyzeGeneratedCode")),
            _ => throw new UnexpectedLanguageException(language)
        };

    private string[] exclusions;
    public string[] Exclusions => exclusions ??= ReadCommaSeparatedArray(ReadSettingsProperty("sonar.exclusions"));

    private string[] inclusions;
    public string[] Inclusions => inclusions ??= ReadCommaSeparatedArray(ReadSettingsProperty("sonar.inclusions"));

    private string[] globalExclusions;
    public string[] GlobalExclusions => globalExclusions ??= ReadCommaSeparatedArray(ReadSettingsProperty("sonar.global.exclusions"));

    private string[] testExclusions;
    public string[] TestExclusions => testExclusions ??= ReadCommaSeparatedArray(ReadSettingsProperty("sonar.test.exclusions"));

    private string[] testInclusions;
    public string[] TestInclusions => testInclusions ??= ReadCommaSeparatedArray(ReadSettingsProperty("sonar.test.inclusions"));

    private string[] globalTestExclusions;
    public string[] GlobalTestExclusions => globalTestExclusions ??= ReadCommaSeparatedArray(ReadSettingsProperty("sonar.global.test.exclusions"));

    private List<SonarLintXmlRule> parametrizedRules;
    public List<SonarLintXmlRule> ParametrizedRules => parametrizedRules ??= ReadRuleParameters();

    public SonarLintXmlReader(SourceText sonarLintXml) =>
        this.sonarLintXml = sonarLintXml == null ? SonarLintXml.Empty : ParseContent(sonarLintXml);

    private static SonarLintXml ParseContent(SourceText sonarLintXml)
    {
        try
        {
            var serializer = new XmlSerializer(typeof(SonarLintXml));
            var byteArray = Encoding.UTF8.GetBytes(sonarLintXml.ToString());
            var stream = new MemoryStream(byteArray);
            using var sr = new StreamReader(stream, Encoding.UTF8, false);
            using var reader = XmlReader.Create(sr);
            return (SonarLintXml)serializer.Deserialize(reader);
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
