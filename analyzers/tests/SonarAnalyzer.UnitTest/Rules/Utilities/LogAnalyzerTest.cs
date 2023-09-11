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

using SonarAnalyzer.Protobuf;
using SonarAnalyzer.Rules;
using SonarAnalyzer.UnitTest.Helpers;
using CS = SonarAnalyzer.Rules.CSharp;
using VB = SonarAnalyzer.Rules.VisualBasic;

namespace SonarAnalyzer.UnitTest.Rules
{
    [TestClass]
    public class LogAnalyzerTest
    {
        private const string BasePath = @"Utilities\LogAnalyzer\";

        public TestContext TestContext { get; set; }

        [TestMethod]
        public void LogCompilationMessages_CS() =>
            Verify(new[] { "Normal.cs", "Second.cs" }, VerifyCompilationMessagesConcurrentRuleExecution);

        [TestMethod]
        public void LogCompilationMessages_CS_NonConcurrent()
        {
            using var scope = new EnvironmentVariableScope(false) { EnableConcurrentAnalysis = false };
            Verify(new[] { "Normal.cs", "Second.cs" }, VerifyCompilationMessagesNonConcurrentRuleExecution);
        }

        [TestMethod]
        public void LogCompilationMessages_VB() =>
            Verify(new[] { "Normal.vb", "Second.vb" }, VerifyCompilationMessagesConcurrentRuleExecution);

        [TestMethod]
        public void LogAutogenerated_CS() =>
            Verify(new[] { "Normal.cs", "GeneratedByName.generated.cs", "GeneratedByContent.cs" }, VerifyGenerated);

        [TestMethod]
        public void LogAutogenerated_VB() =>
            Verify(new[] { "Normal.vb", "GeneratedByName.generated.vb", "GeneratedByContent.vb" }, VerifyGenerated);

        [TestMethod]
        public void LogAutogenerated_CsHtml()
        {
            const string fileName = "Generated_cshtml.g.cs";
            CreateBuilder(fileName)
                .WithAdditionalFilePath(AnalysisScaffolding.CreateSonarProjectConfig(TestContext, ProjectType.Product))
                .VerifyUtilityAnalyzer<LogInfo>(logs => logs.Should().NotContain(x => x.Text.IndexOf(fileName, StringComparison.OrdinalIgnoreCase) != -1));
        }

        [TestMethod]
        public void LogAutogenerated_Razor() =>
            CreateBuilder("Generated_razor.g.cs")
                .WithAdditionalFilePath(AnalysisScaffolding.CreateSonarProjectConfig(TestContext, ProjectType.Product))
                .VerifyUtilityAnalyzer<LogInfo>(logs => logs.Select(x => x.Text).Should().Contain($"File 'Utilities\\LogAnalyzer\\Generated_razor.g.cs' was recognized as razor generated"));

        [DataTestMethod]
        [DataRow("GeneratedByName.generated.cs", 0)]
        [DataRow("SomethingElse.cs", 1)]
        public void Verify_UnchangedFiles(string unchangedFileName, int expectedGeneratedFiles) =>
            CreateBuilder("GeneratedByName.generated.cs")
                .WithAdditionalFilePath(AnalysisScaffolding.CreateSonarProjectConfigWithUnchangedFiles(TestContext, BasePath + unchangedFileName))
                .VerifyUtilityAnalyzer<LogInfo>(x => x.Where(info => info.Text.Contains("generated")).Should().HaveCount(expectedGeneratedFiles + 1)); // +1 to ignore ExtraEmptyFile.g.cs

        private void Verify(string[] paths, Action<IReadOnlyList<LogInfo>> verifyProtobuf) =>
            CreateBuilder(paths)
                .WithAdditionalFilePath(AnalysisScaffolding.CreateSonarProjectConfig(TestContext, ProjectType.Product))
                .VerifyUtilityAnalyzer(verifyProtobuf);

        private VerifierBuilder CreateBuilder(params string[] paths)
        {
            var testRoot = BasePath + TestContext.TestName;
            var language = AnalyzerLanguage.FromPath(paths.First());
            UtilityAnalyzerBase analyzer = language.LanguageName switch
            {
                LanguageNames.CSharp => new TestLogAnalyzer_CS(testRoot),
                LanguageNames.VisualBasic => new TestLogAnalyzer_VB(testRoot),
                _ => throw new UnexpectedLanguageException(language)
            };
            return new VerifierBuilder()
                .AddAnalyzer(() => analyzer)
                .AddPaths(paths)
                .WithBasePath(BasePath)
                .WithProtobufPath(@$"{testRoot}\log.pb");
        }

        private static void VerifyCompilationMessagesNonConcurrentRuleExecution(IReadOnlyList<LogInfo> messages) =>
            VerifyCompilationMessagesBase(messages, "disabled");

        private static void VerifyCompilationMessagesConcurrentRuleExecution(IReadOnlyList<LogInfo> messages) =>
            VerifyCompilationMessagesBase(messages, "enabled");

        private static void VerifyCompilationMessagesBase(IReadOnlyList<LogInfo> messages, string expectedConcurrencyMessage)
        {
            VerifyRoslynVersion(messages);
            VerifyLanguageVersion(messages);
            VerifyConcurrentExecution(messages, expectedConcurrencyMessage);
        }

        private static void VerifyRoslynVersion(IReadOnlyList<LogInfo> messages)
        {
            messages.Should().NotBeEmpty();
            var versionMessage = messages.SingleOrDefault(x => x.Text.Contains("Roslyn version"));
            versionMessage.Should().NotBeNull();
            versionMessage.Severity.Should().Be(LogSeverity.Info);
            versionMessage.Text.Should().MatchRegex(@"^Roslyn version: \d+(\.\d+){3}");
            var version = new Version(versionMessage.Text.Substring(16));
            version.Should().BeGreaterThan(new Version(3, 0));  // Avoid 1.0.0.0
        }

        private static void VerifyLanguageVersion(IReadOnlyList<LogInfo> messages)
        {
            messages.Should().NotBeEmpty();
            var versionMessage = messages.SingleOrDefault(x => x.Text.Contains("Language version"));
            versionMessage.Should().NotBeNull();
            versionMessage.Severity.Should().Be(LogSeverity.Info);
            versionMessage.Text.Should().MatchRegex(@"^Language version: (Preview|(CSharp|VisualBasic)\d+)");
        }

        private static void VerifyConcurrentExecution(IReadOnlyList<LogInfo> messages, string expectedConcurrencyMessage)
        {
            messages.Should().NotBeEmpty();
            var executionState = messages.SingleOrDefault(x => x.Text.Contains("Concurrent execution: "));
            executionState.Should().NotBeNull();
            executionState.Severity.Should().Be(LogSeverity.Info);
            executionState.Text.Should().Be($"Concurrent execution: {expectedConcurrencyMessage}");
        }

        private static void VerifyGenerated(IReadOnlyList<LogInfo> messages)
        {
            messages.Should().NotBeEmpty();
            messages.FirstOrDefault(x => x.Text.Contains("Normal.")).Should().BeNull();

            var generatedByName = messages.SingleOrDefault(x => x.Text.Contains("GeneratedByName.generated."));
            generatedByName.Should().NotBeNull();
            generatedByName.Severity.Should().Be(LogSeverity.Debug);
            generatedByName.Text.Should().Match(@"File 'Utilities\LogAnalyzer\GeneratedByName.generated.*' was recognized as generated");

            var generatedByContent = messages.SingleOrDefault(x => x.Text.Contains("GeneratedByContent."));
            generatedByContent.Should().NotBeNull();
            generatedByContent.Severity.Should().Be(LogSeverity.Debug);
            generatedByContent.Text.Should().Match(@"File 'Utilities\LogAnalyzer\GeneratedByContent.*' was recognized as generated");
        }

        // We need to set protected properties and this class exists just to enable the analyzer without bothering with additional files with parameters
        private sealed class TestLogAnalyzer_CS : CS.LogAnalyzer
        {
            public TestLogAnalyzer_CS(string outPath)
            {
                IsAnalyzerEnabled = true;
                OutPath = outPath;
                IsTestProject = false;
            }
        }

        private sealed class TestLogAnalyzer_VB : VB.LogAnalyzer
        {
            public TestLogAnalyzer_VB(string outPath)
            {
                IsAnalyzerEnabled = true;
                OutPath = outPath;
                IsTestProject = false;
            }
        }
    }
}
