/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2019 SonarSource SA
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

extern alias csharp;
using System;
using csharp::SonarAnalyzer.Rules.CSharp;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SonarAnalyzer.UnitTest.TestFramework;

namespace SonarAnalyzer.UnitTest.Rules
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
        private const string MultiSingleLineRegexHeader = "//-{5}\r\n// MyHeader\r\n//-{5}";
        private const string FailingSingleLineRegexHeader = "[";

        [TestMethod]
        [TestCategory("Rule")]
        public void CheckFileLicense_WhenUnlicensedFileStartingWithUsing_ShouldBeNoncompliant_CS()
        {
            Verifier.VerifyAnalyzer(@"TestCases\CheckFileLicense_NoLicenseStartWithUsing.cs",
                new CheckFileLicense { HeaderFormat = SingleLineHeader });
        }

        [TestMethod]
        [TestCategory("Rule")]
        public void CheckFileLicense_WhenLicensedFileStartingWithUsing_ShouldBeCompliant_CS()
        {
            Verifier.VerifyAnalyzer(@"TestCases\CheckFileLicense_SingleLineLicenseStartWithUsing.cs",
                new CheckFileLicense { HeaderFormat = SingleLineHeader });
        }

        [TestMethod]
        [TestCategory("Rule")]
        public void CheckFileLicense_WhenLicensedFileStartingWithUsingAndUsingCustomValues_ShouldBeCompliant_CS()
        {
            Verifier.VerifyAnalyzer(@"TestCases\CheckFileLicense_SingleLineLicenseStartWithUsing.cs",
                new CheckFileLicense { HeaderFormat = SingleLineRegexHeader, IsRegularExpression = true });
        }

        [TestMethod]
        [TestCategory("Rule")]
        public void CheckFileLicense_WhenLicensedWithMultilineCommentStartingWithUsing_ShouldBeCompliant_CS()
        {
            Verifier.VerifyAnalyzer(@"TestCases\CheckFileLicense_MultiLineLicenseStartWithUsing.cs",
                new CheckFileLicense { HeaderFormat = MultiLineHeader });
        }

        [TestMethod]
        [TestCategory("Rule")]
        public void CheckFileLicense_WhenLicensedWithMultilineCommentStartingWithUsingWithCustomValues_ShouldBeCompliant_CS()
        {
            Verifier.VerifyAnalyzer(@"TestCases\CheckFileLicense_MultiLineLicenseStartWithUsing.cs",
                new CheckFileLicense { HeaderFormat = MultiLineRegexHeader, IsRegularExpression = true });
        }

        [TestMethod]
        [TestCategory("Rule")]
        public void CheckFileLicense_WhenNoLicenseStartingWithNamespace_ShouldBeNonCompliant_CS()
        {
            Verifier.VerifyAnalyzer(@"TestCases\CheckFileLicense_NoLicenseStartWithNamespace.cs",
                new CheckFileLicense { HeaderFormat = SingleLineHeader });
        }

        [TestMethod]
        [TestCategory("Rule")]
        public void CheckFileLicense_WhenLicensedWithSingleLineCommentStartingWithNamespace_ShouldBeCompliant_CS()
        {
            Verifier.VerifyAnalyzer(@"TestCases\CheckFileLicense_SingleLineLicenseStartWithNamespace.cs",
                new CheckFileLicense { HeaderFormat = SingleLineHeader });
        }

        [TestMethod]
        [TestCategory("Rule")]
        public void CheckFileLicense_WhenLicensedWithSingleLineCommentStartingWithNamespaceAndUsingCustomValues_ShouldBeCompliant_CS()
        {
            Verifier.VerifyAnalyzer(@"TestCases\CheckFileLicense_SingleLineLicenseStartWithNamespace.cs",
                new CheckFileLicense { HeaderFormat = SingleLineRegexHeader, IsRegularExpression = true });
        }

        [TestMethod]
        [TestCategory("Rule")]
        public void CheckFileLicense_WhenLicensedWithMultilineCommentStartingWithNamespace_ShouldBeCompliant_CS()
        {
            Verifier.VerifyAnalyzer(@"TestCases\CheckFileLicense_MultiLineLicenseStartWithNamespace.cs",
                new CheckFileLicense { HeaderFormat = MultiLineHeader });
        }

        [TestMethod]
        [TestCategory("Rule")]
        public void CheckFileLicense_WhenLicensedWithMultilineCommentStartingWithNamespaceAndUsingCustomValues_ShouldBeCompliant_CS()
        {
            Verifier.VerifyAnalyzer(@"TestCases\CheckFileLicense_MultiLineLicenseStartWithNamespace.cs",
                new CheckFileLicense { HeaderFormat = MultiLineRegexHeader, IsRegularExpression = true });
        }

        [TestMethod]
        [TestCategory("Rule")]
        public void CheckFileLicense_WhenLicensedWithMultiSingleLineCommentStartingWithNamespaceAndNoRegex_ShouldBeCompliant_CS()
        {
            Verifier.VerifyAnalyzer(@"TestCases\CheckFileLicense_MultiSingleLineLicenseStartWithNamespace.cs",
                new CheckFileLicense { HeaderFormat = MultiSingleLineCommentHeader });
        }

        [TestMethod]
        [TestCategory("Rule")]
        public void CheckFileLicense_WhenLicensedWithMultiSingleLineCommentStartingWithAdditionalComments_ShouldBeCompliant_CS()
        {
            Verifier.VerifyAnalyzer(@"TestCases\CheckFileLicense_MultiSingleLineLicenseStartWithAdditionalComment.cs",
                new CheckFileLicense { HeaderFormat = MultiSingleLineCommentHeader });
        }

        [TestMethod]
        [TestCategory("Rule")]
        public void CheckFileLicense_WhenLicensedWithMultiSingleLineCommentStartingWithAdditionalCommentOnSameLine_ShouldBeNonCompliant_CS()
        {
            Verifier.VerifyAnalyzer(@"TestCases\CheckFileLicense_MultiSingleLineLicenseStartWithAdditionalCommentOnSameLine.cs",
                new CheckFileLicense { HeaderFormat = MultiSingleLineCommentHeader });
        }

        [TestMethod]
        [TestCategory("Rule")]
        public void CheckFileLicense_WithForcingEmptyLines_ShouldBeNonCompliant_CS()
        {
            Verifier.VerifyAnalyzer(@"TestCases\CheckFileLicense_ForcingEmptyLinesKo.cs",
                new CheckFileLicense { HeaderFormat = HeaderForcingLineBreak });
        }

        [TestMethod]
        [TestCategory("Rule")]
        public void CheckFileLicense_WithForcingEmptyLines_ShouldBeCompliant_CS()
        {
            Verifier.VerifyAnalyzer(@"TestCases\CheckFileLicense_ForcingEmptyLinesOk.cs",
                new CheckFileLicense { HeaderFormat = HeaderForcingLineBreak });
        }

        [TestMethod]
        [TestCategory("Rule")]
        public void CheckFileLicense_WhenLicensedWithMultiSingleLineCommentStartingWithNamespaceAndRegex_ShouldBeCompliant_CS()
        {
            Verifier.VerifyAnalyzer(@"TestCases\CheckFileLicense_MultiSingleLineLicenseStartWithNamespace.cs",
                new CheckFileLicense { HeaderFormat = MultiSingleLineRegexHeader, IsRegularExpression = true });
        }

        [TestMethod]
        [TestCategory("Rule")]
        public void CheckFileLicense_WhenEmptyFile_ShouldBeNonCompliant_CS()
        {
            Action action =
                () => Verifier.VerifyAnalyzer(@"TestCases\CheckFileLicense_EmptyFile.cs",
                    new CheckFileLicense { HeaderFormat = SingleLineHeader });
            action.Should().Throw<UnexpectedDiagnosticException>()
                  .WithMessage("Issue with message 'Add or update the header of this file.' not expected on line 1");
        }

        [TestMethod]
        [TestCategory("Rule")]
        public void CheckFileLicense_WhenProvidingAnInvalidRegex_ShouldThrowException_CS()
        {
            Action action = () => Verifier.VerifyAnalyzer(@"TestCases\CheckFileLicense_NoLicenseStartWithUsing.cs",
                new CheckFileLicense { HeaderFormat = FailingSingleLineRegexHeader, IsRegularExpression = true });

            action.Should().Throw<AssertFailedException>()
                .WithMessage("*error AD0001:*'SonarAnalyzer.Rules.CSharp.CheckFileLicense'*System.InvalidOperationException*'Invalid regular expression: ['.*");
        }

        [TestMethod]
        [TestCategory("Rule")]
        public void CheckFileLicense_WhenUsingComplexRegex_ShouldBeCompliant_CS()
        {
            Verifier.VerifyAnalyzer(@"TestCases\CheckFileLicense_ComplexRegex.cs",
               new CheckFileLicense { HeaderFormat = @"// <copyright file="".*\.cs"" company="".*"">\r\n// Copyright \(c\) 2012 All Rights Reserved\r\n// </copyright>\r\n// <author>.*</author>\r\n// <date>.*</date>\r\n// <summary>.*</summary>\r\n", IsRegularExpression = true });
        }

        [TestMethod]
        [TestCategory("Rule")]
        public void CheckFileLicense_WhenUsingMultilinesHeaderAsSingleLineString_ShouldBeCompliant_CS()
        {
            Verifier.VerifyAnalyzer(@"TestCases\CheckFileLicense_ComplexRegex.cs",
               new CheckFileLicense { HeaderFormat = @"// <copyright file=""ProgramHeader2.cs"" company=""My Company Name"">\r\n// Copyright (c) 2012 All Rights Reserved\r\n// </copyright>\r\n// <author>Name of the Authour</author>\r\n// <date>08/22/2017 12:39:58 AM </date>\r\n// <summary>Class representing a Sample entity</summary>\r\n", IsRegularExpression = false });
        }

        [TestMethod]
        [TestCategory("Rule")]
        public void CheckFileLicenseCodeFix_WhenNoLicenseStartWithNamespaceAndUsesDefaultValues_ShouldBeNoncompliant_CS()
        {
            Verifier.VerifyCodeFix(
            @"TestCases\CheckFileLicense_DefaultValues.cs",
            @"TestCases\CheckFileLicense_DefaultValues.Fixed.cs",
            new CheckFileLicense(),
            new CheckFileLicenseCodeFixProvider());
        }

        [TestMethod]
        [TestCategory("CodeFix")]
        public void CheckFileLicenseCodeFix_WhenNoLicenseStartingWithUsing_ShouldBeFixedAsExpected_CS()
        {
            Verifier.VerifyCodeFix(
                @"TestCases\CheckFileLicense_NoLicenseStartWithUsing.cs",
                @"TestCases\CheckFileLicense_NoLicenseStartWithUsing.Fixed.cs",
                new CheckFileLicense { HeaderFormat = SingleLineHeader },
                new CheckFileLicenseCodeFixProvider());
        }

        [TestMethod]
        [TestCategory("CodeFix")]
        public void CheckFileLicenseCodeFix_WhenNoLicenseStartingWithNamespace_ShouldBeFixedAsExpected_CS()
        {
            Verifier.VerifyCodeFix(
                @"TestCases\CheckFileLicense_NoLicenseStartWithNamespace.cs",
                @"TestCases\CheckFileLicense_NoLicenseStartWithNamespace.Fixed.cs",
                new CheckFileLicense { HeaderFormat = SingleLineHeader },
                new CheckFileLicenseCodeFixProvider());
        }

        [TestMethod]
        [TestCategory("CodeFix")]
        public void CheckFileLicenseCodeFix_WhenOutdatedLicenseStartingWithUsing_ShouldBeFixedAsExpected_CS()
        {
            Verifier.VerifyCodeFix(
                @"TestCases\CheckFileLicense_OutdatedLicenseStartWithUsing.cs",
                @"TestCases\CheckFileLicense_OutdatedLicenseStartWithUsing.Fixed.cs",
                new CheckFileLicense { HeaderFormat = MultiLineHeader },
                new CheckFileLicenseCodeFixProvider());
        }

        [TestMethod]
        [TestCategory("CodeFix")]
        public void CheckFileLicenseCodeFix_WhenOutdatedLicenseStartingWithNamespace_ShouldBeFixedAsExpected_CS()
        {
            Verifier.VerifyCodeFix(
                @"TestCases\CheckFileLicense_OutdatedLicenseStartWithNamespace.cs",
                @"TestCases\CheckFileLicense_OutdatedLicenseStartWithNamespace.Fixed.cs",
                new CheckFileLicense { HeaderFormat = MultiLineHeader },
                new CheckFileLicenseCodeFixProvider());
        }

        // No need to duplicate all test cases from C#, because we are sharing the implementation
        [TestMethod]
        [TestCategory("Rule")]
        public void CheckFileLicense_NonCompliant_VB()
        {
            Verifier.VerifyAnalyzer(@"TestCases\CheckFileLicense_NonCompliant.vb",
                new SonarAnalyzer.Rules.VisualBasic.CheckFileLicense());
        }

        [TestMethod]
        [TestCategory("Rule")]
        public void CheckFileLicense_Compliant_VB()
        {
            Verifier.VerifyAnalyzer(@"TestCases\CheckFileLicense_Compliant.vb",
                new SonarAnalyzer.Rules.VisualBasic.CheckFileLicense
                {
                    HeaderFormat = @"Copyright \(c\) [0-9]+ All Rights Reserved
",
                    IsRegularExpression = true
                });

        }
    }
}

