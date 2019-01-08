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
    public class MethodShouldBeNamedAccordingToSynchronicityTest
    {
        [TestMethod]
        [DataRow("4.0.0")]
        [DataRow(Constants.NuGetLatestVersion)]
        [TestCategory("Rule")]
        public void MethodShouldBeNamedAccordingToSynchronicity(string tasksVersion)
        {
            Verifier.VerifyAnalyzer(@"TestCases\MethodShouldBeNamedAccordingToSynchronicity.cs",
                new MethodShouldBeNamedAccordingToSynchronicity(),
                additionalReferences: NuGetMetadataReference.SystemThreadingTasksExtensions(tasksVersion));
        }

        [TestMethod]
        [DataRow("3.0.20105.1")]
        [DataRow(Constants.NuGetLatestVersion)]
        [TestCategory("Rule")]
        public void MethodShouldBeNamedAccordingToSynchronicity_MVC(string mvcVersion)
        {
            Verifier.VerifyAnalyzer(@"TestCases\MethodShouldBeNamedAccordingToSynchronicity.MVC.cs",
                new MethodShouldBeNamedAccordingToSynchronicity(),
                additionalReferences: NuGetMetadataReference.MicrosoftAspNetMvc(mvcVersion));
        }

        [TestMethod]
        [DataRow("2.0.4", "2.0.3")]
        [DataRow(Constants.NuGetLatestVersion, Constants.NuGetLatestVersion)]
        [TestCategory("Rule")]
        public void MethodShouldBeNamedAccordingToSynchronicity_MVC_Core(string aspNetCoreMvcVersion,
            string aspNetCoreRoutingVersion)
        {
            Verifier.VerifyAnalyzer(@"TestCases\MethodShouldBeNamedAccordingToSynchronicity.MVC.Core.cs",
                new MethodShouldBeNamedAccordingToSynchronicity(),
                additionalReferences: FrameworkMetadataReference.Netstandard
                    .Concat(NuGetMetadataReference.MicrosoftAspNetCoreMvcCore(aspNetCoreMvcVersion))
                    .Concat(NuGetMetadataReference.MicrosoftAspNetCoreMvcViewFeatures(aspNetCoreMvcVersion))
                    .Concat(NuGetMetadataReference.MicrosoftAspNetCoreRoutingAbstractions(aspNetCoreRoutingVersion)));
        }

        [DataTestMethod]
        [DataRow("1.1.11")]
        [DataRow(Constants.NuGetLatestVersion)]
        [TestCategory("Rule")]
        public void MethodShouldBeNamedAccordingToSynchronicity_MsTest(string testFwkVersion)
        {
            Verifier.VerifyAnalyzer(@"TestCases\MethodShouldBeNamedAccordingToSynchronicity.MsTest.cs",
                new MethodShouldBeNamedAccordingToSynchronicity(),
                additionalReferences: NuGetMetadataReference.MSTestTestFramework(testFwkVersion)
                    .Concat(NuGetMetadataReference.SystemThreadingTasksExtensions("4.0.0")));
        }

        [DataTestMethod]
        [DataRow("2.5.7.10213")]
        [DataRow(Constants.NuGetLatestVersion)]
        [TestCategory("Rule")]
        public void MethodShouldBeNamedAccordingToSynchronicity_NUnit(string testFwkVersion)
        {
            Verifier.VerifyAnalyzer(@"TestCases\MethodShouldBeNamedAccordingToSynchronicity.NUnit.cs",
                new MethodShouldBeNamedAccordingToSynchronicity(),
                additionalReferences: NuGetMetadataReference.NUnit(testFwkVersion)
                    .Concat(NuGetMetadataReference.SystemThreadingTasksExtensions("4.0.0")));
        }

        [DataTestMethod]
        [DataRow("2.0.0")]
        [DataRow(Constants.NuGetLatestVersion)]
        [TestCategory("Rule")]
        public void MethodShouldBeNamedAccordingToSynchronicity_Xunit(string testFwkVersion)
        {
            Verifier.VerifyAnalyzer(@"TestCases\MethodShouldBeNamedAccordingToSynchronicity.Xunit.cs",
                new MethodShouldBeNamedAccordingToSynchronicity(),
                additionalReferences: NuGetMetadataReference.XunitFramework(testFwkVersion)
                    .Concat(NuGetMetadataReference.SystemThreadingTasksExtensions("4.0.0")));
        }
    }
}
