/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2017 SonarSource SA
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

using Microsoft.VisualStudio.TestTools.UnitTesting;
using SonarAnalyzer.Rules.CSharp;

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
        private const string FailingSingleLineRegexHeader = "[";

        [TestMethod]
        [TestCategory("Rule")]
        public void CheckUnlicensedFileStartWithUsing()
        {
            Verifier.VerifyAnalyzer(@"TestCases\CheckFileLicense_NoLicenseStartWithUsing.cs", new CheckFileLicense { HeaderFormat = SingleLineHeader });
        }

        [TestMethod]
        [TestCategory("Rule")]
        public void CheckSingleLineLicensedFileStartWithUsing()
        {
            Verifier.VerifyAnalyzer(@"TestCases\CheckFileLicense_SingleLineLicenseStartWithUsing.cs", new CheckFileLicense { HeaderFormat = SingleLineHeader });
        }

        [TestMethod]
        [TestCategory("Rule")]
        public void CheckSingleLineRegexLicensedFileStartWithUsing()
        {
            Verifier.VerifyAnalyzer(@"TestCases\CheckFileLicense_SingleLineLicenseStartWithUsing.cs", new CheckFileLicense { HeaderFormat = SingleLineRegexHeader, IsRegularExpression = true });
        }

        [TestMethod]
        [TestCategory("Rule")]
        public void CheckMultiLineLicensedFileStartWithUsing()
        {
            Verifier.VerifyAnalyzer(@"TestCases\CheckFileLicense_MultiLineLicenseStartWithUsing.cs", new CheckFileLicense { HeaderFormat = MultiLineHeader });
        }

        [TestMethod]
        [TestCategory("Rule")]
        public void CheckMultiLineRegexLicensedFileStartWithUsing()
        {
            Verifier.VerifyAnalyzer(@"TestCases\CheckFileLicense_MultiLineLicenseStartWithUsing.cs", new CheckFileLicense { HeaderFormat = MultiLineRegexHeader, IsRegularExpression = true });
        }

        [TestMethod]
        [TestCategory("Rule")]
        public void CheckUnlicensedFileStartWithNamespace()
        {
            Verifier.VerifyAnalyzer(@"TestCases\CheckFileLicense_NoLicenseStartWithNamespace.cs", new CheckFileLicense { HeaderFormat = SingleLineHeader });
        }

        [TestMethod]
        [TestCategory("Rule")]
        public void CheckSingleLineLicensedFileStartWithNamespace()
        {
            Verifier.VerifyAnalyzer(@"TestCases\CheckFileLicense_SingleLineLicenseStartWithNamespace.cs", new CheckFileLicense { HeaderFormat = SingleLineHeader });
        }

        [TestMethod]
        [TestCategory("Rule")]
        public void CheckSingleLineRegexLicensedFileStartWithNamespace()
        {
            Verifier.VerifyAnalyzer(@"TestCases\CheckFileLicense_SingleLineLicenseStartWithNamespace.cs", new CheckFileLicense { HeaderFormat = SingleLineRegexHeader, IsRegularExpression = true });
        }

        [TestMethod]
        [TestCategory("Rule")]
        public void CheckMultiLineLicensedFileStartWithNamespace()
        {
            Verifier.VerifyAnalyzer(@"TestCases\CheckFileLicense_MultiLineLicenseStartWithNamespace.cs", new CheckFileLicense { HeaderFormat = MultiLineHeader });
        }

        [TestMethod]
        [TestCategory("Rule")]
        public void CheckMultiLineRegexLicensedFileStartWithNamespace()
        {
            Verifier.VerifyAnalyzer(@"TestCases\CheckFileLicense_MultiLineLicenseStartWithNamespace.cs", new CheckFileLicense { HeaderFormat = MultiLineRegexHeader, IsRegularExpression = true });
        }

        [TestMethod]
        [TestCategory("Rule")]
        public void CheckEmptyFile()
        {
            try
            {
                Verifier.VerifyAnalyzer(@"TestCases\CheckFileLicense_EmptyFile.cs", new CheckFileLicense { HeaderFormat = SingleLineHeader });
            }
            catch (AssertFailedException ex)
            {
                // Putting the expected issue within the file make the file being no longer empty so we need to catch the expected issue
                // from here.
                if (ex.Message != "Issue not expected on line 1")
                {
                    throw;
                }
            }
        }

        [TestMethod]
        [TestCategory("Rule")]
        public void CheckEmptyHeaderFormat()
        {
            const string expectedErrorMessage =
                "Expected collection to be empty, but found {error AD0001: The Compiler Analyzer 'SonarAnalyzer.Rules.CSharp.CheckFileLicense' " +
                "threw an exception of type 'System.ArgumentException' with message 'Expects a non-empty license header" +
                "\r\nParameter name: headerFormat'.}.";
            try
            {
                Verifier.VerifyAnalyzer(@"TestCases\CheckFileLicense_NoLicenseStartWithUsing.cs", new CheckFileLicense { HeaderFormat = string.Empty });
            }
            catch (AssertFailedException ex)
            {
                if (ex.Message != expectedErrorMessage)
                {
                    throw;
                }
            }
        }

        [TestMethod]
        [TestCategory("Rule")]
        public void CheckInvalidRegex()
        {
            const string expectedErrorMessage =
                "Expected collection to be empty, but found {error AD0001: The Compiler Analyzer 'SonarAnalyzer.Rules.CSharp.CheckFileLicense' " +
                "threw an exception of type 'System.ArgumentException' with message 'Invalid regular expression: " +
                FailingSingleLineRegexHeader +
                "\r\nParameter name: headerFormat'.}.";
            try
            {
                Verifier.VerifyAnalyzer(@"TestCases\CheckFileLicense_NoLicenseStartWithUsing.cs", new CheckFileLicense { HeaderFormat = FailingSingleLineRegexHeader, IsRegularExpression = true });
            }
            catch (AssertFailedException ex)
            {
                if (ex.Message != expectedErrorMessage)
                {
                    throw;
                }
            }
        }

        [TestMethod]
        [TestCategory("Rule")]
        public void CheckUnlicensedFileDefaultValues_CodeFix()
        {
            Verifier.VerifyCodeFix(
            @"TestCases\CheckFileLicense_DefaultValues.cs",
            @"TestCases\CheckFileLicense_DefaultValues.Fixed.cs",
            new CheckFileLicense(),
            new CheckFileLicenseCodeFixProvider());
        }

        [TestMethod]
        [TestCategory("CodeFix")]
        public void CheckUnlicensedFileStartWithUsing_CodeFix()
        {
            Verifier.VerifyCodeFix(
                @"TestCases\CheckFileLicense_NoLicenseStartWithUsing.cs",
                @"TestCases\CheckFileLicense_NoLicenseStartWithUsing.Fixed.cs",
                new CheckFileLicense { HeaderFormat = SingleLineHeader },
                new CheckFileLicenseCodeFixProvider());
        }

        [TestMethod]
        [TestCategory("CodeFix")]
        public void CheckUnlicensedFileStartWithNamespace_CodeFix()
        {
            Verifier.VerifyCodeFix(
                @"TestCases\CheckFileLicense_NoLicenseStartWithNamespace.cs",
                @"TestCases\CheckFileLicense_NoLicenseStartWithNamespace.Fixed.cs",
                new CheckFileLicense { HeaderFormat = SingleLineHeader },
                new CheckFileLicenseCodeFixProvider());
        }

        [TestMethod]
        [TestCategory("CodeFix")]
        public void CheckOutdatedlicensedFileStartWithUsing_CodeFix()
        {
            Verifier.VerifyCodeFix(
                @"TestCases\CheckFileLicense_OutdatedLicenseStartWithUsing.cs",
                @"TestCases\CheckFileLicense_OutdatedLicenseStartWithUsing.Fixed.cs",
                new CheckFileLicense { HeaderFormat = MultiLineHeader },
                new CheckFileLicenseCodeFixProvider());
        }

        [TestMethod]
        [TestCategory("CodeFix")]
        public void CheckOutdatedlicensedFileStartWithNamespace_CodeFix()
        {
            Verifier.VerifyCodeFix(
                @"TestCases\CheckFileLicense_OutdatedLicenseStartWithNamespace.cs",
                @"TestCases\CheckFileLicense_OutdatedLicenseStartWithNamespace.Fixed.cs",
                new CheckFileLicense { HeaderFormat = MultiLineHeader },
                new CheckFileLicenseCodeFixProvider());
        }
    }
}