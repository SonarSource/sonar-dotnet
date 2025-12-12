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
using SonarAnalyzer.Core.AnalysisContext;
using SonarAnalyzer.Core.Rules;
using SonarAnalyzer.Protobuf;
using CS = SonarAnalyzer.CSharp.Rules;
using VB = SonarAnalyzer.VisualBasic.Rules;

namespace SonarAnalyzer.Test.Rules;

[TestClass]
public partial class TokenTypeAnalyzerTest
{
    private const string BasePath = @"Utilities\TokenTypeAnalyzer\";

    public TestContext TestContext { get; set; }

    [TestMethod]
    [DataRow(ProjectType.Product)]
    [DataRow(ProjectType.Test)]
    public void Verify_MainTokens_CS(ProjectType projectType) =>
        Verify("Tokens.cs", projectType, x =>
            {
                x.Should().HaveCount(16);
                x.Where(x => x.TokenType == TokenType.Keyword).Should().HaveCount(10);
                x.Where(x => x.TokenType == TokenType.StringLiteral).Should().HaveCount(4);
                x.Should().ContainSingle(x => x.TokenType == TokenType.TypeName);
                x.Should().ContainSingle(x => x.TokenType == TokenType.NumericLiteral);
            });

    [TestMethod]
    [DataRow(ProjectType.Product)]
    [DataRow(ProjectType.Test)]
    public void Verify_MainTokens_CS_Latest(ProjectType projectType) =>
        Verify("Tokens.Latest.cs", projectType, x =>
            {
                x.Should().HaveCount(94);
                x.Where(x => x.TokenType == TokenType.Keyword).Should().HaveCount(58);
                x.Where(x => x.TokenType == TokenType.StringLiteral).Should().HaveCount(20);
                x.Where(x => x.TokenType == TokenType.NumericLiteral).Should().HaveCount(5);
                x.Where(x => x.TokenType == TokenType.TypeName).Should().HaveCount(10);
                x.Should().ContainSingle(x => x.TokenType == TokenType.Comment);
            });

    [TestMethod]
    [DataRow("Razor.razor")]
    [DataRow("Razor.cshtml")]
    public void Verify_NoMetricsAreComputedForRazorFiles(string fileName) =>
        CreateBuilder(ProjectType.Product, fileName)
            .VerifyUtilityAnalyzer<TokenTypeInfo>(x => x.Select(x => Path.GetFileName(x.FilePath)).Should().BeEmpty());

    [TestMethod]
    [DataRow(ProjectType.Product)]
    [DataRow(ProjectType.Test)]
    public void Verify_MainTokens_VB(ProjectType projectType) =>
        Verify("Tokens.vb", projectType, x =>
            {
                x.Should().HaveCount(19);
                x.Where(x => x.TokenType == TokenType.Keyword).Should().HaveCount(15);
                x.Where(x => x.TokenType == TokenType.StringLiteral).Should().HaveCount(2);
                x.Should().ContainSingle(x => x.TokenType == TokenType.TypeName);
                x.Should().ContainSingle(x => x.TokenType == TokenType.NumericLiteral);
            });

    [TestMethod]
    [DataRow(ProjectType.Product)]
    [DataRow(ProjectType.Test)]
    public void Verify_Identifiers_CS(ProjectType projectType) =>
        Verify("Identifiers.cs", projectType, x =>
            {
                x.Should().HaveCount(40);
                x.Count(x => x.TokenType == TokenType.Keyword).Should().Be(29);
                x.Count(x => x.TokenType == TokenType.TypeName).Should().Be(8);
                x.Count(x => x.TokenType == TokenType.NumericLiteral).Should().Be(1);
                x.Count(x => x.TokenType == TokenType.StringLiteral).Should().Be(2);
            });

    [TestMethod]
    [DataRow(ProjectType.Product)]
    [DataRow(ProjectType.Test)]
    public void Verify_Identifiers_VB(ProjectType projectType) =>
        Verify("Identifiers.vb", projectType, x =>
            {
                x.Should().HaveCount(63);
                x.Where(x => x.TokenType == TokenType.Keyword).Should().HaveCount(56);
                x.Where(x => x.TokenType == TokenType.TypeName).Should().HaveCount(6);
                x.Should().ContainSingle(x => x.TokenType == TokenType.NumericLiteral);
            });

    [TestMethod]
    [DataRow(ProjectType.Product)]
    [DataRow(ProjectType.Test)]
    public void Verify_Trivia_CS(ProjectType projectType) =>
        Verify("Trivia.cs", projectType, x =>
            {
                x.Should().HaveCount(5);
                x.Where(x => x.TokenType == TokenType.Comment).Should().HaveCount(4);
                x.Should().ContainSingle(x => x.TokenType == TokenType.Keyword);
            });

    [TestMethod]
    [DataRow(ProjectType.Product)]
    [DataRow(ProjectType.Test)]
    public void Verify_Trivia_VB(ProjectType projectType) =>
        Verify("Trivia.vb", projectType, x =>
            {
                x.Should().HaveCount(6);
                x.Should().ContainSingle(x => x.TokenType == TokenType.Comment);
                x.Where(x => x.TokenType == TokenType.Keyword).Should().HaveCount(4);
            });

    [TestMethod]
    public void Verify_IdentifierTokenThreshold() =>
        Verify("IdentifierTokenThreshold.cs", ProjectType.Product, x =>
            // IdentifierTokenThreshold.cs has 4001 identifiers which exceeds current threshold of 4000. Due to this, the identifiers are not classified
            x.Should().NotContain(token => token.TokenType == TokenType.TypeName));

    [TestMethod]
    [DataRow("Tokens.cs", true)]
    [DataRow("SomethingElse.cs", false)]
    public void Verify_UnchangedFiles(string unchangedFileName, bool expectedProtobufIsEmpty)
    {
        var builder = CreateBuilder(ProjectType.Product, "Tokens.cs")
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

    private void Verify(string fileName, ProjectType projectType, Action<IReadOnlyList<TokenTypeInfo.Types.TokenInfo>> verifyTokenInfo) =>
        CreateBuilder(projectType, fileName)
            .WithAdditionalFilePath(AnalysisScaffolding.CreateSonarProjectConfig(TestContext, projectType))
            .VerifyUtilityAnalyzer<TokenTypeInfo>(x =>
                {
                    x.Should().ContainSingle();
                    var info = x.Single();
                    info.FilePath.Should().Be(Path.Combine(BasePath, fileName));
                    verifyTokenInfo(info.TokenInfo);
                });

    private VerifierBuilder CreateBuilder(ProjectType projectType, string fileName)
    {
        var testRoot = BasePath + TestContext.TestName;
        var language = AnalyzerLanguage.FromPath(fileName);
        UtilityAnalyzerBase analyzer = language.LanguageName switch
        {
            LanguageNames.CSharp => new TestTokenTypeAnalyzer_CS(testRoot, projectType == ProjectType.Test),
            LanguageNames.VisualBasic => new TestTokenTypeAnalyzer_VB(testRoot, projectType == ProjectType.Test),
            _ => throw new UnexpectedLanguageException(language)
        };
        return new VerifierBuilder()
            .AddAnalyzer(() => analyzer)
            .AddPaths(fileName)
            .WithBasePath(BasePath)
            .WithOptions(LanguageOptions.Latest(language))
            .WithProtobufPath(@$"{testRoot}\token-type.pb");
    }

    // We need to set protected properties and this class exists just to enable the analyzer without bothering with additional files with parameters
    private sealed class TestTokenTypeAnalyzer_CS : CS.TokenTypeAnalyzer
    {
        private readonly string outPath;
        private readonly bool isTestProject;

        public TestTokenTypeAnalyzer_CS(string outPath, bool isTestProject)
        {
            this.outPath = outPath;
            this.isTestProject = isTestProject;
        }

        protected override UtilityAnalyzerParameters ReadParameters(IAnalysisContext context) =>
            base.ReadParameters(context) with { IsAnalyzerEnabled = true, OutPath = outPath, IsTestProject = isTestProject };
    }

    private sealed class TestTokenTypeAnalyzer_VB : VB.TokenTypeAnalyzer
    {
        private readonly string outPath;
        private readonly bool isTestProject;

        public TestTokenTypeAnalyzer_VB(string outPath, bool isTestProject)
        {
            this.outPath = outPath;
            this.isTestProject = isTestProject;
        }

        protected override UtilityAnalyzerParameters ReadParameters(IAnalysisContext context) =>
            base.ReadParameters(context) with { IsAnalyzerEnabled = true, OutPath = outPath, IsTestProject = isTestProject };
    }
}
