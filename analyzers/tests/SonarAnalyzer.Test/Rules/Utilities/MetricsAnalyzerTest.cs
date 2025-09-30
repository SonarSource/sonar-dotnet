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
using SonarAnalyzer.Core.AnalysisContext;
using SonarAnalyzer.Core.Rules;
using SonarAnalyzer.CSharp.Rules;
using SonarAnalyzer.Protobuf;

namespace SonarAnalyzer.Test.Rules
{
    [TestClass]
    public class MetricsAnalyzerTest
    {
        private const string BasePath = @"Utilities\MetricsAnalyzer\";
        private const string AllMetricsFileName = "AllMetrics.cs";
        private const string RazorFileName = "Razor.razor";
        private const string CsHtmlFileName = "Razor.cshtml";
        private const string CSharp12FileName = "Metrics.CSharp12.cs";
        private const string ImportsRazorFileName = "_Imports.razor";

        public TestContext TestContext { get; set; }

        [TestMethod]
        public void VerifyMetrics() =>
            CreateBuilder(false, AllMetricsFileName)
                .VerifyUtilityAnalyzer<MetricsInfo>(messages =>
                    {
                        messages.Should().ContainSingle();
                        var metrics = messages.Single();
                        metrics.FilePath.Should().Be(Path.Combine(BasePath, AllMetricsFileName));
                        metrics.ClassCount.Should().Be(4);
                        metrics.CodeLine.Should().HaveCount(24);
                        metrics.CognitiveComplexity.Should().Be(1);
                        metrics.Complexity.Should().Be(2);
                        metrics.ExecutableLines.Should().HaveCount(5);
                        metrics.FunctionCount.Should().Be(1);
                        metrics.NoSonarComment.Should().ContainSingle();
                        metrics.NonBlankComment.Should().ContainSingle();
                        metrics.StatementCount.Should().Be(5);
                    });

#if NET

        [TestMethod]
        public void VerifyMetrics_Razor() =>
            CreateBuilder(false, RazorFileName, "Component.razor")
                .VerifyUtilityAnalyzer<MetricsInfo>(messages =>
                {
                    var orderedMessages = messages.OrderBy(x => x.FilePath, StringComparer.InvariantCulture).ToArray();
                    orderedMessages.Select(x => Path.GetFileName(x.FilePath)).Should().BeEquivalentTo(RazorFileName, "Component.razor");

                    var metrics = messages.Single(x => x.FilePath.EndsWith(RazorFileName));

                    metrics.ClassCount.Should().Be(1);
                    metrics.CodeLine.Should().BeEquivalentTo(new[] { 1, 2, 3, 5, 8, 10, 13, 15, 16, 17, 19, 22, 23, 24, 26, 28, 29, 32, 33, 34, 36, 37, 39, 40, 43 });
                    metrics.CognitiveComplexity.Should().Be(3);
                    metrics.Complexity.Should().Be(4);
                    metrics.ExecutableLines.Should().BeEquivalentTo(new[] { 3, 5, 13, 15, 17, 24, 28, 29, 32, 36, 39, 43 }); // This is incorrect, see https://sonarsource.atlassian.net/browse/NET-2052
                    metrics.FunctionCount.Should().Be(1);
                    metrics.NoSonarComment.Should().BeEmpty();
                    metrics.NonBlankComment.Should().BeEquivalentTo(new[] { 7, 8, 10, 15, 21, 22, 23, 28, 29, 32, 33, 36, 37, 38 });
                    metrics.StatementCount.Should().Be(13); // This is incorrect, see https://sonarsource.atlassian.net/browse/NET-2052
                });

        [TestMethod]
        // This is incorrect, see see https://sonarsource.atlassian.net/browse/NET-2052
        public void VerifyMetrics_Razor_Usings() =>
            CreateBuilder(false, ImportsRazorFileName)
                .VerifyUtilityAnalyzer<MetricsInfo>(messages =>
                {
                    var orderedMessages = messages.OrderBy(x => x.FilePath, StringComparer.InvariantCulture).ToArray();
                    orderedMessages.Select(x => Path.GetFileName(x.FilePath)).Should().BeEquivalentTo(ImportsRazorFileName);

                    var metrics = messages.Single(x => x.FilePath.EndsWith(ImportsRazorFileName));

                    metrics.ClassCount.Should().Be(0);
                    metrics.CodeLine.Should().BeEquivalentTo(new[] { 1, 2, 4, 5 });  // FN: this file has only 3 lines. See: https://github.com/SonarSource/sonar-dotnet/issues/9288 and https://sonarsource.atlassian.net/browse/NET-2052
                    metrics.CognitiveComplexity.Should().Be(0);
                    metrics.Complexity.Should().Be(1);
                    metrics.ExecutableLines.Should().BeEquivalentTo(Array.Empty<int>());
                    metrics.FunctionCount.Should().Be(0);
                    metrics.NoSonarComment.Should().BeEmpty();
                    metrics.NonBlankComment.Should().BeEquivalentTo(Array.Empty<int>());
                    metrics.StatementCount.Should().Be(0);
                });

        [TestMethod]
        public void VerifyMetrics_CsHtml() =>
            CreateBuilder(false, CsHtmlFileName)
                .VerifyUtilityAnalyzer<MetricsInfo>(messages =>
                        // There should be no metrics messages for the cshtml files.
                        messages.Select(x => Path.GetFileName(x.FilePath)).Should().BeEmpty());

        [TestMethod]
        public void VerifyMetrics_CSharp12() =>
            CreateBuilder(false, CSharp12FileName)
                .VerifyUtilityAnalyzer<MetricsInfo>(messages =>
                {
                    messages.Should().ContainSingle();
                    var metrics = messages.Single();
                    metrics.ClassCount.Should().Be(1); // no changes
                    metrics.CodeLine.Should().HaveCount(13);
                    metrics.CognitiveComplexity.Should().Be(1); // no changes
                    metrics.Complexity.Should().Be(3); // no changes
                    metrics.ExecutableLines.Should().HaveCount(3); // 5, 7, 9
                    metrics.FunctionCount.Should().Be(2); // no changes
                    metrics.NoSonarComment.Should().BeEmpty();
                    metrics.NonBlankComment.Should().ContainSingle();
                    metrics.StatementCount.Should().Be(3);
                });

#endif

        [TestMethod]
        public void Verify_NotRunForTestProject() =>
            CreateBuilder(true, AllMetricsFileName).VerifyUtilityAnalyzerProducesEmptyProtobuf();

        [TestMethod]
        [DataRow(AllMetricsFileName, true)]
        [DataRow("SomethingElse.cs", false)]
        public void Verify_UnchangedFiles(string unchangedFileName, bool expectedProtobufIsEmpty)
        {
            var builder = CreateBuilder(false, AllMetricsFileName)
                .WithAdditionalFilePath(AnalysisScaffolding.CreateSonarProjectConfigWithUnchangedFiles(TestContext, BasePath + unchangedFileName));
            if (expectedProtobufIsEmpty)
            {
                builder.VerifyUtilityAnalyzerProducesEmptyProtobuf();
            }
            else
            {
                builder.VerifyUtilityAnalyzer<TokenTypeInfo>(x => x.Should().NotBeEmpty());
            }
        }

        private VerifierBuilder CreateBuilder(bool isTestProject, params string[] fileNames)
        {
            var testRoot = BasePath + TestContext.TestName;
            return new VerifierBuilder()
                .AddAnalyzer(() => new TestMetricsAnalyzer(testRoot, isTestProject))
                .AddPaths(fileNames)
                .WithBasePath(BasePath)
                .WithOptions(LanguageOptions.CSharpLatest)
                .WithProtobufPath(@$"{testRoot}\metrics.pb");
        }

        // We need to set protected properties and this class exists just to enable the analyzer without bothering with additional files with parameters
        private sealed class TestMetricsAnalyzer : MetricsAnalyzer
        {
            private readonly string outPath;
            private readonly bool isTestProject;

            public TestMetricsAnalyzer(string outPath, bool isTestProject)
            {
                this.outPath = outPath;
                this.isTestProject = isTestProject;
            }

            protected override UtilityAnalyzerParameters ReadParameters(IAnalysisContext context) =>
                base.ReadParameters(context) with { IsAnalyzerEnabled = true, OutPath = outPath, IsTestProject = isTestProject };
        }
    }
}
