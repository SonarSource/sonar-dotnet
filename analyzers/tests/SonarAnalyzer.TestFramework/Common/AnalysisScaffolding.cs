﻿/*
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

using System.IO;
using System.Xml.Linq;
using Microsoft.CodeAnalysis.Text;
using NSubstitute;
using SonarAnalyzer.AnalysisContext;
using RoslynAnalysisContext = Microsoft.CodeAnalysis.Diagnostics.AnalysisContext;

namespace SonarAnalyzer.TestFramework.Common;

public static class AnalysisScaffolding
{
    public static SonarAnalysisContext CreateSonarAnalysisContext() =>
        new(Substitute.For<RoslynAnalysisContext>(), ImmutableArray<DiagnosticDescriptor>.Empty);

    public static AnalyzerOptions CreateOptions() =>
        new(ImmutableArray<AdditionalText>.Empty);

    public static AnalyzerOptions CreateOptions(string relativePath)
    {
        var text = File.Exists(relativePath) ? SourceText.From(File.ReadAllText(relativePath)) : null;
        return CreateOptions(relativePath, text);
    }

    public static AnalyzerOptions CreateOptions(string relativePath, SourceText text)
    {
        var additionalText = Substitute.For<AdditionalText>();
        additionalText.Path.Returns(relativePath);
        additionalText.GetText(default).Returns(text);
        return new AnalyzerOptions(ImmutableArray.Create(additionalText));
    }

    public static DiagnosticDescriptor CreateDescriptorMain(string id = "Sxxxx") =>
        CreateDescriptor(id, DiagnosticDescriptorFactory.MainSourceScopeTag);

    public static DiagnosticDescriptor CreateDescriptor(string id, params string[] customTags) =>
        new(id, "Title", "Message for " + id, "Category", DiagnosticSeverity.Warning, true, customTags: customTags);

    public static string CreateAnalysisConfig(TestContext context, IEnumerable<string> unchangedFiles) =>
        CreateAnalysisConfig(context, "UnchangedFilesPath", TestHelper.WriteFile(context, "UnchangedFiles.txt", unchangedFiles.JoinStr(Environment.NewLine)));

    public static string CreateAnalysisConfig(TestContext context, string settingsId, string settingValue) =>
        TestHelper.WriteFile(context, "SonarQubeAnalysisConfig.xml", $"""
            <?xml version="1.0" encoding="utf-8"?>
            <AnalysisConfig xmlns:xsd="http://www.w3.org/2001/XMLSchema" xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" xmlns="http://www.sonarsource.com/msbuild/integration/2015/1">
                <AdditionalConfig>
                    <ConfigSetting Id="{settingsId}" Value="{settingValue}" />
                </AdditionalConfig>
            </AnalysisConfig>
            """);

    public static string CreateSonarProjectConfigWithFilesToAnalyze(TestContext context, params string[] filesToAnalyze)
    {
        var filesToAnalyzePath = TestHelper.TestPath(context, "FilesToAnalyze.txt");
        File.WriteAllLines(filesToAnalyzePath, filesToAnalyze);
        return CreateSonarProjectConfig(context, "FilesToAnalyzePath", filesToAnalyzePath, true);
    }

    public static string CreateSonarProjectConfigWithUnchangedFiles(TestContext context, params string[] unchangedFiles) =>
        CreateSonarProjectConfig(context, "NotImportant", null, true, CreateAnalysisConfig(context, unchangedFiles));

    public static string CreateSonarProjectConfig(TestContext context, ProjectType projectType, bool isScannerRun = true) =>
        CreateSonarProjectConfig(context, "ProjectType", projectType.ToString(), isScannerRun);

    public static string CreateSonarLintXml(
        TestContext context,
        string language = LanguageNames.CSharp,
        bool analyzeGeneratedCode = false,
        bool ignoreHeaderComments = false,
        string[] exclusions = null,
        string[] inclusions = null,
        string[] globalExclusions = null,
        string[] testExclusions = null,
        string[] testInclusions = null,
        string[] globalTestExclusions = null,
        List<SonarLintXmlRule> rulesParameters = null,
        bool analyzeRazorCode = true) =>
        TestHelper.WriteFile(context, "SonarLint.xml", GenerateSonarLintXmlContent(language, analyzeGeneratedCode, ignoreHeaderComments, exclusions, inclusions, globalExclusions, testExclusions, testInclusions, globalTestExclusions, rulesParameters, analyzeRazorCode));

    public static string GenerateSonarLintXmlContent(
        string language = LanguageNames.CSharp,
        bool analyzeGeneratedCode = false,
        bool ignoreHeaderComments = false,
        string[] exclusions = null,
        string[] inclusions = null,
        string[] globalExclusions = null,
        string[] testExclusions = null,
        string[] testInclusions = null,
        string[] globalTestExclusions = null,
        List<SonarLintXmlRule> rulesParameters = null,
        bool analyzeRazorCode = true) =>
        new XDocument(
            new XDeclaration("1.0", "utf-8", "yes"),
            new XElement("AnalysisInput",
                new XElement("Settings",
                    CreateSetting($"sonar.{(language == LanguageNames.CSharp ? "cs" : "vbnet")}.analyzeGeneratedCode", analyzeGeneratedCode.ToString()),
                    CreateSetting($"sonar.{(language == LanguageNames.CSharp ? "cs" : "vbnet")}.ignoreHeaderComments", ignoreHeaderComments.ToString()),
                    CreateSetting($"sonar.{(language == LanguageNames.CSharp ? "cs" : "vbnet")}.analyzeRazorCode", analyzeRazorCode.ToString()),
                    CreateSetting("sonar.exclusions", ConcatenateStringArray(exclusions)),
                    CreateSetting("sonar.inclusions", ConcatenateStringArray(inclusions)),
                    CreateSetting("sonar.global.exclusions", ConcatenateStringArray(globalExclusions)),
                    CreateSetting("sonar.test.exclusions", ConcatenateStringArray(testExclusions)),
                    CreateSetting("sonar.test.inclusions", ConcatenateStringArray(testInclusions)),
                    CreateSetting("sonar.global.test.exclusions", ConcatenateStringArray(globalTestExclusions))),
                new XElement("Rules", CreateRules(rulesParameters)))).ToString();

    private static IEnumerable<XElement> CreateRules(List<SonarLintXmlRule> ruleParameters)
    {
        foreach (var rule in ruleParameters ?? new())
        {
            yield return CreateRule(rule);
        }
    }

    private static XElement CreateRule(SonarLintXmlRule rule)
    {
        List<XElement> elements = new();
        foreach (var param in rule.Parameters)
        {
            elements.Add(CreateKeyValuePair("Parameter", param.Key, param.Value));
        }
        return new("Rule", new XElement("Key", rule.Key), new XElement("Parameters", elements));
    }

    private static XElement CreateSetting(string key, string value) =>
        CreateKeyValuePair("Setting", key, value);

    private static XElement CreateKeyValuePair(string containerName, string key, string value) =>
        new(containerName, new XElement("Key", key), new XElement("Value", value));

    private static string ConcatenateStringArray(string[] array) =>
        string.Join(",", array ?? Array.Empty<string>());

    private static string CreateSonarProjectConfig(TestContext context, string element, string value, bool isScannerRun, string analysisConfigPath = null)
    {
        var sonarProjectConfigPath = TestHelper.TestPath(context, "SonarProjectConfig.xml");
        var outPath = isScannerRun ? Path.GetDirectoryName(sonarProjectConfigPath) : null;
        analysisConfigPath ??= CreateAnalysisConfig(context, "NotImportant", null);
        var projectConfigContent = $"""
            <SonarProjectConfig xmlns="http://www.sonarsource.com/msbuild/analyzer/2021/1">
                <AnalysisConfigPath>{analysisConfigPath}</AnalysisConfigPath>
                <OutPath>{outPath}</OutPath>
                <{element}>{value}</{element}>
            </SonarProjectConfig>
            """;
        File.WriteAllText(sonarProjectConfigPath, projectConfigContent);
        return sonarProjectConfigPath;
    }
}
