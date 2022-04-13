/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2022 SonarSource SA
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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using FluentAssertions;
using Microsoft.CodeAnalysis;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SonarAnalyzer.Common;
using SonarAnalyzer.Helpers;
using SonarAnalyzer.Protobuf;
using SonarAnalyzer.Rules;
using SonarAnalyzer.UnitTest.TestFramework;
using CS = SonarAnalyzer.Rules.CSharp;
using VB = SonarAnalyzer.Rules.VisualBasic;

namespace SonarAnalyzer.UnitTest.Rules
{
    [TestClass]
    public class TokenTypeAnalyzerTest
    {
        private const string BasePath = @"Utilities\TokenTypeAnalyzer\";

        [DataTestMethod]
        [DataRow(ProjectType.Product)]
        [DataRow(ProjectType.Test)]
        public void Verify_MainTokens_CS(ProjectType projectType) =>
            Verify("Tokens.cs", projectType, info =>
            {
                info.Should().HaveCount(15);
                info.Where(x => x.TokenType == TokenType.Keyword).Should().HaveCount(10);
                info.Where(x => x.TokenType == TokenType.StringLiteral).Should().HaveCount(3);
                info.Should().ContainSingle(x => x.TokenType == TokenType.TypeName);
                info.Should().ContainSingle(x => x.TokenType == TokenType.NumericLiteral);
            });

        [DataTestMethod]
        [DataRow(ProjectType.Product)]
        [DataRow(ProjectType.Test)]
        public void Verify_MainTokens_VB(ProjectType projectType) =>
            Verify("Tokens.vb", projectType, info =>
            {
                info.Should().HaveCount(18);
                info.Where(x => x.TokenType == TokenType.Keyword).Should().HaveCount(15);
                info.Should().ContainSingle(x => x.TokenType == TokenType.StringLiteral);
                info.Should().ContainSingle(x => x.TokenType == TokenType.TypeName);
                info.Should().ContainSingle(x => x.TokenType == TokenType.NumericLiteral);
            });

        [DataTestMethod]
        [DataRow(ProjectType.Product)]
        [DataRow(ProjectType.Test)]
        public void Verify_Identifiers_CS(ProjectType projectType) =>
            Verify("Identifiers.cs", projectType, info =>
            {
                info.Should().HaveCount(46);
                info.Count(x => x.TokenType == TokenType.Keyword).Should().Be(33);
                info.Count(x => x.TokenType == TokenType.TypeName).Should().Be(8);
                info.Count(x => x.TokenType == TokenType.NumericLiteral).Should().Be(3);
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

        private static void Verify(string fileName, ProjectType projectType, Action<IReadOnlyList<TokenTypeInfo.Types.TokenInfo>> verifyTokenInfo, [CallerMemberName] string testName = "")
        {
            var testRoot = BasePath + testName;
            var language = AnalyzerLanguage.FromPath(fileName);
            UtilityAnalyzerBase analyzer = language.LanguageName switch
            {
                LanguageNames.CSharp => new TestTokenTypeAnalyzer_CS(testRoot, projectType == ProjectType.Test),
                LanguageNames.VisualBasic => new TestTokenTypeAnalyzer_VB(testRoot, projectType == ProjectType.Test),
                _ => throw new UnexpectedLanguageException(language)
            };

            new VerifierBuilder()
                .AddAnalyzer(() => analyzer)
                .AddPaths(fileName)
                .WithBasePath(BasePath)
                .WithOptions(ParseOptionsHelper.Latest(language))
                .WithSonarProjectConfigPath(TestHelper.CreateSonarProjectConfig(testRoot, projectType))
                .WithProtobufPath(@$"{testRoot}\token-type.pb")
                .VerifyUtilityAnalyzer<TokenTypeInfo>(messages =>
                    {
                        messages.Should().HaveCount(1);
                        var info = messages.Single();
                        info.FilePath.Should().Be(fileName);
                        verifyTokenInfo(info.TokenInfo);
                    });
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
