/*
 * SonarAnalyzer for .NET
 * Copyright (C) SonarSource Sàrl
 * mailto:info AT sonarsource DOT com
 *
 * You can redistribute and/or modify this program under the terms of
 * the Sonar Source-Available License Version 1, as published by SonarSource Sàrl.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
 * See the Sonar Source-Available License for more details.
 *
 * You should have received a copy of the Sonar Source-Available License
 * along with this program; if not, see https://sonarsource.com/license/ssal/
 */

using SonarAnalyzer.CSharp.Rules;

namespace SonarAnalyzer.Test.Rules;

[TestClass]
public class LoggerMembersNamesShouldComplyTest
{
    private static readonly VerifierBuilder Builder = new VerifierBuilder<LoggerMembersNamesShouldComply>();

    public TestContext TestContext { get; set; }

    [TestMethod]
    [DataRow("log")]
    [DataRow("_log")]
    [DataRow("Log")]
    [DataRow("_Log")]
    [DataRow("logger")]
    [DataRow("_logger")]
    [DataRow("Logger")]
    [DataRow("_Logger")]
    [DataRow("instance")]
    [DataRow("Instance")]
    public void LoggerMembersNamesShouldComply_Compliant_CS(string name) =>
        Builder.AddSnippet($$"""
            using System;
            using Microsoft.Extensions.Logging;

            public class One
            {
                ILogger {{name}};                       // Compliant
            }
            public class Two
            {
                ILogger<string> {{name}} { get; set; }  // Compliant
            }
            """)
            .AddReferences(NuGetMetadataReference.MicrosoftExtensionsLoggingAbstractions())
            .VerifyNoIssues();

    [TestMethod]
    public void LoggerMembersNamesShouldComply_MicrosoftExtensionsLogging_CS() =>
        Builder.AddPaths("LoggerMembersNamesShouldComply.MicrosoftExtensionsLogging.cs")
            .AddReferences(NuGetMetadataReference.MicrosoftExtensionsLoggingAbstractions())
            .Verify();

    [TestMethod]
    public void LoggerMembersNamesShouldComply_Serilog_CS() =>
        Builder.AddPaths("LoggerMembersNamesShouldComply.Serilog.cs")
            .AddReferences(NuGetMetadataReference.Serilog())
            .Verify();

    [TestMethod]
    public void LoggerMembersNamesShouldComply_NLog_CS() =>
        Builder.AddPaths("LoggerMembersNamesShouldComply.NLog.cs")
            .AddReferences(NuGetMetadataReference.NLog())
            .Verify();

    [TestMethod]
    public void LoggerMembersNamesShouldComply_log4net_CS() =>
        Builder.AddPaths("LoggerMembersNamesShouldComply.log4net.cs")
            .AddReferences(NuGetMetadataReference.Log4Net(TestConstants.NuGetLatestVersion, "netstandard2.0"))
            .Verify();

    [TestMethod]
    public void LoggerMembersNamesShouldComply_CastleCore_CS() =>
        Builder.AddPaths("LoggerMembersNamesShouldComply.CastleCore.cs")
            .AddReferences(NuGetMetadataReference.CastleCore())
            .Verify();

    [TestMethod]
    public void LoggerMembersNamesShouldComply_Parameterized_CS() =>
        new VerifierBuilder()
            .AddAnalyzer(() => new LoggerMembersNamesShouldComply { Format = "^chocolate$" })
            .AddPaths("LoggerMembersNamesShouldComply.Parameterized.cs")
            .AddReferences(NuGetMetadataReference.MicrosoftExtensionsLoggingAbstractions())
            .Verify();

    [TestMethod]
    public void LoggerMembersNamesShouldComply_FormatFromSonarLintXml_CS() => // NET-4546 The 'format' parameter was ignored, because ParameterLoader did not convert PropertyType.RegularExpression
        Builder.AddPaths("LoggerMembersNamesShouldComply.FormatFromSonarLintXml.cs")
            .AddReferences(NuGetMetadataReference.MicrosoftExtensionsLoggingAbstractions())
            .WithAdditionalFilePath(CreateSonarLintXmlWithFormat("^chocolate$"))
            .Verify();

    [TestMethod]
    public void LoggerMembersNamesShouldComply_InvalidFormatFromSonarLintXml_FallsBackToDefault_CS() =>
        Builder.AddPaths("LoggerMembersNamesShouldComply.InvalidFormatFromSonarLintXml.cs")
            .AddReferences(NuGetMetadataReference.MicrosoftExtensionsLoggingAbstractions())
            .WithAdditionalFilePath(CreateSonarLintXmlWithFormat("^m?Logger($")) // Unbalanced parenthesis, the analysis should not fail
            .Verify();

    private string CreateSonarLintXmlWithFormat(string format) =>
        AnalysisScaffolding.CreateSonarLintXml(TestContext, rulesParameters:
        [
            new()
            {
                Key = "S6669",
                Parameters = [new() { Key = "format", Value = format }]
            }
        ]);
}
