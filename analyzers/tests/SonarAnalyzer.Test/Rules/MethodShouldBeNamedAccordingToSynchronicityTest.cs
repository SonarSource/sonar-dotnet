/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2024 SonarSource SA
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

using SonarAnalyzer.Rules.CSharp;

namespace SonarAnalyzer.Test.Rules
{
    [TestClass]
    public class MethodShouldBeNamedAccordingToSynchronicityTest
    {
        private readonly VerifierBuilder builder = new VerifierBuilder<MethodShouldBeNamedAccordingToSynchronicity>();

        [TestMethod]
        [DataRow("4.0.0")]
        [DataRow(Constants.NuGetLatestVersion)]
        public void MethodShouldBeNamedAccordingToSynchronicity(string tasksVersion) =>
            builder.AddPaths("MethodShouldBeNamedAccordingToSynchronicity.cs")
                .AddReferences(MetadataReferenceFacade.SystemThreadingTasksExtensions(tasksVersion)
                    .Union(NuGetMetadataReference.MicrosoftAspNetSignalRCore())
                    .Union(MetadataReferenceFacade.SystemComponentModelPrimitives))
                .Verify();

        [TestMethod]
        [DataRow("3.0.20105.1")]
        [DataRow(Constants.NuGetLatestVersion)]
        public void MethodShouldBeNamedAccordingToSynchronicity_MVC(string mvcVersion) =>
            builder.AddPaths("MethodShouldBeNamedAccordingToSynchronicity.MVC.cs").AddReferences(NuGetMetadataReference.MicrosoftAspNetMvc(mvcVersion))
                .VerifyNoIssues();   // rule does not apply to MVC

        [TestMethod]
        [DataRow("2.0.4", "2.0.3")]
        [DataRow(Constants.NuGetLatestVersion, Constants.NuGetLatestVersion)]
        public void MethodShouldBeNamedAccordingToSynchronicity_MVC_Core(string aspNetCoreMvcVersion, string aspNetCoreRoutingVersion) =>
            builder.AddPaths("MethodShouldBeNamedAccordingToSynchronicity.MVC.Core.cs")
                .AddReferences(NuGetMetadataReference.MicrosoftAspNetCoreMvcCore(aspNetCoreMvcVersion)
                    .Concat(NuGetMetadataReference.MicrosoftAspNetCoreMvcViewFeatures(aspNetCoreMvcVersion))
                    .Concat(NuGetMetadataReference.MicrosoftAspNetCoreRoutingAbstractions(aspNetCoreRoutingVersion)))
                .Verify();

        [DataTestMethod]
        [DataRow("1.1.11")]
        [DataRow(Constants.NuGetLatestVersion)]
        public void MethodShouldBeNamedAccordingToSynchronicity_MsTest(string testFwkVersion) =>
            builder.AddPaths("MethodShouldBeNamedAccordingToSynchronicity.MsTest.cs").AddReferences(NuGetMetadataReference.MSTestTestFramework(testFwkVersion))
                .VerifyNoIssues();   // rule does not apply to MsTest

        [DataTestMethod]
        [DataRow("2.5.7.10213")]
        [DataRow(Constants.NuGetLatestVersion)]
        public void MethodShouldBeNamedAccordingToSynchronicity_NUnit(string testFwkVersion) =>
            builder.AddPaths("MethodShouldBeNamedAccordingToSynchronicity.NUnit.cs").AddReferences(NuGetMetadataReference.NUnit(testFwkVersion))
                .VerifyNoIssues();   // rule does not apply to NUnit

        [DataTestMethod]
        [DataRow("2.0.0")]
        [DataRow(Constants.NuGetLatestVersion)]
        public void MethodShouldBeNamedAccordingToSynchronicity_Xunit(string testFwkVersion) =>
            builder.AddPaths("MethodShouldBeNamedAccordingToSynchronicity.Xunit.cs").AddReferences(NuGetMetadataReference.XunitFramework(testFwkVersion))
                .VerifyNoIssues();   // rule does not apply to Xunit

        [TestMethod]
        public void MethodShouldBeNamedAccordingToSynchronicity_CSharp8() =>
            builder.AddPaths("MethodShouldBeNamedAccordingToSynchronicity.CSharp8.cs").WithOptions(ParseOptionsHelper.FromCSharp8).AddReferences(MetadataReferenceFacade.NetStandard21).Verify();

#if NET

        [TestMethod]
        public void MethodShouldBeNamedAccordingToSynchronicity_CSharp11() =>
            builder.AddPaths("MethodShouldBeNamedAccordingToSynchronicity.CSharp11.cs")
                .WithOptions(ParseOptionsHelper.FromCSharp11)
                .AddReferences(NuGetMetadataReference.MicrosoftAspNetSignalRCore())
                .Verify();

#endif
    }
}
