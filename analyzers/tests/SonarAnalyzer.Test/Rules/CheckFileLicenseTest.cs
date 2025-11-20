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

using CS = SonarAnalyzer.CSharp.Rules;
using VB = SonarAnalyzer.VisualBasic.Rules;

namespace SonarAnalyzer.Test.Rules
{
    [TestClass]
    public class CheckFileLicenseTest
    {
        private const string SingleLineHeader = "// Copyright (c) SonarSource. All Rights Reserved. Licensed under the LGPL License.  See License.txt in the project root for license information.";
        private const string MultiLineHeader = @"/*
 * SonarQube, open source software quality management tool.
 * Copyright (C) 2008-2013 SonarSource
 * mailto:contact AT sonarsource DOT com
 *
 * SonarQube is free software; you can redistribute it and/or
 * modify it under the terms of the GNU Lesser General Public
 * License as published by the Free Software Foundation; either
 * version 3 of the License, or (at your option) any later version.
 *
 * SonarQube is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU
 * Lesser General Public License for more details.
 *
 * You should have received a copy of the GNU Lesser General Public License
 * along with this program; if not, write to the Free Software Foundation,
 * Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02110-1301, USA.
 */";
        private const string MultiSingleLineCommentHeader = @"//-----
// MyHeader
//-----";
        private const string HeaderForcingLineBreak = @"//---


";
        private const string SingleLineRegexHeader = @"// Copyright \(c\) \w*\. All Rights Reserved\. " +
            @"Licensed under the LGPL License\.  See License\.txt in the project root for license information\.";
        private const string MultiLineRegexHeader = @"/\*
 \* SonarQube, open source software quality management tool\.
 \* Copyright \(C\) \d\d\d\d-\d\d\d\d SonarSource
 \* mailto:contact AT sonarsource DOT com
 \*
 \* SonarQube is free software; you can redistribute it and/or
 \* modify it under the terms of the GNU Lesser General Public
 \* License as published by the Free Software Foundation; either
 \* version 3 of the License, or \(at your option\) any later version\.
 \*
 \* SonarQube is distributed in the hope that it will be useful,
 \* but WITHOUT ANY WARRANTY; without even the implied warranty of
 \* MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE\. See the GNU
 \* Lesser General Public License for more details\.
 \*
 \* You should have received a copy of the GNU Lesser General Public License
 \* along with this program; if not, write to the Free Software Foundation,
 \* Inc\., 51 Franklin Street, Fifth Floor, Boston, MA  02110-1301, USA\.
 \*/";
        private const string MultiLineRegexWithNewLine = "//-{5}\r\n// MyHeader\r\n//-{5}";
        private const string MultiLineRegexWithDot = "//-{5}.+// MyHeader.+//-{5}";
        private const string FailingSingleLineRegexHeader = "[";

        [TestMethod]
        public void CheckFileLicense_WhenUnlicensedFileStartingWithUsing_ShouldBeNoncompliant_CS() =>
            Builder(SingleLineHeader).AddPaths("CheckFileLicense_NoLicenseStartWithUsing.cs").Verify();

        [TestMethod]
        public void CheckFileLicense_WhenLicensedFileStartingWithUsing_ShouldBeCompliant_CS() =>
            Builder(SingleLineHeader).AddPaths("CheckFileLicense_SingleLineLicenseStartWithUsing.cs")
                .WithConcurrentAnalysis(false)
                .VerifyNoIssues();

        [TestMethod]
        public void CheckFileLicense_WhenLicensedFileStartingWithUsingAndUsingCustomValues_ShouldBeCompliant_CS() =>
            Builder(SingleLineRegexHeader, true).AddPaths("CheckFileLicense_SingleLineLicenseStartWithUsing.cs")
                .WithConcurrentAnalysis(false)
                .VerifyNoIssues();

        [TestMethod]
        public void CheckFileLicense_WhenLicensedWithMultilineCommentStartingWithUsing_ShouldBeCompliant_CS() =>
            Builder(MultiLineHeader).AddPaths("CheckFileLicense_MultiLineLicenseStartWithUsing.cs")
                .WithConcurrentAnalysis(false)
                .VerifyNoIssues();

        [TestMethod]
        public void CheckFileLicense_WhenLicensedWithMultilineCommentStartingWithUsingWithCustomValues_ShouldBeCompliant_CS() =>
            Builder(MultiLineRegexHeader, true).AddPaths("CheckFileLicense_MultiLineLicenseStartWithUsing.cs")
                .WithConcurrentAnalysis(false)
                .VerifyNoIssues();

        [TestMethod]
        public void CheckFileLicense_WhenNoLicenseStartingWithNamespace_ShouldBeNonCompliant_CS() =>
            Builder(SingleLineHeader).AddPaths("CheckFileLicense_NoLicenseStartWithNamespace.cs").Verify();

        [TestMethod]
        public void CheckFileLicense_WhenLicensedWithSingleLineCommentStartingWithNamespace_ShouldBeCompliant_CS() =>
            Builder(SingleLineHeader).AddPaths("CheckFileLicense_SingleLineLicenseStartWithNamespace.cs")
                .WithConcurrentAnalysis(false)
                .VerifyNoIssues();

        [TestMethod]
        public void CheckFileLicense_WhenLicensedWithSingleLineCommentStartingWithNamespaceAndUsingCustomValues_ShouldBeCompliant_CS() =>
            Builder(SingleLineRegexHeader, true).AddPaths("CheckFileLicense_SingleLineLicenseStartWithNamespace.cs")
                .WithConcurrentAnalysis(false)
                .VerifyNoIssues();

        [TestMethod]
        public void CheckFileLicense_WhenLicensedWithMultilineCommentStartingWithNamespace_ShouldBeCompliant_CS() =>
            Builder(MultiLineHeader).AddPaths("CheckFileLicense_MultiLineLicenseStartWithNamespace.cs")
                .WithConcurrentAnalysis(false)
                .VerifyNoIssues();

        [TestMethod]
        public void CheckFileLicense_WhenLicensedWithMultilineCommentStartingWithNamespaceAndUsingCustomValues_ShouldBeCompliant_CS() =>
            Builder(MultiLineRegexHeader, true).AddPaths("CheckFileLicense_MultiLineLicenseStartWithNamespace.cs")
                .WithConcurrentAnalysis(false)
                .VerifyNoIssues();

        [TestMethod]
        public void CheckFileLicense_WhenLicensedWithMultiSingleLineCommentStartingWithNamespaceAndNoRegex_ShouldBeCompliant_CS() =>
            Builder(MultiSingleLineCommentHeader).AddPaths("CheckFileLicense_MultiSingleLineLicenseStartWithNamespace.cs")
                .WithConcurrentAnalysis(false)
                .VerifyNoIssues();

        [TestMethod]
        public void CheckFileLicense_WhenLicensedWithMultiSingleLineCommentStartingWithAdditionalComments_ShouldBeCompliant_CS() =>
            Builder(MultiSingleLineCommentHeader).AddPaths("CheckFileLicense_MultiSingleLineLicenseStartWithAdditionalComment.cs")
                .WithConcurrentAnalysis(false)
                .VerifyNoIssues();

        [TestMethod]
        public void CheckFileLicense_WhenLicensedWithMultiSingleLineCommentStartingWithAdditionalCommentOnSameLine_ShouldBeNonCompliant_CS() =>
            Builder(MultiSingleLineCommentHeader).AddPaths("CheckFileLicense_MultiSingleLineLicenseStartWithAdditionalCommentOnSameLine.cs").Verify();

        [TestMethod]
        public void CheckFileLicense_WithForcingEmptyLines_ShouldBeNonCompliant_CS() =>
            Builder(HeaderForcingLineBreak).AddPaths("CheckFileLicense_ForcingEmptyLinesKo.cs").Verify();

        [TestMethod]
        public void CheckFileLicense_WithForcingEmptyLines_ShouldBeCompliant_CS() =>
            Builder(HeaderForcingLineBreak).AddPaths("CheckFileLicense_ForcingEmptyLinesOk.cs")
                .WithConcurrentAnalysis(false)
                .VerifyNoIssues();

        [TestMethod]
        public void CheckFileLicense_WhenLicensedWithMultiSingleLineCommentStartingWithNamespaceAndMultiLineRegexWithNewLine_ShouldBeCompliant_CS() =>
            Builder(MultiLineRegexWithNewLine, true).AddPaths("CheckFileLicense_MultiSingleLineLicenseStartWithNamespace.cs")
                .WithConcurrentAnalysis(false)
                .VerifyNoIssues();

        [TestMethod]
        public void CheckFileLicense_WhenLicensedWithMultiSingleLineCommentStartingWithNamespaceAndMultiLineRegexWithDot_ShouldBeCompliant_CS() =>
            Builder(MultiLineRegexWithDot, true).AddPaths("CheckFileLicense_MultiSingleLineLicenseStartWithNamespace.cs")
                .WithConcurrentAnalysis(false)
                .VerifyNoIssues();

        [TestMethod]
        public void CheckFileLicense_WhenEmptyFile_ShouldBeNonCompliant_CS() =>
            // While we put Noncompliant annotation on 1st line of any file, in this case, the file needs to be empty
            Builder(SingleLineHeader).AddPaths("CheckFileLicense_EmptyFile.cs").Invoking(x => x.Verify()).Should().Throw<DiagnosticVerifierException>().WithMessage("""
                There are differences for CSharp7 CheckFileLicense_EmptyFile.cs:
                  Line 1: Unexpected issue 'Add or update the header of this file.' Rule S1451

                There is 1 more difference in CheckFileLicense_EmptyFile.Concurrent.cs

                """);

        [TestMethod]
        public void CheckFileLicenseCodeFix_WhenThereIsAYearDifference_ShouldBeNonCompliant_CS() =>
            Builder(MultiLineHeader).AddPaths("CheckFileLicense_YearDifference.cs").Verify();

        [TestMethod]
        public void CheckFileLicense_WhenProvidingAnInvalidRegex_ShouldThrowException_CS()
        {
            var compilation = SolutionBuilder.CreateSolutionFromPath(@"TestCases\CheckFileLicense_NoLicenseStartWithUsing.cs").Compile(LanguageOptions.CSharpLatest.ToArray()).Single();
            var errors = DiagnosticVerifier.AnalyzerExceptions(compilation, new CS.CheckFileLicense { HeaderFormat = FailingSingleLineRegexHeader, IsRegularExpression = true });
            errors.Should().ContainSingle().Which.GetMessage().Should()
                .Contain("System.InvalidOperationException")
                .And.Contain("Invalid regular expression: " + FailingSingleLineRegexHeader);
        }

        [TestMethod]
        public void CheckFileLicense_WhenUsingComplexRegex_ShouldBeCompliant_CS() =>
            Builder(@"// <copyright file="".*\.cs"" company="".*"">\r\n// Copyright \(c\) 2012 All Rights Reserved\r\n// </copyright>\r\n// <author>.*</author>\r\n// <date>.*</date>\r\n// <summary>.*</summary>\r\n", true)
                .AddPaths("CheckFileLicense_ComplexRegex.cs")
                .WithConcurrentAnalysis(false)
                .VerifyNoIssues();

        [TestMethod]
        public void CheckFileLicense_WhenUsingMultilinesHeaderAsSingleLineString_ShouldBeCompliant_CS() =>
            Builder(@"// <copyright file=""ProgramHeader2.cs"" company=""My Company Name"">\r\n// Copyright (c) 2012 All Rights Reserved\r\n// </copyright>\r\n// <author>Name of the Author</author>\r\n// <date>08/22/2017 12:39:58 AM </date>\r\n// <summary>Class representing a Sample entity</summary>\r\n", false)
                .AddPaths("CheckFileLicense_ComplexRegex.cs")
                .WithConcurrentAnalysis(false)
                .VerifyNoIssues();

        [TestMethod]
        public void CheckFileLicenseCodeFix_WhenNoLicenseStartWithNamespaceAndUsesDefaultValues_ShouldBeNoncompliant_CS() =>
            new VerifierBuilder<CS.CheckFileLicense>()
                .WithCodeFix<CS.CheckFileLicenseCodeFix>()
                .AddPaths("CheckFileLicense_DefaultValues.cs")
                .WithCodeFixedPaths("CheckFileLicense_DefaultValues.Fixed.cs")
                .VerifyCodeFix();

        [TestMethod]
        public void CheckFileLicenseCodeFix_CSharp9_ShouldBeNoncompliant_CS() =>
            new VerifierBuilder<CS.CheckFileLicense>()
                .WithCodeFix<CS.CheckFileLicenseCodeFix>()
                .AddPaths("CheckFileLicense_CSharp9.cs")
                .WithCodeFixedPaths("CheckFileLicense_CSharp9.Fixed.cs")
                .WithOptions(LanguageOptions.FromCSharp9)
                .VerifyCodeFix();

        [TestMethod]
        public void CheckFileLicenseCodeFix_WhenNoLicenseStartingWithUsing_ShouldBeFixedAsExpected_CS() =>
            Builder(SingleLineHeader)
                .WithCodeFix<CS.CheckFileLicenseCodeFix>()
                .AddPaths("CheckFileLicense_NoLicenseStartWithUsing.cs")
                .WithCodeFixedPaths("CheckFileLicense_NoLicenseStartWithUsing.Fixed.cs")
                .VerifyCodeFix();

        [TestMethod]
        public void CheckFileLicenseCodeFix_WhenNoLicenseStartingWithNamespace_ShouldBeFixedAsExpected_CS() =>
            Builder(SingleLineHeader)
                .WithCodeFix<CS.CheckFileLicenseCodeFix>()
                .AddPaths("CheckFileLicense_NoLicenseStartWithNamespace.cs")
                .WithCodeFixedPaths("CheckFileLicense_NoLicenseStartWithNamespace.Fixed.cs")
                .VerifyCodeFix();

        [TestMethod]
        public void CheckFileLicenseCodeFix_WhenThereIsAYearDifference_ShouldBeFixedAsExpected_CS() =>
            Builder(MultiLineHeader)
                .WithCodeFix<CS.CheckFileLicenseCodeFix>()
                .AddPaths("CheckFileLicense_YearDifference.cs")
                .WithCodeFixedPaths("CheckFileLicense_YearDifference.Fixed.cs")
                .VerifyCodeFix();

        [TestMethod]
        public void CheckFileLicenseCodeFix_WhenOutdatedLicenseStartingWithUsing_ShouldBeFixedAsExpected_CS() =>
            Builder(MultiLineHeader)
                .WithCodeFix<CS.CheckFileLicenseCodeFix>()
                .AddPaths("CheckFileLicense_OutdatedLicenseStartWithUsing.cs")
                .WithCodeFixedPaths("CheckFileLicense_OutdatedLicenseStartWithUsing.Fixed.cs")
                .VerifyCodeFix();

        [TestMethod]
        public void CheckFileLicenseCodeFix_WhenOutdatedLicenseStartingWithNamespace_ShouldBeFixedAsExpected_CS() =>
            Builder(MultiLineHeader)
                .WithCodeFix<CS.CheckFileLicenseCodeFix>()
                .AddPaths("CheckFileLicense_OutdatedLicenseStartWithNamespace.cs")
                .WithCodeFixedPaths("CheckFileLicense_OutdatedLicenseStartWithNamespace.Fixed.cs")
                .VerifyCodeFix();

        [TestMethod]
        public void CheckFileLicense_NullHeader_NoIssueReported_CS() =>
            Builder(null).AddPaths("CheckFileLicense_NoLicenseStartWithNamespace.cs").VerifyNoIssues();

        // No need to duplicate all test cases from C#, because we are sharing the implementation
        [TestMethod]
        public void CheckFileLicense_NonCompliant_VB() =>
            new VerifierBuilder<VB.CheckFileLicense>().AddPaths("CheckFileLicense_NonCompliant.vb").Verify();

        [TestMethod]
        public void CheckFileLicense_Compliant_VB() =>
            new VerifierBuilder().AddAnalyzer(() => new VB.CheckFileLicense
            {
                HeaderFormat = @"Copyright \(c\) [0-9]+ All Rights Reserved
",
                IsRegularExpression = true
            })
                .AddPaths("CheckFileLicense_Compliant.vb")
                .VerifyNoIssues();

        private static VerifierBuilder Builder(string headerFormat, bool isRegularExpression = false) =>
            new VerifierBuilder().AddAnalyzer(() => new CS.CheckFileLicense { HeaderFormat = headerFormat, IsRegularExpression = isRegularExpression });
    }
}
