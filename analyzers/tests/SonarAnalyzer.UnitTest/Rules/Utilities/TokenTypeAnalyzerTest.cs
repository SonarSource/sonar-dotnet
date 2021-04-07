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
        [DataRow(true)]
        [DataRow(false)]
        [TestCategory("Rule")]
        public void Verify_MainTokens(bool isTestProject) =>
            Verify("Tokens.cs", isTestProject, info =>
            {
                info.Should().HaveCount(15);
                info.Where(x => x.TokenType == TokenType.Keyword).Should().HaveCount(10);
                info.Where(x => x.TokenType == TokenType.StringLiteral).Should().HaveCount(3);
                info.Should().ContainSingle(x => x.TokenType == TokenType.TypeName);
                info.Should().ContainSingle(x => x.TokenType == TokenType.NumericLiteral);
            });

        [DataTestMethod]
        [DataRow(true)]
        [DataRow(false)]
        [TestCategory("Rule")]
        public void Verify_Identifiers(bool isTestProject) =>
            Verify("Identifiers.cs", isTestProject, info =>
            {
                info.Should().HaveCount(34);
                info.Where(x => x.TokenType == TokenType.Keyword).Should().HaveCount(26);
                info.Where(x => x.TokenType == TokenType.TypeName).Should().HaveCount(7);
                info.Should().ContainSingle(x => x.TokenType == TokenType.NumericLiteral);
            });

        [DataTestMethod]
        [DataRow(true)]
        [DataRow(false)]
        [TestCategory("Rule")]
        public void Verify_Trivia(bool isTestProject) =>
            Verify("Trivia.cs", isTestProject, info =>
            {
                info.Should().HaveCount(5);
                info.Where(x => x.TokenType == TokenType.Comment).Should().HaveCount(4);
                info.Should().ContainSingle(x => x.TokenType == TokenType.Keyword);
            });

        public void Verify(string fileName, bool isTestProject, Action<IReadOnlyList<TokenTypeInfo.Types.TokenInfo>> verifyTokenInfo)
        {
            var testRoot = Root + TestContext.TestName;
            Verifier.VerifyUtilityAnalyzer<TokenTypeInfo>(
                new[] { Root + fileName },
                new TestTokenTypeAnalyzer(testRoot, isTestProject),
                @$"{testRoot}\token-type.pb",
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
