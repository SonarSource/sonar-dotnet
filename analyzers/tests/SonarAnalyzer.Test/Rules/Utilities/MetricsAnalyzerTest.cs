/*
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
using SonarAnalyzer.AnalysisContext;
using SonarAnalyzer.Protobuf;
using SonarAnalyzer.Rules;
using SonarAnalyzer.Rules.CSharp;

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

        public TestContext TestContext { get; set; }

        [DataTestMethod]
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
                    orderedMessages.Select(x => Path.GetFileName(x.FilePath)).Should().BeEquivalentTo("_Imports.razor", RazorFileName, "Component.razor");

                    var metrics = messages.Single(x => x.FilePath.EndsWith(RazorFileName));

                    metrics.ClassCount.Should().Be(1);
                    metrics.CodeLine.Should().BeEquivalentTo(new[] { 3, 5, 8, 10, 13, 15, 16, 17, 19, 22, 23, 24, 26, 28, 29, 32, 33, 34, 36, 37, 39, 40, 43 });
                    metrics.CognitiveComplexity.Should().Be(3);
                    metrics.Complexity.Should().Be(4);
                    metrics.ExecutableLines.Should().BeEquivalentTo(new[] { 3, 5, 13, 15, 17, 24, 28, 29, 32, 36, 39, 43 });
                    metrics.FunctionCount.Should().Be(1);
                    metrics.NoSonarComment.Should().BeEmpty();
                    metrics.NonBlankComment.Should().BeEquivalentTo(new[] { 7, 8, 10, 15, 21, 22, 23, 28, 29, 32, 33, 36, 37, 38 });
                    metrics.StatementCount.Should().Be(13);
                });

        [TestMethod]
        public void VerifyMetrics_CsHtml() =>
            CreateBuilder(false, CsHtmlFileName)
                .VerifyUtilityAnalyzer<MetricsInfo>(messages =>
                        // There should be no metrics messages for the cshtml files.
                        messages.Select(x => Path.GetFileName(x.FilePath)).Should().BeEquivalentTo("_Imports.razor"));

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

        [DataTestMethod]
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
                .WithOptions(ParseOptionsHelper.CSharpLatest)
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

            protected override UtilityAnalyzerParameters ReadParameters<T>(SonarAnalysisContextBase<T> context) =>
                base.ReadParameters(context) with { IsAnalyzerEnabled = true, OutPath = outPath, IsTestProject = isTestProject };
        }
    }
}
