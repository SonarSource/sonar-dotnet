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

using System.Linq;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SonarAnalyzer.UnitTest.MetadataReferences;
using SonarAnalyzer.UnitTest.TestFramework;
using CS = SonarAnalyzer.Rules.CSharp;
using VB = SonarAnalyzer.Rules.VisualBasic;

namespace SonarAnalyzer.UnitTest.Rules
{
    [TestClass]
    public class ClassAndMethodNameTest
    {
        [TestMethod]
        [TestCategory("Rule")]
        public void ClassName_CS() =>
            Verifier.VerifyAnalyzer(
                new[]
                {
                    @"TestCases\ClassName.cs",
                    @"TestCases\ClassName.Partial.cs",
                }, new CS.ClassAndMethodName(),
#if NETFRAMEWORK
                additionalReferences: NuGetMetadataReference.NETStandardV2_1_0,
#endif
                options: ParseOptionsHelper.FromCSharp8);

        [TestMethod]
        [TestCategory("Rule")]
        public void ClassName_InTestProject_CS() =>
            Verifier.VerifyAnalyzerInTest(@"TestCases\ClassName.Tests.cs", new CS.ClassAndMethodName(), ParseOptionsHelper.FromCSharp8);

#if NET
        [TestMethod]
        [TestCategory("Rule")]
        public void ClassName_TopLevelStatement_CS() =>
            Verifier.VerifyAnalyzerFromCSharp9Console(@"TestCases\ClassName.TopLevelStatement.cs", new CS.ClassAndMethodName());

        [TestMethod]
        [TestCategory("Rule")]
        public void ClassName_TopLevelStatement_InTestProject_CS() =>
            Verifier.VerifyAnalyzerFromCSharp9ConsoleInTest(@"TestCases\ClassName.TopLevelStatement.Test.cs", new CS.ClassAndMethodName());

        [TestMethod]
        [TestCategory("Rule")]
        public void RecordName_CS() =>
            Verifier.VerifyAnalyzerFromCSharp9Library(@"TestCases\RecordName.cs", new CS.ClassAndMethodName());

        [TestMethod]
        [TestCategory("Rule")]
        public void RecordName_InTestProject_CS() =>
            Verifier.VerifyNoIssueReportedFromCSharp9InTest(@"TestCases\RecordName.cs", new CS.ClassAndMethodName());
#endif

        [TestMethod]
        [TestCategory("Rule")]
        public void ClassName_VB() =>
            Verifier.VerifyAnalyzer(@"TestCases\ClassName.vb", new VB.ClassName());

        [TestMethod]
        [TestCategory("Rule")]
        public void MethodName_CS() =>
            Verifier.VerifyAnalyzer(
                new[]
                {
                    @"TestCases\MethodName.cs",
                    @"TestCases\MethodName.Partial.cs",
                },
                new CS.ClassAndMethodName(),
                ParseOptionsHelper.FromCSharp8);

        // ToDo fix conflict with PR #4179
        [TestMethod]
        [TestCategory("Rule")]
        public void MethodName_InTestProject_CS() =>
            Verifier.VerifyAnalyzerInTest(
                new[]
                {
                    @"TestCases\MethodName.cs",
                    @"TestCases\MethodName.Partial.cs",
                },
                new CS.ClassAndMethodName(),
                ParseOptionsHelper.FromCSharp8);

#if NET
        [TestMethod]
        [TestCategory("Rule")]
        public void MethodName_CSharp9() =>
            Verifier.VerifyAnalyzerFromCSharp9Library(@"TestCases\MethodName.CSharp9.cs", new CS.ClassAndMethodName());

        [TestMethod]
        [TestCategory("Rule")]
        public void MethodName_InTestProject_CSharp9() =>
            Verifier.VerifyAnalyzerFromCSharp9LibraryInTest(@"TestCases\MethodName.CSharp9.cs", new CS.ClassAndMethodName());

#endif

        [TestMethod]
        public void TestSplitToParts() =>
            new[]
            {
                ("foo", new[] { "foo" }),
                ("Foo", new[] { "Foo" }),
                ("FFF", new[] { "FFF" }),
                ("FfF", new[] { "Ff", "F" }),
                ("Ff9F", new[] { "Ff", "9", "F" }),
                ("你好", new[] { "你", "好" }),
                ("FFf", new[] { "F", "Ff" }),
                ("", new string[0]),
                ("FF9d", new[] { "FF", "9", "d" }),
                ("y2x5__w7", new[] { "y", "2", "x", "5", "_", "_", "w", "7" }),
                ("3%c#account", new[] { "3", "%", "c", "#", "account" }),
            }
            .Select(x =>
            (
                actual: CS.ClassAndMethodName.SplitToParts(x.Item1).ToArray(),
                expected: x.Item2
            ))
            .ToList()
            .ForEach(x => x.actual.Should().Equal(x.expected));
    }
}
