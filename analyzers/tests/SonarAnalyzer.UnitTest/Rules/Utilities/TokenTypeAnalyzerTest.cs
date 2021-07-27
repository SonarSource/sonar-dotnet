/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2021 SonarSource SA
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
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SonarAnalyzer.Helpers;
using SonarAnalyzer.Protobuf;
using SonarAnalyzer.Rules.CSharp;
using SonarAnalyzer.UnitTest.TestFramework;

namespace SonarAnalyzer.UnitTest.Rules
{
    [TestClass]
    public class TokenTypeAnalyzerTest
    {
        private const string Root = @"TestCases\Utilities\TokenTypeAnalyzer\";

        public TestContext TestContext { get; set; } // Set automatically by MsTest

        [DataTestMethod]
        [DataRow(ProjectType.Product)]
        [DataRow(ProjectType.Test)]
        [TestCategory("Rule")]
        public void Verify_MainTokens(ProjectType projectType) =>
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
        [TestCategory("Rule")]
        public void Verify_Identifiers(ProjectType projectType) =>
            Verify("Identifiers.cs", projectType, info =>
            {
                info.Should().HaveCount(34);
                info.Where(x => x.TokenType == TokenType.Keyword).Should().HaveCount(26);
                info.Where(x => x.TokenType == TokenType.TypeName).Should().HaveCount(7);
                info.Should().ContainSingle(x => x.TokenType == TokenType.NumericLiteral);
            });

        [DataTestMethod]
        [DataRow(ProjectType.Product)]
        [DataRow(ProjectType.Test)]
        [TestCategory("Rule")]
        public void Verify_Trivia(ProjectType projectType) =>
            Verify("Trivia.cs", projectType, info =>
            {
                info.Should().HaveCount(5);
                info.Where(x => x.TokenType == TokenType.Comment).Should().HaveCount(4);
                info.Should().ContainSingle(x => x.TokenType == TokenType.Keyword);
            });

        [TestMethod]
        [TestCategory("Rule")]
        public void Verify_IdentifierTokenThreshold() =>
            Verify("IdentifierTokenThreshold.cs", ProjectType.Product, tokenInfo =>
            {
                // IdentifierTokenThreshold.cs has 4001 identifiers which exceeds current threshold of 4000. Due to this, the identifiers are not classified
                tokenInfo.Where(token => token.TokenType == TokenType.TypeName).Should().BeEmpty();
            });

        private void Verify(string fileName, ProjectType projectType, Action<IReadOnlyList<TokenTypeInfo.Types.TokenInfo>> verifyTokenInfo)
        {
            var testRoot = Root + TestContext.TestName;
            Verifier.VerifyNonConcurrentUtilityAnalyzer<TokenTypeInfo>(
                new[] { Root + fileName },
                new TestTokenTypeAnalyzer(testRoot, projectType == ProjectType.Test),
                @$"{testRoot}\token-type.pb",
                TestHelper.CreateSonarProjectConfig(testRoot, projectType),
                messages =>
                {
                    messages.Should().HaveCount(1);
                    var info = messages.Single();
                    info.FilePath.Should().Be(fileName);
                    verifyTokenInfo(info.TokenInfo);
                });
        }

        // We need to set protected properties and this class exists just to enable the analyzer without bothering with additional files with parameters
        private class TestTokenTypeAnalyzer : TokenTypeAnalyzer
        {
            public TestTokenTypeAnalyzer(string outPath, bool isTestProject)
            {
                IsAnalyzerEnabled = true;
                OutPath = outPath;
                IsTestProject = isTestProject;
            }
        }
    }
}
