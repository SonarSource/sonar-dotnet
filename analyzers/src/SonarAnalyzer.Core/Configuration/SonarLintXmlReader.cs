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

using System.IO;
using System.Xml.Serialization;
using Microsoft.CodeAnalysis.Text;

namespace SonarAnalyzer.Core.Configuration;

public class SonarLintXmlReader
{
    public static readonly SonarLintXmlReader Empty = new(null);
    private readonly bool ignoreHeaderCommentsCS;
    private readonly bool ignoreHeaderCommentsVB;
    private readonly bool analyzeGeneratedCodeCS;
    private readonly bool analyzeGeneratedCodeVB;
    private readonly bool analyzeRazorCodeCS;

    public string[] Exclusions { get; }
    public string[] Inclusions { get; }
    public string[] GlobalExclusions { get; }
    public string[] TestExclusions { get; }
    public string[] TestInclusions { get; }
    public string[] GlobalTestExclusions { get; }
    public List<SonarLintXmlRule> ParametrizedRules { get; }

    public SonarLintXmlReader(SourceText sonarLintXmlText)
    {
        var sonarLintXml = sonarLintXmlText is null ? SonarLintXml.Empty : ParseContent(sonarLintXmlText);
        var settings = sonarLintXml.Settings?.GroupBy(x => x.Key).ToDictionary(x => x.Key, x => x.First().Value) ?? new Dictionary<string, string>();
        Exclusions = ReadArray("sonar.exclusions");
        Inclusions = ReadArray("sonar.inclusions");
        GlobalExclusions = ReadArray("sonar.global.exclusions");
        TestExclusions = ReadArray("sonar.test.exclusions");
        TestInclusions = ReadArray("sonar.test.inclusions");
        GlobalTestExclusions = ReadArray("sonar.global.test.exclusions");
        ParametrizedRules = ReadRuleParameters();
        ignoreHeaderCommentsCS = ReadBoolean("sonar.cs.ignoreHeaderComments");
        ignoreHeaderCommentsVB = ReadBoolean("sonar.vbnet.ignoreHeaderComments");
        analyzeGeneratedCodeCS = ReadBoolean("sonar.cs.analyzeGeneratedCode");
        analyzeGeneratedCodeVB = ReadBoolean("sonar.vbnet.analyzeGeneratedCode");
        analyzeRazorCodeCS = ReadBoolean("sonar.cs.analyzeRazorCode", true);

        string[] ReadArray(string key) =>
            settings.GetValueOrDefault(key) is { } value && !string.IsNullOrEmpty(value)
                ? value.Split(',')
                : Array.Empty<string>();

        bool ReadBoolean(string key, bool defaultValue = false) =>
            settings.TryGetValue(key, out var value)
                ? bool.TryParse(value, out var boolValue) && boolValue
                : defaultValue;

        List<SonarLintXmlRule> ReadRuleParameters() =>
            sonarLintXml.Rules?.Where(x => x.Parameters.Any()).ToList() ?? new();
    }

    public bool IgnoreHeaderComments(string language) =>
    language switch
    {
        LanguageNames.CSharp => ignoreHeaderCommentsCS,
        LanguageNames.VisualBasic => ignoreHeaderCommentsVB,
        _ => throw new UnexpectedLanguageException(language)
    };

    public bool AnalyzeGeneratedCode(string language) =>
        language switch
        {
            LanguageNames.CSharp => analyzeGeneratedCodeCS,
            LanguageNames.VisualBasic => analyzeGeneratedCodeVB,
            _ => throw new UnexpectedLanguageException(language)
        };

    public bool AnalyzeRazorCode(string language) =>
        language switch
        {
            LanguageNames.CSharp => analyzeRazorCodeCS,
            LanguageNames.VisualBasic => false,
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
}
