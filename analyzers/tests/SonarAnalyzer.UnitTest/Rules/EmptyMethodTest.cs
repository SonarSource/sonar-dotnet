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

#if NET
using Microsoft.CodeAnalysis;
#endif
using SonarAnalyzer.UnitTest.MetadataReferences;
using CS = SonarAnalyzer.Rules.CSharp;
using VB = SonarAnalyzer.Rules.VisualBasic;

namespace SonarAnalyzer.UnitTest.Rules
{
    [TestClass]
    public class EmptyMethodTest
    {
        [TestMethod]
        public void EmptyMethod() =>
            OldVerifier.VerifyAnalyzer(@"TestCases\EmptyMethod.cs",
                new CS.EmptyMethod(),
                ParseOptionsHelper.FromCSharp8,
                MetadataReferenceFacade.NETStandard21);

#if NET
        [TestMethod]
        public void EmptyMethod_CSharp10() =>
            OldVerifier.VerifyAnalyzerFromCSharp10Library(@"TestCases\EmptyMethod.CSharp10.cs", new CS.EmptyMethod());

        [TestMethod]
        public void EmptyMethod_CSharp9() =>
            OldVerifier.VerifyAnalyzerFromCSharp9Console(@"TestCases\EmptyMethod.CSharp9.cs", new CS.EmptyMethod());
#endif

        [TestMethod]
        public void EmptyMethod_CodeFix_Throw() =>
            OldVerifier.VerifyCodeFix<CS.EmptyMethodCodeFix>(
                @"TestCases\EmptyMethod.cs",
                @"TestCases\EmptyMethod.Throw.Fixed.cs",
                new CS.EmptyMethod(),
                CS.EmptyMethodCodeFix.TitleThrow);

        [TestMethod]
        public void EmptyMethod_CodeFix_Comment() =>
            OldVerifier.VerifyCodeFix<CS.EmptyMethodCodeFix>(
                @"TestCases\EmptyMethod.cs",
                @"TestCases\EmptyMethod.Comment.Fixed.cs",
                new CS.EmptyMethod(),
                CS.EmptyMethodCodeFix.TitleComment);

        [TestMethod]
        public void EmptyMethod_VB() =>
            OldVerifier.VerifyAnalyzer(@"TestCases\EmptyMethod.vb", new VB.EmptyMethod());

        [TestMethod]
        public void EmptyMethod_WithVirtualOverride_RaisesIssueForMainProject_CS() =>
            OldVerifier.VerifyAnalyzer(@"TestCases\EmptyMethod.OverrideVirtual.cs", new CS.EmptyMethod());

        [TestMethod]
        public void EmptyMethod_WithVirtualOverride_DoesNotRaiseIssuesForTestProject_CS() =>
            OldVerifier.VerifyNoIssueReportedInTest(@"TestCases\EmptyMethod.OverrideVirtual.cs", new CS.EmptyMethod());

        [TestMethod]
        public void EmptyMethod_WithVirtualOverride_RaisesIssueForMainProject_VB() =>
            OldVerifier.VerifyAnalyzer(@"TestCases\EmptyMethod.OverrideVirtual.vb", new VB.EmptyMethod());

        [TestMethod]
        public void EmptyMethod_WithVirtualOverride_DoesNotRaiseIssuesForTestProject_VB() =>
            OldVerifier.VerifyNoIssueReportedInTest(@"TestCases\EmptyMethod.OverrideVirtual.vb", new VB.EmptyMethod());
    }
}
