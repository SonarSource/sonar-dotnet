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
    [DataRow(CodeAnalysisCS.LanguageVersion.CSharp7, "netstandard2.0")]
    [DataRow(CodeAnalysisCS.LanguageVersion.CSharp8, "netstandard2.1")]
    [DataRow(CodeAnalysisCS.LanguageVersion.CSharp9, "net5")]
    [DataRow(CodeAnalysisCS.LanguageVersion.CSharp10, "net6")]
    [DataRow(CodeAnalysisCS.LanguageVersion.CSharp11, "net7")]
    [DataRow(CodeAnalysisCS.LanguageVersion.CSharp12, "net8")]
    [DataRow(CodeAnalysisCS.LanguageVersion.CSharp13, "net9")]
    public async Task TelemetryAnalyzer_CreateAnalysisMessages_ValidData_CS(CodeAnalysisCS.LanguageVersion langVersion, string targetFramework)
    {
        var telemetry = await Telemetry_CS(langVersion, targetFramework);
        telemetry.Should().Be(new Protobuf.Telemetry
        {
            LanguageVersion = langVersion.ToString(),
            ProjectFullPath = "A.csproj",
            TargetFramework = { targetFramework },
        });
        telemetry.LanguageVersion.Should().StartWith("CSharp");
    }

    [TestMethod]
    public async Task TelemetryAnalyzer_CreateAnalysisMessages_InvalidLanguageVersion_CS()
    {
        var telemetry = await Telemetry_CS((CodeAnalysisCS.LanguageVersion)99999, "netX");
        telemetry.Should().Be(new Protobuf.Telemetry
        {
            LanguageVersion = "99999",
            ProjectFullPath = "A.csproj",
            TargetFramework = { "netX" },
        });
    }

    [TestMethod]
    [DataRow("", new string[] { })]
    [DataRow("   ", new string[] { })]
    [DataRow(" net6 ", new[] { "net6" })]
    [DataRow(" net5;   net6;net8   ", new[] { "net5", "net6", "net8" })]
    [DataRow("net5;;net8", new[] { "net5", "net8" })]
    [DataRow("wp [wp7]; uap10.0 [win10] [netcore50]", new[] { "wp [wp7]", "uap10.0 [win10] [netcore50]" })]
    [DataRow(";;", new string[] { })]
    [DataRow("net5-net6", new[] { "net5-net6" })]
    [DataRow("net5;äöüß😉", new[] { "net5", "äöüß😉" })]
    public async Task TelemetryAnalyzer_CreateAnalysisMessages_TargetFrameworkEdgeCases(string targetFramework, string[] expected)
    {
        var telemetry = await Telemetry_CS(CodeAnalysisCS.LanguageVersion.CSharp13, targetFramework);
        telemetry.Should().Be(new Protobuf.Telemetry
        {
            LanguageVersion = "CSharp13",
            ProjectFullPath = "A.csproj",
            TargetFramework = { expected },
        });
    }

    [TestMethod]
    [DataRow(CodeAnalysisVB.LanguageVersion.VisualBasic15_3, "netcoreapp3.1")]
    [DataRow(CodeAnalysisVB.LanguageVersion.VisualBasic15_5, "netstandard2.0")]
    [DataRow(CodeAnalysisVB.LanguageVersion.VisualBasic11, "net5")]
    [DataRow(CodeAnalysisVB.LanguageVersion.VisualBasic12, "net6")]
    [DataRow(CodeAnalysisVB.LanguageVersion.VisualBasic14, "net7")]
    [DataRow(CodeAnalysisVB.LanguageVersion.VisualBasic15, "net8")]
    [DataRow(CodeAnalysisVB.LanguageVersion.VisualBasic16, "net9")]
    public async Task TelemetryAnalyzer_CreateAnalysisMessages_ValidData_VB(CodeAnalysisVB.LanguageVersion langVersion, string targetFramework)
    {
        var telemetry = await Telemetry_VB(langVersion, targetFramework);
        telemetry.Should().Be(new Protobuf.Telemetry
        {
            LanguageVersion = langVersion.ToString(),
            ProjectFullPath = "A.vbproj",
            TargetFramework = { targetFramework },
        });
        telemetry.LanguageVersion.Should().StartWith("VisualBasic");
    }

    [TestMethod]
    public async Task TelemetryAnalyzer_CreateAnalysisMessages_InvalidLanguageVersion_VB()
    {
        var telemetry = await Telemetry_VB((CodeAnalysisVB.LanguageVersion)99999, "netX");
        telemetry.Should().Be(new Protobuf.Telemetry
        {
            LanguageVersion = "99999",
            ProjectFullPath = "A.vbproj",
            TargetFramework = { "netX" },
        });
    }

    [TestMethod]
    public async Task TelemetryAnalyzer_CreateAnalysisMessages_MissingTargetFramework()
    {
        var telemetry = await Telemetry_CS(CodeAnalysisCS.LanguageVersion.CSharp12, null);
        telemetry.Should().Be(new Protobuf.Telemetry
        {
            LanguageVersion = "CSharp12",
            ProjectFullPath = "A.csproj",
            TargetFramework = { }
        });
    }

    [TestMethod]
    public async Task TelemetryAnalyzer_CreateAnalysisMessages_MultipleTargetFrameworks9()
    {
        var telemetry = await Telemetry_CS(CodeAnalysisCS.LanguageVersion.CSharp13, "net5;net6;net8");
        telemetry.Should().Be(new Protobuf.Telemetry
        {
            LanguageVersion = "CSharp13",
            ProjectFullPath = "A.csproj",
            TargetFramework = { "net5", "net6", "net8" },
        });
    }

    [TestMethod]
    public async Task TelemetryAnalyzer_CreateAnalysisMessages_MissingProjectName()
    {
        var telemetry = await Telemetry_CS(CodeAnalysisCS.LanguageVersion.CSharp12, "net5", null);
        telemetry.Should().Be(new Protobuf.Telemetry
        {
            LanguageVersion = "CSharp12",
            ProjectFullPath = string.Empty,
            TargetFramework = { "net5" }
        });
    }

    private async Task<Protobuf.Telemetry> Telemetry_CS(CodeAnalysisCS.LanguageVersion languageVersion, string targetFramework, string projectName = "A.csproj")
    {
        var syntaxTrees = new[] { CodeAnalysisCS.SyntaxFactory.ParseSyntaxTree(string.Empty, CodeAnalysisCS.CSharpParseOptions.Default.WithLanguageVersion(languageVersion)) };
        var compilation = CodeAnalysisCS.CSharpCompilation.Create(null, syntaxTrees);
        var outPath = Path.Combine(BasePath, TestContext.TestName, Interlocked.Increment(ref testNumber).ToString());

        var compilationWithAnalyzer = compilation.WithAnalyzers(
            [new CS.TelemetryAnalyzer()],
            new AnalyzerOptions([SonarProjectConfigXmlMock(projectName, outPath, targetFramework), SonarLintXmlMock()]));
        var result = await compilationWithAnalyzer.GetAnalysisResultAsync(CancellationToken.None);
        result.GetAllDiagnostics().Should().BeEmpty();
        return ParseTelemetryProtobuf(Path.Combine(outPath, "output-cs", "telemetry.pb"));
    }

    private async Task<Protobuf.Telemetry> Telemetry_VB(CodeAnalysisVB.LanguageVersion languageVersion, string targetFramework, string projectName = "A.vbproj")
    {
        var syntaxTrees = new[] { CodeAnalysisVB.SyntaxFactory.ParseSyntaxTree(string.Empty, CodeAnalysisVB.VisualBasicParseOptions.Default.WithLanguageVersion(languageVersion)) };
        var compilation = CodeAnalysisVB.VisualBasicCompilation.Create(null, syntaxTrees);
        var outPath = Path.Combine(BasePath, TestContext.TestName, Interlocked.Increment(ref testNumber).ToString());

        var compilationWithAnalyzer = compilation.WithAnalyzers(
            [new VB.TelemetryAnalyzer()],
            new AnalyzerOptions([SonarProjectConfigXmlMock(projectName, outPath, targetFramework), SonarLintXmlMock()]));
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

    private static AdditionalText SonarProjectConfigXmlMock(string projectName, string outPath, string targetFramework)
    {
        var config = Substitute.For<AdditionalText>();
        config.Path.Returns("SonarProjectConfig.xml");
        config.GetText().Returns(SourceText.From($"""
            <?xml version="1.0" encoding="utf-8"?>
            <SonarProjectConfig xmlns="http://www.sonarsource.com/msbuild/analyzer/2021/1">
              <AnalysisConfigPath>SonarQubeAnalysisConfig.xml</AnalysisConfigPath>
              {(projectName is not null ? $"<ProjectPath>{projectName}</ProjectPath>" : string.Empty)}
              {(outPath is not null ? $"<OutPath>{outPath}</OutPath>" : string.Empty)}
              {(targetFramework is not null ? $"<TargetFramework>{targetFramework}</TargetFramework>" : string.Empty)}
            </SonarProjectConfig>
            """));
        return config;
    }
}
