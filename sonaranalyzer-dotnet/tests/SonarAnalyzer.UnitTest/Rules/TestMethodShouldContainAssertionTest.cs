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

using System.Linq;
using csharp::SonarAnalyzer.Rules.CSharp;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace SonarAnalyzer.UnitTest.Rules
{
    [TestClass]
    public class TestMethodShouldContainAssertionTest
    {
        [DataTestMethod]
        [DataRow("1.1.11", "1.6.0")]
        [DataRow(Constants.NuGetLatestVersion, "4.19.4")]
        [TestCategory("Rule")]
        public void TestMethodShouldContainAssertion_MSTest(string testFwkVersion, string fluentVersion)
        {
            Verifier.VerifyAnalyzer(@"TestCases\TestMethodShouldContainAssertion.MsTest.cs",
                new TestMethodShouldContainAssertion(),
                additionalReferences: NuGetMetadataReference.MSTestTestFramework(testFwkVersion)
                    .Concat(NuGetMetadataReference.FluentAssertions(fluentVersion))
                    .Concat(FrameworkMetadataReference.SystemReflection)
                    .Concat(FrameworkMetadataReference.SystemXmlXDocument)
                    .Concat(FrameworkMetadataReference.SystemXmlLinq)
                    .Concat(FrameworkMetadataReference.SystemThreadingTasks)
                    .ToArray());
        }

        [DataTestMethod]
        [DataRow("2.5.7.10213", "4.19.4")]
        [DataRow(Constants.NuGetLatestVersion, "4.19.4")]
        [TestCategory("Rule")]
        public void TestMethodShouldContainAssertion_NUnit(string testFwkVersion, string fluentVersion)
        {
            Verifier.VerifyAnalyzer(@"TestCases\TestMethodShouldContainAssertion.NUnit.cs",
                new TestMethodShouldContainAssertion(),
                additionalReferences: NuGetMetadataReference.NUnit(testFwkVersion)
                    .Concat(NuGetMetadataReference.FluentAssertions(fluentVersion))
                    .Concat(FrameworkMetadataReference.SystemReflection)
                    .Concat(FrameworkMetadataReference.SystemXmlXDocument)
                    .Concat(FrameworkMetadataReference.SystemXmlLinq)
                    .Concat(FrameworkMetadataReference.SystemThreadingTasks)
                    .ToArray());
        }

        [DataTestMethod]
        [DataRow("2.5.7.10213", "4.19.4")]
        [DataRow("2.7.0", "4.19.4")]
        [TestCategory("Rule")]
        public void TestMethodShouldContainAssertion_NUnit_V2Specific(string testFwkVersion, string fluentVersion)
        {
            Verifier.VerifyCSharpAnalyzer(@"
using System;
using FluentAssertions;
using NUnit.Framework;

[TestFixture]
public class Foo
{
    [TestCase]
    [ExpectedException(typeof(Exception))]
    public void TestCase4()
    {
        var x = System.IO.File.Open("""", System.IO.FileMode.Open);
    }

    [Theory]
    [ExpectedException(typeof(Exception))]
    public void Theory4()
    {
        var x = System.IO.File.Open("""", System.IO.FileMode.Open);
    }

    [TestCaseSource(""Foo"")]
    [ExpectedException(typeof(Exception))]
    public void TestCaseSource4()
    {
        var x = System.IO.File.Open("""", System.IO.FileMode.Open);
    }

    [Test]
    [ExpectedException(typeof(Exception))]
    public void Test4()
    {
        var x = System.IO.File.Open("""", System.IO.FileMode.Open);
    }
}",
                new TestMethodShouldContainAssertion(),
                additionalReferences: NuGetMetadataReference.NUnit(testFwkVersion)
                    .Concat(NuGetMetadataReference.FluentAssertions(fluentVersion))
                    .Concat(FrameworkMetadataReference.SystemReflection)
                    .Concat(FrameworkMetadataReference.SystemXmlXDocument)
                    .Concat(FrameworkMetadataReference.SystemXmlLinq)
                    .Concat(FrameworkMetadataReference.SystemThreadingTasks)
                    .ToArray());
        }

        [DataTestMethod]
        [DataRow("2.0.0", "4.19.4")]
        [DataRow(Constants.NuGetLatestVersion, "4.19.4")]
        [TestCategory("Rule")]
        public void TestMethodShouldContainAssertion_Xunit(string testFwkVersion, string fluentVersion)
        {
            Verifier.VerifyAnalyzer(@"TestCases\TestMethodShouldContainAssertion.Xunit.cs",
                new TestMethodShouldContainAssertion(),
                additionalReferences: NuGetMetadataReference.XunitFramework(testFwkVersion)
                    .Concat(NuGetMetadataReference.FluentAssertions(fluentVersion))
                    .Concat(FrameworkMetadataReference.SystemReflection)
                    .Concat(FrameworkMetadataReference.SystemXmlXDocument)
                    .Concat(FrameworkMetadataReference.SystemXmlLinq)
                    .Concat(FrameworkMetadataReference.SystemThreadingTasks)
                    .ToArray());
        }

        [TestMethod]
        [TestCategory("Rule")]
        public void TestMethodShouldContainAssertion_Xunit_Legacy()
        {
            Verifier.VerifyAnalyzer(@"TestCases\TestMethodShouldContainAssertion.Xunit.Legacy.cs",
                new TestMethodShouldContainAssertion(),
                additionalReferences: NuGetMetadataReference.XunitFrameworkV1
                    .Concat(NuGetMetadataReference.FluentAssertions("1.6.0"))
                    .ToArray());
        }
    }
}
