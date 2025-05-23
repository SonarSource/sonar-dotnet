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
using SonarAnalyzer.AnalysisContext;
using SonarAnalyzer.Protobuf;
using SonarAnalyzer.Rules;
using CS = SonarAnalyzer.Rules.CSharp;
using VB = SonarAnalyzer.Rules.VisualBasic;

namespace SonarAnalyzer.Test.Rules;

[TestClass]
public class CopyPasteTokenAnalyzerTest
{
    private const string BasePath = @"Utilities\CopyPasteTokenAnalyzer\";

    public TestContext TestContext { get; set; }

    [TestMethod]
    public void Verify_Unique_CS() =>
        Verify("Unique.cs", x =>
        {
            x.Should().HaveCount(102);
            x.Count(token => token.TokenValue == "$str").Should().Be(9);
            x.Count(token => token.TokenValue == "$num").Should().Be(1);
            x.Count(token => token.TokenValue == "$char").Should().Be(2);
        });

#if NET

    [TestMethod]
    public void Verify_Unique_CSharp11() =>
        Verify("Unique.Csharp11.cs", x =>
        {
            x.Should().HaveCount(155);
            x.Count(token => token.TokenValue == "$str").Should().Be(16);
            x.Count(token => token.TokenValue == "$num").Should().Be(1);
            x.Count(token => token.TokenValue == "$char").Should().Be(2);
        });

    [TestMethod]
    public void Verify_Unique_CSharp12() =>
        Verify("Unique.Csharp12.cs", x =>
        {
            x.Should().HaveCount(81);
            x.Count(token => token.TokenValue == "$str").Should().Be(4);
            x.Count(token => token.TokenValue == "$num").Should().Be(4);
            x.Count(token => token.TokenValue == "$char").Should().Be(4);
        });

#endif

    [TestMethod]
    public void Verify_Unique_VB() =>
        Verify("Unique.vb", x =>
        {
            x.Should().HaveCount(88);
            x.Where(token => token.TokenValue == "$str").Should().HaveCount(3);
            x.Where(token => token.TokenValue == "$num").Should().HaveCount(7);
            x.Should().ContainSingle(token => token.TokenValue == "$char");
        });

    [TestMethod]
    public void Verify_Duplicated_CS() =>
        Verify("Duplicated.cs", x =>
        {
            x.Should().HaveCount(39);
            x.Where(token => token.TokenValue == "$num").Should().HaveCount(2);
        });

    [TestMethod]
    public void Verify_Duplicated_CS_GlobalUsings() =>
        CreateBuilder(ProjectType.Product, "Duplicated.CSharp10.cs")
            .VerifyUtilityAnalyzer<CopyPasteTokenInfo>(x =>
            {
                x.Should().ContainSingle();
                var info = x.Single();
                info.FilePath.Should().Be(Path.Combine(BasePath, "Duplicated.CSharp10.cs"));
                info.TokenInfo.Should().HaveCount(39);
                info.TokenInfo.Where(token => token.TokenValue == "$num").Should().HaveCount(2);
            });

    [TestMethod]
    public void Verify_DuplicatedDifferentLiterals_CS() =>
        Verify("DuplicatedDifferentLiterals.cs", x =>
        {
            x.Should().HaveCount(39);
            x.Where(token => token.TokenValue == "$num").Should().HaveCount(2);
        });

    [TestMethod]
    public void Verify_NotRunForTestProject_CS() =>
        CreateBuilder(ProjectType.Test, "DuplicatedDifferentLiterals.cs").VerifyUtilityAnalyzerProducesEmptyProtobuf();

    [DataTestMethod]
    [DataRow("Unique.cs")]
    [DataRow("SomethingElse.cs")]
    public void Verify_UnchangedFiles(string unchangedFileName) =>
        CreateBuilder(ProjectType.Product, "Unique.cs")
            .WithAdditionalFilePath(AnalysisScaffolding.CreateSonarProjectConfigWithUnchangedFiles(TestContext, BasePath + unchangedFileName))
            .VerifyUtilityAnalyzer<TokenTypeInfo>(x => x.Should().NotBeEmpty());

    private void Verify(string fileName, Action<IReadOnlyList<CopyPasteTokenInfo.Types.TokenInfo>> verifyTokenInfo) =>
        CreateBuilder(ProjectType.Product, fileName)
            .VerifyUtilityAnalyzer<CopyPasteTokenInfo>(x =>
                {
                    x.Should().ContainSingle();
                    var info = x.Single();
                    info.FilePath.Should().Be(Path.Combine(BasePath, fileName));
                    verifyTokenInfo(info.TokenInfo);
                });

#if NET

    [DataTestMethod]
    [DataRow("Razor.razor")]
    [DataRow("Razor.cshtml")]
    public void Verify_NoMetricsAreComputedForRazorFiles(string fileName) =>
        CreateBuilder(ProjectType.Product, fileName)
            .VerifyUtilityAnalyzer<CopyPasteTokenInfo>(x => x.Select(token => Path.GetFileName(token.FilePath)).Should().BeEmpty());

#endif

    private VerifierBuilder CreateBuilder(ProjectType projectType, string fileName)
    {
        var testRoot = BasePath + TestContext.TestName;
        var language = AnalyzerLanguage.FromPath(fileName);
        UtilityAnalyzerBase analyzer = language.LanguageName switch
        {
            LanguageNames.CSharp => new TestCopyPasteTokenAnalyzer_CS(testRoot, projectType == ProjectType.Test),
            LanguageNames.VisualBasic => new TestCopyPasteTokenAnalyzer_VB(testRoot, projectType == ProjectType.Test),
            _ => throw new UnexpectedLanguageException(language)
        };
        return new VerifierBuilder()
            .AddAnalyzer(() => analyzer)
            .AddPaths(fileName)
            .WithBasePath(BasePath)
            .WithOptions(ParseOptionsHelper.Latest(language))
            .WithProtobufPath(@$"{testRoot}\token-cpd.pb");
    }

    // We need to set protected properties and this class exists just to enable the analyzer without bothering with additional files with parameters
    private sealed class TestCopyPasteTokenAnalyzer_CS : CS.CopyPasteTokenAnalyzer
    {
        private readonly string outPath;
        private readonly bool isTestProject;

        public TestCopyPasteTokenAnalyzer_CS(string outPath, bool isTestProject)
        {
            this.outPath = outPath;
            this.isTestProject = isTestProject;
        }

        protected override UtilityAnalyzerParameters ReadParameters<T>(SonarAnalysisContextBase<T> context) =>
            base.ReadParameters(context) with { IsAnalyzerEnabled = true, OutPath = outPath, IsTestProject = isTestProject };
    }

    private sealed class TestCopyPasteTokenAnalyzer_VB : VB.CopyPasteTokenAnalyzer
    {
        private readonly string outPath;
        private readonly bool isTestProject;

        public TestCopyPasteTokenAnalyzer_VB(string outPath, bool isTestProject)
        {
            this.outPath = outPath;
            this.isTestProject = isTestProject;
        }

        protected override UtilityAnalyzerParameters ReadParameters<T>(SonarAnalysisContextBase<T> context) =>
            base.ReadParameters(context) with { IsAnalyzerEnabled = true, OutPath = outPath, IsTestProject = isTestProject };
    }
}
