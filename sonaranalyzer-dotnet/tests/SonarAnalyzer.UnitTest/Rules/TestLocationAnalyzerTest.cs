/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2018 SonarSource SA
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
extern alias vbnet;

using System.Linq;
using FluentAssertions;
using Microsoft.CodeAnalysis;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SonarAnalyzer.Protobuf;
using SonarAnalyzer.UnitTest.Helpers;
using CSharpTestLocationAnalyzer = csharp.SonarAnalyzer.Rules.TestLocationAnalyzer;
using VisualBasicTestLocationAnalyzer = vbnet.SonarAnalyzer.Rules.TestLocationAnalyzer;

namespace SonarAnalyzer.UnitTest.Rules
{
    [TestClass]
    public class TestLocationAnalyzerTest
    {
        [DataTestMethod]
        [DataRow("1.1.11")]
        [DataRow(Constants.NuGetLatestVersion)]
        [TestCategory("Rule")]
        public void FileMetadataAnalyzer_MsTest_CSharp(string mstestVersion)
        {
            var projectFiles = new[]
            {
                "TestCases/TestClassShouldHaveTestMethod.MsTest.cs",
                "TestCases/TestMethodShouldHaveCorrectSignature.MsTest.cs",
            };

            Verifier.VerifyUtilityAnalyzer<TestLocation>(
                projectFiles,
                new CSharpTestLocationAnalyzer_
                {
                    IsEnabled = true,
                    WorkingPath = @"TestLocationAnalyzer",
                },
                @"TestLocationAnalyzer\test-location.pb",
                (messages) =>
                {
                    messages.Should().HaveCount(projectFiles.Length + 1); // + ExtraEmptyFile.g.cs

                    messages[0].FilePath.Should().EndWith("ExtraEmptyFile.g.cs");
                    messages[0].TestNames.Should().BeEmpty();

                    messages[1].FilePath.Should().EndWith("TestClassShouldHaveTestMethod.MsTest.cs");
                    messages[1].TestNames.Should().OnlyContain(
                        "Tests.Diagnostics.ClassTest9.Foo",
                        "Tests.Diagnostics.ClassTest12.Foo",
                        "Tests.Diagnostics.Foo_WhenFoo_ExpectsFoo.Foo_WhenFoo_ExpectsFoo"
                        );

                    messages[2].FilePath.Should().EndWith("TestMethodShouldHaveCorrectSignature.MsTest.cs");
                    messages[2].TestNames.Should().OnlyContain(
                        "Tests.Diagnostics.MsTestTest.PrivateTestMethod",
                        "Tests.Diagnostics.MsTestTest.ProtectedTestMethod",
                        "Tests.Diagnostics.MsTestTest.InternalTestMethod",
                        "Tests.Diagnostics.MsTestTest.AsyncTestMethod",
                        "Tests.Diagnostics.MsTestTest.GenericTestMethod",
                        "Tests.Diagnostics.MsTestTest.MultiErrorsMethod1",
                        "Tests.Diagnostics.MsTestTest.MultiErrorsMethod2",
                        "Tests.Diagnostics.MsTestTest.DoSomethingAsync",
                        "Tests.Diagnostics.MsTestTest_DataTestMethods.PrivateTestMethod",
                        "Tests.Diagnostics.MsTestTest_DataTestMethods.ProtectedTestMethod",
                        "Tests.Diagnostics.MsTestTest_DataTestMethods.InternalTestMethod",
                        "Tests.Diagnostics.MsTestTest_DataTestMethods.AsyncTestMethod",
                        "Tests.Diagnostics.MsTestTest_DataTestMethods.GenericTestMethod",
                        "Tests.Diagnostics.MsTestTest_DataTestMethods.MultiErrorsMethod1",
                        "Tests.Diagnostics.MsTestTest_DataTestMethods.MultiErrorsMethod2",
                        "Tests.Diagnostics.MsTestTest_DataTestMethods.DoSomethingAsync"
                        );
                },
                additionalReferences: Enumerable.Empty<MetadataReference>()
                    .Union(NuGetMetadataReference.MSTestTestFramework(mstestVersion))
                    .Union(NuGetMetadataReference.FluentAssertions(Constants.NuGetLatestVersion)));
        }

        [DataTestMethod]
        [DataRow("2.5.7.10213")]
        [DataRow(Constants.NuGetLatestVersion)]
        [TestCategory("Rule")]
        public void FileMetadataAnalyzer_NUnit_CSharp(string nunitVersion)
        {
            var projectFiles = new[]
            {
                "TestCases/TestClassShouldHaveTestMethod.NUnit.cs",
                "TestCases/TestMethodShouldHaveCorrectSignature.NUnit.cs",
            };

            Verifier.VerifyUtilityAnalyzer<TestLocation>(
                projectFiles,
                new CSharpTestLocationAnalyzer_
                {
                    IsEnabled = true,
                    WorkingPath = @"TestLocationAnalyzer",
                },
                @"TestLocationAnalyzer\test-location.pb",
                (messages) =>
                {
                    messages.Should().HaveCount(projectFiles.Length + 1); // + ExtraEmptyFile.g.cs

                    messages[0].FilePath.Should().EndWith("ExtraEmptyFile.g.cs");
                    messages[0].TestNames.Should().BeEmpty();

                    messages[1].FilePath.Should().EndWith("TestClassShouldHaveTestMethod.NUnit.cs");
                    messages[1].TestNames.Should().OnlyContain(
                        "Tests.Diagnostics.ClassTest7.Foo",
                        "Tests.Diagnostics.ClassTest8.Foo",
                        "Tests.Diagnostics.ClassTest10.Foo",
                        "Tests.Diagnostics.ClassTest11.DivideTest",
                        "Tests.Diagnostics.TestSubFoo.Foo_WhenFoo_ExpectsFoo"
                        );

                    messages[2].FilePath.Should().EndWith("TestMethodShouldHaveCorrectSignature.NUnit.cs");
                    messages[2].TestNames.Should().OnlyContain(
                        "Tests.Diagnostics.NUnitTest.PrivateTestMethod",
                        "Tests.Diagnostics.NUnitTest.ProtectedTestMethod",
                        "Tests.Diagnostics.NUnitTest.InternalTestMethod",
                        "Tests.Diagnostics.NUnitTest.AsyncTestMethod",
                        "Tests.Diagnostics.NUnitTest.GenericTestMethod",
                        "Tests.Diagnostics.NUnitTest.ValidTest",
                        "Tests.Diagnostics.NUnitTest_TestCase.PrivateTestMethod",
                        "Tests.Diagnostics.NUnitTest_TestCase.ProtectedTestMethod",
                        "Tests.Diagnostics.NUnitTest_TestCase.InternalTestMethod",
                        "Tests.Diagnostics.NUnitTest_TestCase.AsyncTestMethod",
                        "Tests.Diagnostics.NUnitTest_TestCase.GenericTestMethod",
                        "Tests.Diagnostics.NUnitTest_TestCase.ValidTest",
                        "Tests.Diagnostics.NUnitTest_TestCaseSource.PrivateTestMethod",
                        "Tests.Diagnostics.NUnitTest_TestCaseSource.ProtectedTestMethod",
                        "Tests.Diagnostics.NUnitTest_TestCaseSource.InternalTestMethod",
                        "Tests.Diagnostics.NUnitTest_TestCaseSource.AsyncTestMethod",
                        "Tests.Diagnostics.NUnitTest_TestCaseSource.GenericTestMethod",
                        "Tests.Diagnostics.NUnitTest_TestCaseSource.ValidTest",
                        "Tests.Diagnostics.NUnitTest_Theories.PrivateTestMethod",
                        "Tests.Diagnostics.NUnitTest_Theories.ProtectedTestMethod",
                        "Tests.Diagnostics.NUnitTest_Theories.InternalTestMethod",
                        "Tests.Diagnostics.NUnitTest_Theories.AsyncTestMethod",
                        "Tests.Diagnostics.NUnitTest_Theories.GenericTestMethod",
                        "Tests.Diagnostics.NUnitTest_Theories.ValidTest"
                        );
                },
                additionalReferences: NuGetMetadataReference.NUnit(nunitVersion));
        }

        [DataTestMethod]
        [DataRow("2.0.0")]
        [DataRow(Constants.NuGetLatestVersion)]
        [TestCategory("Rule")]
        public void FileMetadataAnalyzer_XUnit_CSharp(string xunitVersion)
        {
            var projectFiles = new[]
            {
                "TestCases/TestMethodShouldHaveCorrectSignature.XUnit.cs",
            };

            Verifier.VerifyUtilityAnalyzer<TestLocation>(
                projectFiles,
                new CSharpTestLocationAnalyzer_
                {
                    IsEnabled = true,
                    WorkingPath = @"TestLocationAnalyzer",
                },
                @"TestLocationAnalyzer\test-location.pb",
                (messages) =>
                {
                    messages.Should().HaveCount(projectFiles.Length + 1); // + ExtraEmptyFile.g.cs

                    messages[0].FilePath.Should().EndWith("ExtraEmptyFile.g.cs");
                    messages[0].TestNames.Should().BeEmpty();

                    messages[1].FilePath.Should().EndWith("TestMethodShouldHaveCorrectSignature.XUnit.cs");
                    messages[1].TestNames.Should().OnlyContain(
                        "Tests.Diagnostics.XUnitTest.PrivateTestMethod",
                        "Tests.Diagnostics.XUnitTest.ProtectedTestMethod",
                        "Tests.Diagnostics.XUnitTest.InternalTestMethod",
                        "Tests.Diagnostics.XUnitTest.AsyncTestMethod",
                        "Tests.Diagnostics.XUnitTest.GenericTestMethod",
                        "Tests.Diagnostics.XUnitTest.PrivateTestMethod_Theory",
                        "Tests.Diagnostics.XUnitTest.ProtectedTestMethod_Theory",
                        "Tests.Diagnostics.XUnitTest.InternalTestMethod_Theory",
                        "Tests.Diagnostics.XUnitTest.AsyncTestMethod_Theory",
                        "Tests.Diagnostics.XUnitTest.GenericTestMethod_Theory"
                        );
                },
                additionalReferences: NuGetMetadataReference.XunitFramework(xunitVersion));
        }

        [DataTestMethod]
        [DataRow("1.1.11")]
        [DataRow(Constants.NuGetLatestVersion)]
        [TestCategory("Rule")]
        public void FileMetadataAnalyzer_MsTest_VisualBasic(string mstestVersion)
        {
            var projectFiles = new[]
            {
                "TestCases/TestClassShouldHaveTestMethod.MsTest.vb",
                "TestCases/TestMethodShouldHaveCorrectSignature.MsTest.vb",
            };

            Verifier.VerifyUtilityAnalyzer<TestLocation>(
                projectFiles,
                new VisualBasicTestLocationAnalyzer_
                {
                    IsEnabled = true,
                    WorkingPath = @"TestLocationAnalyzer",
                },
                @"TestLocationAnalyzer\test-location.pb",
                (messages) =>
                {
                    messages.Should().HaveCount(projectFiles.Length + 1); // + ExtraEmptyFile.g.vb

                    messages[0].FilePath.Should().EndWith("ExtraEmptyFile.g.vb");
                    messages[0].TestNames.Should().BeEmpty();

                    messages[1].FilePath.Should().EndWith("TestClassShouldHaveTestMethod.MsTest.vb");
                    messages[1].TestNames.Should().OnlyContain(
                        "Tests.Diagnostics.ClassTest9.Foo",
                        "Tests.Diagnostics.ClassTest12.Foo",
                        "Tests.Diagnostics.Foo_WhenFoo_ExpectsFoo.Foo_WhenFoo_ExpectsFoo"
                        );

                    messages[2].FilePath.Should().EndWith("TestMethodShouldHaveCorrectSignature.MsTest.vb");
                    messages[2].TestNames.Should().OnlyContain(
                        "Tests.Diagnostics.MsTestTest.PrivateTestMethod",
                        "Tests.Diagnostics.MsTestTest.ProtectedTestMethod",
                        "Tests.Diagnostics.MsTestTest.InternalTestMethod",
                        "Tests.Diagnostics.MsTestTest.AsyncTestMethod",
                        "Tests.Diagnostics.MsTestTest.GenericTestMethod",
                        "Tests.Diagnostics.MsTestTest.MultiErrorsMethod1",
                        "Tests.Diagnostics.MsTestTest.MultiErrorsMethod2",
                        "Tests.Diagnostics.MsTestTest.DoSomethingAsync",
                        "Tests.Diagnostics.MsTestTest_DataTestMethods.PrivateTestMethod",
                        "Tests.Diagnostics.MsTestTest_DataTestMethods.ProtectedTestMethod",
                        "Tests.Diagnostics.MsTestTest_DataTestMethods.InternalTestMethod",
                        "Tests.Diagnostics.MsTestTest_DataTestMethods.AsyncTestMethod",
                        "Tests.Diagnostics.MsTestTest_DataTestMethods.GenericTestMethod",
                        "Tests.Diagnostics.MsTestTest_DataTestMethods.MultiErrorsMethod1",
                        "Tests.Diagnostics.MsTestTest_DataTestMethods.MultiErrorsMethod2",
                        "Tests.Diagnostics.MsTestTest_DataTestMethods.DoSomethingAsync"
                        );
                },
                additionalReferences: Enumerable.Empty<MetadataReference>()
                    .Union(NuGetMetadataReference.MSTestTestFramework(mstestVersion))
                    .Union(NuGetMetadataReference.FluentAssertions(Constants.NuGetLatestVersion)));
        }

        [DataTestMethod]
        [DataRow("2.5.7.10213")]
        [DataRow(Constants.NuGetLatestVersion)]
        [TestCategory("Rule")]
        public void FileMetadataAnalyzer_NUnit_VisualBasic(string nunitVersion)
        {
            var projectFiles = new[]
            {
                "TestCases/TestClassShouldHaveTestMethod.NUnit.vb",
                "TestCases/TestMethodShouldHaveCorrectSignature.NUnit.vb",
            };

            Verifier.VerifyUtilityAnalyzer<TestLocation>(
                projectFiles,
                new VisualBasicTestLocationAnalyzer_
                {
                    IsEnabled = true,
                    WorkingPath = @"TestLocationAnalyzer",
                },
                @"TestLocationAnalyzer\test-location.pb",
                (messages) =>
                {
                    messages.Should().HaveCount(projectFiles.Length + 1); // + ExtraEmptyFile.g.vb

                    messages[0].FilePath.Should().EndWith("ExtraEmptyFile.g.vb");
                    messages[0].TestNames.Should().BeEmpty();

                    messages[1].FilePath.Should().EndWith("TestClassShouldHaveTestMethod.NUnit.vb");
                    messages[1].TestNames.Should().OnlyContain(
                        "Tests.Diagnostics.ClassTest7.Foo",
                        "Tests.Diagnostics.ClassTest8.Foo",
                        "Tests.Diagnostics.ClassTest10.Foo",
                        "Tests.Diagnostics.ClassTest11.DivideTest",
                        "Tests.Diagnostics.TestSubFoo.Foo_WhenFoo_ExpectsFoo"
                        );

                    messages[2].FilePath.Should().EndWith("TestMethodShouldHaveCorrectSignature.NUnit.vb");
                    messages[2].TestNames.Should().OnlyContain(
                        "Tests.Diagnostics.NUnitTest.PrivateTestMethod",
                        "Tests.Diagnostics.NUnitTest.ProtectedTestMethod",
                        "Tests.Diagnostics.NUnitTest.InternalTestMethod",
                        "Tests.Diagnostics.NUnitTest.AsyncTestMethod",
                        "Tests.Diagnostics.NUnitTest.GenericTestMethod",
                        "Tests.Diagnostics.NUnitTest.ValidTest",
                        "Tests.Diagnostics.NUnitTest_TestCase.PrivateTestMethod",
                        "Tests.Diagnostics.NUnitTest_TestCase.ProtectedTestMethod",
                        "Tests.Diagnostics.NUnitTest_TestCase.InternalTestMethod",
                        "Tests.Diagnostics.NUnitTest_TestCase.AsyncTestMethod",
                        "Tests.Diagnostics.NUnitTest_TestCase.GenericTestMethod",
                        "Tests.Diagnostics.NUnitTest_TestCase.ValidTest",
                        "Tests.Diagnostics.NUnitTest_TestCaseSource.PrivateTestMethod",
                        "Tests.Diagnostics.NUnitTest_TestCaseSource.ProtectedTestMethod",
                        "Tests.Diagnostics.NUnitTest_TestCaseSource.InternalTestMethod",
                        "Tests.Diagnostics.NUnitTest_TestCaseSource.AsyncTestMethod",
                        "Tests.Diagnostics.NUnitTest_TestCaseSource.GenericTestMethod",
                        "Tests.Diagnostics.NUnitTest_TestCaseSource.ValidTest",
                        "Tests.Diagnostics.NUnitTest_Theories.PrivateTestMethod",
                        "Tests.Diagnostics.NUnitTest_Theories.ProtectedTestMethod",
                        "Tests.Diagnostics.NUnitTest_Theories.InternalTestMethod",
                        "Tests.Diagnostics.NUnitTest_Theories.AsyncTestMethod",
                        "Tests.Diagnostics.NUnitTest_Theories.GenericTestMethod",
                        "Tests.Diagnostics.NUnitTest_Theories.ValidTest"
                        );
                },
                additionalReferences: NuGetMetadataReference.NUnit(nunitVersion));
        }

        [DataTestMethod]
        [DataRow("2.0.0")]
        [DataRow(Constants.NuGetLatestVersion)]
        [TestCategory("Rule")]
        public void FileMetadataAnalyzer_XUnit_VisualBasic(string xunitVersion)
        {
            var projectFiles = new[]
            {
                "TestCases/TestMethodShouldHaveCorrectSignature.XUnit.vb",
            };

            Verifier.VerifyUtilityAnalyzer<TestLocation>(
                projectFiles,
                new VisualBasicTestLocationAnalyzer_
                {
                    IsEnabled = true,
                    WorkingPath = @"TestLocationAnalyzer",
                },
                @"TestLocationAnalyzer\test-location.pb",
                (messages) =>
                {
                    messages.Should().HaveCount(projectFiles.Length + 1); // + ExtraEmptyFile.g.vb

                    messages[0].FilePath.Should().EndWith("ExtraEmptyFile.g.vb");
                    messages[0].TestNames.Should().BeEmpty();

                    messages[1].FilePath.Should().EndWith("TestMethodShouldHaveCorrectSignature.XUnit.vb");
                    messages[1].TestNames.Should().OnlyContain(
                        "Tests.Diagnostics.XUnitTest.PrivateTestMethod",
                        "Tests.Diagnostics.XUnitTest.ProtectedTestMethod",
                        "Tests.Diagnostics.XUnitTest.InternalTestMethod",
                        "Tests.Diagnostics.XUnitTest.AsyncTestMethod",
                        "Tests.Diagnostics.XUnitTest.GenericTestMethod",
                        "Tests.Diagnostics.XUnitTest.PrivateTestMethod_Theory",
                        "Tests.Diagnostics.XUnitTest.ProtectedTestMethod_Theory",
                        "Tests.Diagnostics.XUnitTest.InternalTestMethod_Theory",
                        "Tests.Diagnostics.XUnitTest.AsyncTestMethod_Theory",
                        "Tests.Diagnostics.XUnitTest.GenericTestMethod_Theory"
                        );
                },
                additionalReferences: NuGetMetadataReference.XunitFramework(xunitVersion));
        }

        // We need to set protected properties and this class exists just to enable the analyzer
        // without bothering with additional files with parameters
        private class CSharpTestLocationAnalyzer_ : CSharpTestLocationAnalyzer
        {
            public bool IsEnabled
            {
                get => IsAnalyzerEnabled;
                set => IsAnalyzerEnabled = value;
            }

            public string WorkingPath
            {
                get => WorkDirectoryBasePath;
                set => WorkDirectoryBasePath = value;
            }
        }
        private class VisualBasicTestLocationAnalyzer_ : VisualBasicTestLocationAnalyzer
        {
            public bool IsEnabled
            {
                get => IsAnalyzerEnabled;
                set => IsAnalyzerEnabled = value;
            }

            public string WorkingPath
            {
                get => WorkDirectoryBasePath;
                set => WorkDirectoryBasePath = value;
            }
        }
    }
}
