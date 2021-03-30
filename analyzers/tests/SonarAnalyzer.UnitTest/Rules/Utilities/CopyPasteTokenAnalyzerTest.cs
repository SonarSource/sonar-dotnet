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
    public class CopyPasteTokenAnalyzerTest
    {
        private const string Root = @"TestCases\Utilities\CopyPasteTokenAnalyzer\";

        public TestContext TestContext { get; set; } // Set automatically by MsTest

        [TestMethod]
        [TestCategory("Rule")]
        public void Verify_Unique() =>
            Verify("Unique.cs", info =>
            {
                info.Should().HaveCount(54);
                info.Where(x => x.TokenValue == "$str").Should().HaveCount(2); // ToDo: Expect 4 of them https://github.com/SonarSource/sonar-dotnet/issues/4205
                info.Should().ContainSingle(x => x.TokenValue == "$num");
            });

        [TestMethod]
        [TestCategory("Rule")]
        public void Verify_Duplicated() =>
            Verify("Duplicated.cs", info =>
            {
                info.Should().HaveCount(39);
                info.Where(x => x.TokenValue == "$num").Should().HaveCount(2);
            });

        [TestMethod]
        [TestCategory("Rule")]
        public void Verify_DuplicatedDifferentLiterals() =>
            Verify("DuplicatedDifferentLiterals.cs", info =>
            {
                info.Should().HaveCount(39);
                info.Where(x => x.TokenValue == "$num").Should().HaveCount(2);
            });

        public void Verify(string fileName, Action<IEnumerable<CopyPasteTokenInfo.Types.TokenInfo>> verifyTokenInfo)
        {
            var testRoot = Root + TestContext.TestName;
            Verifier.VerifyUtilityAnalyzer<CopyPasteTokenInfo>(
                new[] { Root + fileName },
                new TestCopyPasteTokenAnalyzer(testRoot),
                @$"{testRoot}\token-cpd.pb",
                messages =>
                {
                    messages.Should().HaveCount(1);
                    var info = messages.Single();
                    info.FilePath.Should().Be(fileName);
                    verifyTokenInfo(info.TokenInfo);
                });
        }

        // We need to set protected properties and this class exists just to enable the analyzer without bothering with additional files with parameters
        private class TestCopyPasteTokenAnalyzer : CopyPasteTokenAnalyzer
        {
            public TestCopyPasteTokenAnalyzer(string outPath)
            {
                IsAnalyzerEnabled = true;
                OutPath = outPath;
            }
        }
    }
}
