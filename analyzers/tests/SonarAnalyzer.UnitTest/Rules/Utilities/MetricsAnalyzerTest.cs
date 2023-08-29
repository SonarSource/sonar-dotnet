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
using SonarAnalyzer.Protobuf;
using SonarAnalyzer.Rules.CSharp;

namespace SonarAnalyzer.UnitTest.Rules
{
    [TestClass]
    public class MetricsAnalyzerTest
    {
        private const string BasePath = @"Utilities\MetricsAnalyzer\";

        public TestContext TestContext { get; set; }

        [DataTestMethod]
        public void VerifyMetrics() =>
            CreateBuilder(false)
                .WithAdditionalFilePath(AnalysisScaffolding.CreateSonarProjectConfig(TestContext, ProjectType.Product))
                .VerifyUtilityAnalyzer<MetricsInfo>(messages =>
                    {
                        messages.Should().HaveCount(1);
                        var metrics = messages.Single();
                        metrics.FilePath.Should().Be(Path.Combine(BasePath, "AllMetrics.cs"));
                        metrics.ClassCount.Should().Be(4);
                        metrics.CodeLine.Should().HaveCount(24);
                        metrics.CognitiveComplexity.Should().Be(1);
                        metrics.Complexity.Should().Be(2);
                        metrics.ExecutableLines.Should().HaveCount(5);
                        metrics.FunctionCount.Should().Be(1);
                        metrics.NoSonarComment.Should().HaveCount(1);
                        metrics.NonBlankComment.Should().HaveCount(1);
                        metrics.StatementCount.Should().Be(5);
                    });

        [TestMethod]
        public void Verify_NotRunForTestProject() =>
            CreateBuilder(true).VerifyUtilityAnalyzerProducesEmptyProtobuf();

        [DataTestMethod]
        [DataRow("AllMetrics.cs", true)]
        [DataRow("SomethingElse.cs", false)]
        public void Verify_UnchangedFiles(string unchangedFileName, bool expectedProtobufIsEmpty)
        {
            var builder = CreateBuilder(false).WithAdditionalFilePath(AnalysisScaffolding.CreateSonarProjectConfigWithUnchangedFiles(TestContext, BasePath + unchangedFileName));
            if (expectedProtobufIsEmpty)
            {
                builder.VerifyUtilityAnalyzerProducesEmptyProtobuf();
            }
            else
            {
                builder.VerifyUtilityAnalyzer<TokenTypeInfo>(x => x.Should().NotBeEmpty());
            }
        }

        private VerifierBuilder CreateBuilder(bool isTestProject)
        {
            var testRoot = BasePath + TestContext.TestName;
            return new VerifierBuilder()
                .AddAnalyzer(() => new TestMetricsAnalyzer(testRoot, isTestProject))
                .AddPaths("AllMetrics.cs")
                .WithBasePath(BasePath)
                .WithOptions(ParseOptionsHelper.CSharpLatest)
                .WithProtobufPath(@$"{testRoot}\metrics.pb");
        }

        // We need to set protected properties and this class exists just to enable the analyzer without bothering with additional files with parameters
        private sealed class TestMetricsAnalyzer : MetricsAnalyzer
        {
            public TestMetricsAnalyzer(string outPath, bool isTestProject)
            {
                IsAnalyzerEnabled = true;
                OutPath = outPath;
                IsTestProject = isTestProject;
            }
        }
    }
}
