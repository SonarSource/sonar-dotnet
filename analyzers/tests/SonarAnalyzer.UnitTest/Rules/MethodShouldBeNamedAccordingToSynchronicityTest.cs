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
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SonarAnalyzer.Rules.CSharp;
using SonarAnalyzer.UnitTest.MetadataReferences;
using SonarAnalyzer.UnitTest.TestFramework;

namespace SonarAnalyzer.UnitTest.Rules
{
    [TestClass]
    public class MethodShouldBeNamedAccordingToSynchronicityTest
    {
        [TestMethod]
        [DataRow("4.0.0")]
        [DataRow(Constants.NuGetLatestVersion)]
        [TestCategory("Rule")]
        public void MethodShouldBeNamedAccordingToSynchronicity(string tasksVersion) =>
            Verifier.VerifyAnalyzer(@"TestCases\MethodShouldBeNamedAccordingToSynchronicity.cs",
                                              new MethodShouldBeNamedAccordingToSynchronicity(),
                                              MetadataReferenceFacade.SystemThreadingTasksExtensions(tasksVersion)
                                                                   .Union(MetadataReferenceFacade.SystemComponentModelPrimitives)
                                                                   .Union(NuGetMetadataReference.MicrosoftAspNetSignalRCore()));

        [TestMethod]
        [DataRow("3.0.20105.1")]
        [DataRow(Constants.NuGetLatestVersion)]
        [TestCategory("Rule")]
        public void MethodShouldBeNamedAccordingToSynchronicity_MVC(string mvcVersion) =>
            Verifier.VerifyNonConcurrentAnalyzer(@"TestCases\MethodShouldBeNamedAccordingToSynchronicity.MVC.cs",
                new MethodShouldBeNamedAccordingToSynchronicity(),
                NuGetMetadataReference.MicrosoftAspNetMvc(mvcVersion));

        [TestMethod]
        [DataRow("2.0.4", "2.0.3")]
        [DataRow(Constants.NuGetLatestVersion, Constants.NuGetLatestVersion)]
        [TestCategory("Rule")]
        public void MethodShouldBeNamedAccordingToSynchronicity_MVC_Core(string aspNetCoreMvcVersion,
            string aspNetCoreRoutingVersion) =>
            Verifier.VerifyNonConcurrentAnalyzer(@"TestCases\MethodShouldBeNamedAccordingToSynchronicity.MVC.Core.cs",
                new MethodShouldBeNamedAccordingToSynchronicity(),
                NetStandardMetadataReference.Netstandard
                    .Concat(NuGetMetadataReference.MicrosoftAspNetCoreMvcCore(aspNetCoreMvcVersion))
                    .Concat(NuGetMetadataReference.MicrosoftAspNetCoreMvcViewFeatures(aspNetCoreMvcVersion))
                    .Concat(NuGetMetadataReference.MicrosoftAspNetCoreRoutingAbstractions(aspNetCoreRoutingVersion)));

        [DataTestMethod]
        [DataRow("1.1.11")]
        [DataRow(Constants.NuGetLatestVersion)]
        [TestCategory("Rule")]
        public void MethodShouldBeNamedAccordingToSynchronicity_MsTest(string testFwkVersion) =>
            Verifier.VerifyNonConcurrentAnalyzer(@"TestCases\MethodShouldBeNamedAccordingToSynchronicity.MsTest.cs",
                new MethodShouldBeNamedAccordingToSynchronicity(),
                NuGetMetadataReference.MSTestTestFramework(testFwkVersion)
                    .Concat(NuGetMetadataReference.SystemThreadingTasksExtensions("4.0.0")));

        [DataTestMethod]
        [DataRow("2.5.7.10213")]
        [DataRow(Constants.NuGetLatestVersion)]
        [TestCategory("Rule")]
        public void MethodShouldBeNamedAccordingToSynchronicity_NUnit(string testFwkVersion) =>
            Verifier.VerifyNonConcurrentAnalyzer(@"TestCases\MethodShouldBeNamedAccordingToSynchronicity.NUnit.cs",
                new MethodShouldBeNamedAccordingToSynchronicity(),
                NuGetMetadataReference.NUnit(testFwkVersion)
                    .Concat(NuGetMetadataReference.SystemThreadingTasksExtensions("4.0.0")));

        [DataTestMethod]
        [DataRow("2.0.0")]
        [DataRow(Constants.NuGetLatestVersion)]
        [TestCategory("Rule")]
        public void MethodShouldBeNamedAccordingToSynchronicity_Xunit(string testFwkVersion) =>
            Verifier.VerifyNonConcurrentAnalyzer(@"TestCases\MethodShouldBeNamedAccordingToSynchronicity.Xunit.cs",
                new MethodShouldBeNamedAccordingToSynchronicity(),
                NuGetMetadataReference.XunitFramework(testFwkVersion)
                    .Concat(NuGetMetadataReference.SystemThreadingTasksExtensions("4.0.0")));

        [TestCategory("Rule")]
        [TestMethod]
        public void MethodShouldBeNamedAccordingToSynchronicity_CSharp8() =>
            Verifier.VerifyNonConcurrentAnalyzer(@"TestCases\MethodShouldBeNamedAccordingToSynchronicity.CSharp8.cs",
                                    new MethodShouldBeNamedAccordingToSynchronicity(),
#if NETFRAMEWORK
                                    ParseOptionsHelper.FromCSharp8,
                                    NuGetMetadataReference.NETStandardV2_1_0);
#else
                                    ParseOptionsHelper.FromCSharp8);
#endif
    }
}
