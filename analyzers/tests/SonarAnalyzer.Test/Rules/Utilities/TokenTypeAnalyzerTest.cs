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
using SonarAnalyzer.Common;
using SonarAnalyzer.Protobuf;
using SonarAnalyzer.Rules;
using CS = SonarAnalyzer.Rules.CSharp;
using VB = SonarAnalyzer.Rules.VisualBasic;

namespace SonarAnalyzer.Test.Rules
{
    [TestClass]
    public partial class TokenTypeAnalyzerTest
    {
        private const string BasePath = @"Utilities\TokenTypeAnalyzer\";

        public TestContext TestContext { get; set; }

        [DataTestMethod]
        [DataRow(ProjectType.Product)]
        [DataRow(ProjectType.Test)]
        public void Verify_MainTokens_CS(ProjectType projectType) =>
            Verify("Tokens.cs", projectType, info =>
            {
                info.Should().HaveCount(16);
                info.Where(x => x.TokenType == TokenType.Keyword).Should().HaveCount(10);
                info.Where(x => x.TokenType == TokenType.StringLiteral).Should().HaveCount(4);
                info.Should().ContainSingle(x => x.TokenType == TokenType.TypeName);
                info.Should().ContainSingle(x => x.TokenType == TokenType.NumericLiteral);
            });

#if NET

        [DataTestMethod]
        [DataRow(ProjectType.Product)]
        [DataRow(ProjectType.Test)]
        public void Verify_MainTokens_CSharp11(ProjectType projectType) =>
            Verify("Tokens.Csharp11.cs", projectType, info =>
            {
                info.Should().HaveCount(42);
                info.Where(x => x.TokenType == TokenType.Keyword).Should().HaveCount(25);
                info.Where(x => x.TokenType == TokenType.StringLiteral).Should().HaveCount(15);
                info.Should().ContainSingle(x => x.TokenType == TokenType.TypeName);
                info.Should().ContainSingle(x => x.TokenType == TokenType.NumericLiteral);
            });

        [DataTestMethod]
        [DataRow(ProjectType.Product)]
        [DataRow(ProjectType.Test)]
        public void Verify_MainTokens_CSharp12(ProjectType projectType) =>
            Verify("Tokens.Csharp12.cs", projectType, info =>
            {
                info.Should().HaveCount(34);
                info.Where(x => x.TokenType == TokenType.Keyword).Should().HaveCount(17);
                info.Where(x => x.TokenType == TokenType.StringLiteral).Should().HaveCount(5);
                info.Where(x => x.TokenType == TokenType.NumericLiteral).Should().HaveCount(4);
                info.Where(x => x.TokenType == TokenType.TypeName).Should().HaveCount(7);
                info.Should().ContainSingle(x => x.TokenType == TokenType.Comment);
            });

        [DataTestMethod]
        [DataRow("Razor.razor")]
        [DataRow("Razor.cshtml")]
        public void Verify_NoMetricsAreComputedForRazorFiles(string fileName) =>
            CreateBuilder(ProjectType.Product, fileName)
                .VerifyUtilityAnalyzer<TokenTypeInfo>(messages => messages.Select(x => Path.GetFileName(x.FilePath)).Should().BeEmpty());

#endif

        [DataTestMethod]
        [DataRow(ProjectType.Product)]
        [DataRow(ProjectType.Test)] 
        public void Verify_MainTokens_VB(ProjectType projectType) =>
            Verify("Tokens.vb", projectType, info =>
            {
                info.Should().HaveCount(19);
                info.Where(x => x.TokenType == TokenType.Keyword).Should().HaveCount(15);
                info.Where(x => x.TokenType == TokenType.StringLiteral).Should().HaveCount(2);
                info.Should().ContainSingle(x => x.TokenType == TokenType.TypeName);
                info.Should().ContainSingle(x => x.TokenType == TokenType.NumericLiteral);
            });

        [DataTestMethod]
        [DataRow(ProjectType.Product)]
        [DataRow(ProjectType.Test)]
        public void Verify_Identifiers_CS(ProjectType projectType) =>
            Verify("Identifiers.cs", projectType, info =>
            {
                info.Should().HaveCount(40);
                info.Count(x => x.TokenType == TokenType.Keyword).Should().Be(29);
                info.Count(x => x.TokenType == TokenType.TypeName).Should().Be(8);
                info.Count(x => x.TokenType == TokenType.NumericLiteral).Should().Be(1);
                info.Count(x => x.TokenType == TokenType.StringLiteral).Should().Be(2);
            });

        [DataTestMethod]
        [DataRow(ProjectType.Product)]
        [DataRow(ProjectType.Test)]
        public void Verify_Identifiers_VB(ProjectType projectType) =>
            Verify("Identifiers.vb", projectType, info =>
            {
                info.Should().HaveCount(63);
                info.Where(x => x.TokenType == TokenType.Keyword).Should().HaveCount(56);
                info.Where(x => x.TokenType == TokenType.TypeName).Should().HaveCount(6);
                info.Should().ContainSingle(x => x.TokenType == TokenType.NumericLiteral);
            });

        [DataTestMethod]
        [DataRow(ProjectType.Product)]
        [DataRow(ProjectType.Test)]
        public void Verify_Trivia_CS(ProjectType projectType) =>
            Verify("Trivia.cs", projectType, info =>
            {
                info.Should().HaveCount(5);
                info.Where(x => x.TokenType == TokenType.Comment).Should().HaveCount(4);
                info.Should().ContainSingle(x => x.TokenType == TokenType.Keyword);
            });

        [DataTestMethod]
        [DataRow(ProjectType.Product)]
        [DataRow(ProjectType.Test)]
        public void Verify_Trivia_VB(ProjectType projectType) =>
            Verify("Trivia.vb", projectType, info =>
            {
                info.Should().HaveCount(6);
                info.Should().ContainSingle(x => x.TokenType == TokenType.Comment);
                info.Where(x => x.TokenType == TokenType.Keyword).Should().HaveCount(4);
            });

        [TestMethod]
        public void Verify_IdentifierTokenThreshold() =>
            Verify("IdentifierTokenThreshold.cs", ProjectType.Product, tokenInfo =>
                // IdentifierTokenThreshold.cs has 4001 identifiers which exceeds current threshold of 4000. Due to this, the identifiers are not classified
                tokenInfo.Should().NotContain(token => token.TokenType == TokenType.TypeName));

        [DataTestMethod]
        [DataRow("Tokens.cs", true)]
        [DataRow("SomethingElse.cs", false)]
        public void Verify_UnchangedFiles(string unchangedFileName, bool expectedProtobufIsEmpty)
        {
            var builder = CreateBuilder(ProjectType.Product, "Tokens.cs").WithAdditionalFilePath(AnalysisScaffolding.CreateSonarProjectConfigWithUnchangedFiles(TestContext, BasePath + unchangedFileName));
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
                .VerifyUtilityAnalyzer<TokenTypeInfo>(messages =>
                    {
                        messages.Should().ContainSingle();
                        var info = messages.Single();
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
                .WithOptions(ParseOptionsHelper.Latest(language))
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

            protected override UtilityAnalyzerParameters ReadParameters<T>(SonarAnalysisContextBase<T> context) =>
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

            protected override UtilityAnalyzerParameters ReadParameters<T>(SonarAnalysisContextBase<T> context) =>
                base.ReadParameters(context) with { IsAnalyzerEnabled = true, OutPath = outPath, IsTestProject = isTestProject };
        }
    }
}
