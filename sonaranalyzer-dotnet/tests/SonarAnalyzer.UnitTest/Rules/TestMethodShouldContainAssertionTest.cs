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
using csharp::SonarAnalyzer.Rules.CSharp;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace SonarAnalyzer.UnitTest.Rules
{
    [TestClass]
    public class TestMethodShouldContainAssertionTest
    {
        [TestMethod]
        [TestCategory("Rule")]
        public void TestMethodShouldContainAssertion_MSTest()
        {
            Verifier.VerifyAnalyzer(@"TestCases\TestMethodShouldContainAssertion_MSTest.cs",
                new TestMethodShouldContainAssertion(),
                null,
                AssemblyReference.FromNuGet("Microsoft.VisualStudio.TestPlatform.TestFramework.dll", "MSTest.TestFramework", "1.2.0"),
                AssemblyReference.FromNuGet("FluentAssertions.dll", "FluentAssertions", "4.19.4"),
                AssemblyReference.FromNuGet("FluentAssertions.Core.dll", "FluentAssertions", "4.19.4"));
        }

        [TestMethod]
        [TestCategory("Rule")]
        public void TestMethodShouldContainAssertion_NUnit()
        {
            Verifier.VerifyAnalyzer(@"TestCases\TestMethodShouldContainAssertion_NUnit.cs",
                new TestMethodShouldContainAssertion(),
                null,
                AssemblyReference.FromNuGet("nunit.framework.dll", "NUnit", "2.6.4"),
                AssemblyReference.FromNuGet("FluentAssertions.dll", "FluentAssertions", "4.19.4"),
                AssemblyReference.FromNuGet("FluentAssertions.Core.dll", "FluentAssertions", "4.19.4"));
        }

        [TestMethod]
        [TestCategory("Rule")]
        public void TestMethodShouldContainAssertion_Xunit()
        {
            Verifier.VerifyAnalyzer(@"TestCases\TestMethodShouldContainAssertion_Xunit.cs",
                new TestMethodShouldContainAssertion(),
                null,
                AssemblyReference.FromNuGet("xunit.assert.dll", "xunit.assert", "2.2.0"),
                AssemblyReference.FromNuGet("xunit.core.dll", "xunit.extensibility.core", "2.2.0"),
                AssemblyReference.FromNuGet("FluentAssertions.dll", "FluentAssertions", "4.19.4"),
                AssemblyReference.FromNuGet("FluentAssertions.Core.dll", "FluentAssertions", "4.19.4"));
        }
    }
}
