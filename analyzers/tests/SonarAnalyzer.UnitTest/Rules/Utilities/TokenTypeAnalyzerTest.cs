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
using System.Linq;
using SonarAnalyzer.Protobuf;
using SonarAnalyzer.Rules;
using SonarAnalyzer.UnitTest.Helpers;
using CS = SonarAnalyzer.Rules.CSharp;
using VB = SonarAnalyzer.Rules.VisualBasic;

namespace SonarAnalyzer.UnitTest.Rules
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
        [DataRow("Razor.razor")]
        [DataRow("Razor.cshtml")]
        public void Verify_NoMetricsAreComputedForRazorFiles(string fileName) =>
            CreateBuilder(ProjectType.Product, fileName)
                .WithAdditionalFilePath(AnalysisScaffolding.CreateSonarProjectConfig(TestContext, ProjectType.Product))
                .VerifyUtilityAnalyzer<TokenTypeInfo>(messages => messages.Select(x => Path.GetFileName(x.FilePath)).Should().BeEmpty());

        [TestMethod]
        public void Verify_Razor()
        {
            CreateBuilder(ProjectType.Product, "RazorTokens.razor", "RazorTokens.cs")
                .WithAdditionalFilePath(AnalysisScaffolding.CreateSonarProjectConfig(TestContext, ProjectType.Product))
                .WithConcurrentAnalysis(false)
                .VerifyUtilityAnalyzer<TokenTypeInfo>(tokens =>
                    {
                        var orderedTokens = tokens.OrderBy(x => x.FilePath, StringComparer.Ordinal).ToArray();
                        orderedTokens.Select(x => Path.GetFileName(x.FilePath)).Should().Contain("_Imports.razor", "RazorTokens.razor", "RazorTokens.cs");
                        orderedTokens[2].FilePath.Should().EndWith("_Imports.razor");
                        orderedTokens[1].FilePath.Should().EndWith("RazorTokens.razor");
                        orderedTokens[1].TokenInfo
                            .GroupBy(x => (x.TextRange.StartLine, x.TextRange.EndLine))
                            .ToDictionary(x => x.Key, x => string.Join(string.Empty, x.Select(y => TokenTypeAcronyms[y.TokenType])))
                            .Should()
                                // Razor directives including C# Tokens
                                .Contain((41, 41), "k")
                                .And.Contain((42, 42), "k")
                                .And.Contain((43, 43), "k")
                                .And.Contain((44, 44), "ktt")
                                .And.Contain((47, 47), "kk")
                                .And.Contain((51, 51), "kkn")
                                .And.Contain((52, 52), "kkn")
                                .And.Contain((53, 53), "kkkn")
                                .And.Contain((54, 54), "kkkn")
                                .And.Contain((55, 55), "ks")
                                .And.Contain((56, 56), "ks")
                                .And.Contain((57, 57), "kssns")
                                .And.Contain((58, 58), "ks")
                                .And.Contain((59, 59), "ksns")
                                .And.Contain((60, 60), "ksnss")
                                .And.Contain((62, 66), "c")
                                .And.Contain((66, 66), "kkkcsc")
                                .And.Contain((68, 68), "kktkkkkkts")
                                .And.Contain((70, 70), "c")
                                .And.Contain((71, 73), "c")
                                .And.Contain((74, 74), "c")
                                // Implicit Razor expressions including C# Tokens
                                .And.Contain((84, 84), "t")
                                .And.Contain((85, 85), "tn")
                                .And.Contain((86, 86), "kkk")
                                .And.Contain((87, 87), "t")
                                // Explicit Razor expressions including C# Tokens
                                .And.Contain((91, 91), "nn")
                                .And.Contain((92, 92), "nn")
                                .And.Contain((93, 93), "ttn")
                                // Razor code blocks
                                .And.Contain((98, 98), "kn")
                                .And.Contain((99, 99), "kn")
                                .And.Contain((100, 100), "kn")
                                // Single-level nesting with templated Razor delegates
                                .And.Contain((106, 106), "nnn")
                                .And.Contain((107, 107), "sssns")
                                .And.Contain((107, 109), "s")
                                .And.Contain((109, 109), "s")
                                .And.Contain((110, 110), "n")
                                .And.Contain((111, 111), "s")
                                .And.Contain((112, 112), "k")
                                .And.Contain((113, 113), "k")
                                .And.Contain((114, 114), "nc")
                                .And.Contain((115, 116), "cc")
                                .And.Contain((116, 116), "c")
                                // Multi-level nesting with templated Razor delegates
                                .And.Contain((123, 123), "nnn")
                                .And.Contain((124, 124), "sssnsssns")
                                .And.Contain((124, 124), "cc");
                    });
        }

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
            {
                // IdentifierTokenThreshold.cs has 4001 identifiers which exceeds current threshold of 4000. Due to this, the identifiers are not classified
                tokenInfo.Where(token => token.TokenType == TokenType.TypeName).Should().BeEmpty();
            });

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
                        messages.Should().HaveCount(1);
                        var info = messages.Single();
                        info.FilePath.Should().Be(Path.Combine(BasePath, fileName));
                        verifyTokenInfo(info.TokenInfo);
                    });

        private VerifierBuilder CreateBuilder(ProjectType projectType, params string[] fileNames)
        {
            var testRoot = BasePath + TestContext.TestName;
            var language = AnalyzerLanguage.FromPath(fileNames[0]);
            UtilityAnalyzerBase analyzer = language.LanguageName switch
            {
                LanguageNames.CSharp => new TestTokenTypeAnalyzer_CS(testRoot, projectType == ProjectType.Test),
                LanguageNames.VisualBasic => new TestTokenTypeAnalyzer_VB(testRoot, projectType == ProjectType.Test),
                _ => throw new UnexpectedLanguageException(language)
            };
            return new VerifierBuilder()
                .AddAnalyzer(() => analyzer)
                .AddPaths(fileNames)
                .WithBasePath(BasePath)
                .WithOptions(ParseOptionsHelper.Latest(language))
                .WithProtobufPath(@$"{testRoot}\token-type.pb");
        }

        // We need to set protected properties and this class exists just to enable the analyzer without bothering with additional files with parameters
        private sealed class TestTokenTypeAnalyzer_CS : CS.TokenTypeAnalyzer
        {
            public TestTokenTypeAnalyzer_CS(string outPath, bool isTestProject)
            {
                IsAnalyzerEnabled = true;
                OutPath = outPath;
                IsTestProject = isTestProject;
            }
        }

        private sealed class TestTokenTypeAnalyzer_VB : VB.TokenTypeAnalyzer
        {
            public TestTokenTypeAnalyzer_VB(string outPath, bool isTestProject)
            {
                IsAnalyzerEnabled = true;
                OutPath = outPath;
                IsTestProject = isTestProject;
            }
        }
    }
}
