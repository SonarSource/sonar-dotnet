/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2014-2025 SonarSource Sàrl
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
using Microsoft.CodeAnalysis.Text;
using CodeAnalysisCS = Microsoft.CodeAnalysis.CSharp;
using CodeAnalysisVB = Microsoft.CodeAnalysis.VisualBasic;
using CS = SonarAnalyzer.CSharp.Rules;
using VB = SonarAnalyzer.VisualBasic.Rules;

namespace SonarAnalyzer.Test.Rules;

[TestClass]
public class TelemetryAnalyzerTest
{
    private const string BasePath = @"Utilities\TelemetryAnalyzer";

    private static int testNumber = 0;

    public TestContext TestContext { get; set; }

    [TestMethod]
    [DataRow(CodeAnalysisCS.LanguageVersion.CSharp7)]
    [DataRow(CodeAnalysisCS.LanguageVersion.CSharp8)]
    [DataRow(CodeAnalysisCS.LanguageVersion.CSharp9)]
    [DataRow(CodeAnalysisCS.LanguageVersion.CSharp10)]
    [DataRow(CodeAnalysisCS.LanguageVersion.CSharp11)]
    [DataRow(CodeAnalysisCS.LanguageVersion.CSharp12)]
    [DataRow(CodeAnalysisCS.LanguageVersion.CSharp13)]
    public async Task TelemetryAnalyzer_CreateAnalysisMessages_ValidData_CS(CodeAnalysisCS.LanguageVersion langVersion)
    {
        var telemetry = await Telemetry_CS(langVersion);
        telemetry.Should().Be(new Protobuf.Telemetry
        {
            LanguageVersion = langVersion.ToString(),
            ProjectFullPath = "A.csproj",
        });
        telemetry.LanguageVersion.Should().StartWith("CSharp");
    }

    [TestMethod]
    public async Task TelemetryAnalyzer_CreateAnalysisMessages_InvalidLanguageVersion_CS()
    {
        var telemetry = await Telemetry_CS((CodeAnalysisCS.LanguageVersion)99999);
        telemetry.Should().Be(new Protobuf.Telemetry
        {
            LanguageVersion = "99999",
            ProjectFullPath = "A.csproj",
        });
    }

    [TestMethod]
    [DataRow(CodeAnalysisVB.LanguageVersion.VisualBasic15_3)]
    [DataRow(CodeAnalysisVB.LanguageVersion.VisualBasic15_5)]
    [DataRow(CodeAnalysisVB.LanguageVersion.VisualBasic11)]
    [DataRow(CodeAnalysisVB.LanguageVersion.VisualBasic12)]
    [DataRow(CodeAnalysisVB.LanguageVersion.VisualBasic14)]
    [DataRow(CodeAnalysisVB.LanguageVersion.VisualBasic15)]
    [DataRow(CodeAnalysisVB.LanguageVersion.VisualBasic16)]
    public async Task TelemetryAnalyzer_CreateAnalysisMessages_ValidData_VB(CodeAnalysisVB.LanguageVersion langVersion)
    {
        var telemetry = await Telemetry_VB(langVersion);
        telemetry.Should().Be(new Protobuf.Telemetry
        {
            LanguageVersion = langVersion.ToString(),
            ProjectFullPath = "A.vbproj",
        });
        telemetry.LanguageVersion.Should().StartWith("VisualBasic");
    }

    [TestMethod]
    public async Task TelemetryAnalyzer_CreateAnalysisMessages_InvalidLanguageVersion_VB()
    {
        var telemetry = await Telemetry_VB((CodeAnalysisVB.LanguageVersion)99999);
        telemetry.Should().Be(new Protobuf.Telemetry
        {
            LanguageVersion = "99999",
            ProjectFullPath = "A.vbproj",
        });
    }

    [TestMethod]
    public async Task TelemetryAnalyzer_CreateAnalysisMessages_MissingProjectName()
    {
        var telemetry = await Telemetry_CS(CodeAnalysisCS.LanguageVersion.CSharp12, null);
        telemetry.Should().Be(new Protobuf.Telemetry
        {
            LanguageVersion = "CSharp12",
            ProjectFullPath = string.Empty,
        });
    }

    private async Task<Protobuf.Telemetry> Telemetry_CS(CodeAnalysisCS.LanguageVersion languageVersion, string projectName = "A.csproj")
    {
        var syntaxTrees = new[] { CodeAnalysisCS.SyntaxFactory.ParseSyntaxTree(string.Empty, CodeAnalysisCS.CSharpParseOptions.Default.WithLanguageVersion(languageVersion)) };
        var compilation = CodeAnalysisCS.CSharpCompilation.Create(null, syntaxTrees);
        var outPath = Path.Combine(BasePath, TestContext.TestName, Interlocked.Increment(ref testNumber).ToString());

        var compilationWithAnalyzer = compilation.WithAnalyzers(
            [new CS.TelemetryAnalyzer()],
            new AnalyzerOptions([SonarProjectConfigXmlMock(projectName, outPath), SonarLintXmlMock()]));
        var result = await compilationWithAnalyzer.GetAnalysisResultAsync(CancellationToken.None);
        result.GetAllDiagnostics().Should().BeEmpty();
        return ParseTelemetryProtobuf(Path.Combine(outPath, "output-cs", "telemetry.pb"));
    }

    private async Task<Protobuf.Telemetry> Telemetry_VB(CodeAnalysisVB.LanguageVersion languageVersion, string projectName = "A.vbproj")
    {
        var syntaxTrees = new[] { CodeAnalysisVB.SyntaxFactory.ParseSyntaxTree(string.Empty, CodeAnalysisVB.VisualBasicParseOptions.Default.WithLanguageVersion(languageVersion)) };
        var compilation = CodeAnalysisVB.VisualBasicCompilation.Create(null, syntaxTrees);
        var outPath = Path.Combine(BasePath, TestContext.TestName, Interlocked.Increment(ref testNumber).ToString());

        var compilationWithAnalyzer = compilation.WithAnalyzers(
            [new VB.TelemetryAnalyzer()],
            new AnalyzerOptions([SonarProjectConfigXmlMock(projectName, outPath), SonarLintXmlMock()]));
        var result = await compilationWithAnalyzer.GetAnalysisResultAsync(CancellationToken.None);
        result.GetAllDiagnostics().Should().BeEmpty();
        return ParseTelemetryProtobuf(Path.Combine(outPath, "output-vbnet", "telemetry.pb"));
    }

    private Protobuf.Telemetry ParseTelemetryProtobuf(string protobufFilePath)
    {
        File.Exists(protobufFilePath).Should().BeTrue();
        TestContext.AddResultFile(protobufFilePath); // Including the file in the test results
        using var protoFile = File.Open(protobufFilePath, FileMode.Open);
        return Protobuf.Telemetry.Parser.ParseDelimitedFrom(protoFile);
    }

    private static AdditionalText SonarLintXmlMock()
    {
        var sonarLint = Substitute.For<AdditionalText>();
        sonarLint.Path.Returns("SonarLint.xml");
        sonarLint.GetText().Returns(SourceText.From("""
            <?xml version="1.0" encoding="UTF-8"?>
            <AnalysisInput/>
            """));
        return sonarLint;
    }

    private static AdditionalText SonarProjectConfigXmlMock(string projectName, string outPath)
    {
        var config = Substitute.For<AdditionalText>();
        config.Path.Returns("SonarProjectConfig.xml");
        config.GetText().Returns(SourceText.From($"""
            <?xml version="1.0" encoding="utf-8"?>
            <SonarProjectConfig xmlns="http://www.sonarsource.com/msbuild/analyzer/2021/1">
              <AnalysisConfigPath>SonarQubeAnalysisConfig.xml</AnalysisConfigPath>
              {(projectName is not null ? $"<ProjectPath>{projectName}</ProjectPath>" : string.Empty)}
              {(outPath is not null ? $"<OutPath>{outPath}</OutPath>" : string.Empty)}
            </SonarProjectConfig>
            """));
        return config;
    }
}
