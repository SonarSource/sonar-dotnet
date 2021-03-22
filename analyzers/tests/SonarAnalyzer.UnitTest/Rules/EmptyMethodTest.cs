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

#if NET
using Microsoft.CodeAnalysis;
#endif
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SonarAnalyzer.Helpers;
using SonarAnalyzer.UnitTest.MetadataReferences;
using SonarAnalyzer.UnitTest.TestFramework;
using CS = SonarAnalyzer.Rules.CSharp;
using VB = SonarAnalyzer.Rules.VisualBasic;

namespace SonarAnalyzer.UnitTest.Rules
{
    [TestClass]
    public class EmptyMethodTest
    {
        [TestMethod]
        [TestCategory("Rule")]
        public void EmptyMethod() =>
            Verifier.VerifyAnalyzer(@"TestCases\EmptyMethod.cs",
                new CS.EmptyMethod(),
#if NETFRAMEWORK
                ParseOptionsHelper.FromCSharp8,
                NuGetMetadataReference.NETStandardV2_1_0);
#else
                ParseOptionsHelper.FromCSharp8);
#endif

#if NET
        [TestMethod]
        [TestCategory("Rule")]
        public void EmptyMethod_CSharp9() =>
            Verifier.VerifyAnalyzerFromCSharp9Console(@"TestCases\EmptyMethod.CSharp9.cs", new CS.EmptyMethod());
#endif

            [TestMethod]
        [TestCategory("CodeFix")]
        public void EmptyMethod_CodeFix_Throw() =>
            Verifier.VerifyCodeFix(
                @"TestCases\EmptyMethod.cs",
                @"TestCases\EmptyMethod.Throw.Fixed.cs",
                new CS.EmptyMethod(),
                new CS.EmptyMethodCodeFixProvider(),
                CS.EmptyMethodCodeFixProvider.TitleThrow);

        [TestMethod]
        [TestCategory("CodeFix")]
        public void EmptyMethod_CodeFix_Comment() =>
            Verifier.VerifyCodeFix(
                @"TestCases\EmptyMethod.cs",
                @"TestCases\EmptyMethod.Comment.Fixed.cs",
                new CS.EmptyMethod(),
                new CS.EmptyMethodCodeFixProvider(),
                CS.EmptyMethodCodeFixProvider.TitleComment);

        [TestMethod]
        [TestCategory("Rule")]
        public void EmptyMethod_VB() =>
            Verifier.VerifyAnalyzer(@"TestCases\EmptyMethod.vb", new VB.EmptyMethod());

        [DataTestMethod]
        [DataRow(ProjectType.Product)]
        [DataRow(ProjectType.Unknown)]
        public void EmptyMethod_WithVirtualOverride_RaisesIssueForMainAndUnknownProject_CS(ProjectType projectType) =>
            Verifier.VerifyAnalyzer(@"TestCases\EmptyMethod.OverrideVirtual.cs",
                new CS.EmptyMethod(),
                options: ParseOptionsHelper.FromCSharp8,
                additionalReferences: null,
                sonarProjectConfigPath: TestHelper.CreateSonarProjectConfig(
                    Directory.CreateDirectory(@"TestCases\EmptyMethod_WithVirtualOverride_RaisesIssueForMainAndUnknownProject_CS").FullName,
                    projectType));

        [TestMethod]
        public void EmptyMethod_WithVirtualOverride_DoesNotRaiseIssuesForTestProject_CS() =>
            Verifier.VerifyNoIssueReported(@"TestCases\EmptyMethod.OverrideVirtual.cs",
                new CS.EmptyMethod(),
                ParseOptionsHelper.FromCSharp8,
                NuGetMetadataReference.NETStandardV2_1_0,
                sonarProjectConfigPath: TestHelper.CreateSonarProjectConfig(
                    Directory.CreateDirectory(@"TestCases\EmptyMethod_WithVirtualOverride_DoesNotRaiseIssuesForTestProject_CS").FullName,
                    ProjectType.Test));

        [DataTestMethod]
        [DataRow(ProjectType.Product)]
        [DataRow(ProjectType.Unknown)]
        public void EmptyMethod_WithVirtualOverride_RaisesIssueForMainAndUnknownProject_VB(ProjectType projectType) =>
            Verifier.VerifyAnalyzer(@"TestCases\EmptyMethod.OverrideVirtual.vb",
                new VB.EmptyMethod(),
                options: ParseOptionsHelper.FromCSharp8,
                additionalReferences: null,
                sonarProjectConfigPath: TestHelper.CreateSonarProjectConfig(
                    Directory.CreateDirectory(@"TestCases\EmptyMethod_WithVirtualOverride_RaisesIssueForMainAndUnknownProject_VB").FullName,
                    projectType));

        [TestMethod]
        public void EmptyMethod_WithVirtualOverride_DoesNotRaiseIssuesForTestProject_VB() =>
            Verifier.VerifyNoIssueReported(@"TestCases\EmptyMethod.OverrideVirtual.vb",
                new VB.EmptyMethod(),
                sonarProjectConfigPath: TestHelper.CreateSonarProjectConfig(
                    Directory.CreateDirectory(@"TestCases\EmptyMethod_WithVirtualOverride_DoesNotRaiseIssuesForTestProject_VB").FullName,
                    ProjectType.Test));
    }
}
