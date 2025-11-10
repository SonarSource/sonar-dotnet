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

using SonarAnalyzer.CSharp.Rules;
using static SonarAnalyzer.TestFramework.MetadataReferences.NugetPackageVersions;

namespace SonarAnalyzer.Test.Rules;

[TestClass]
public class MethodShouldBeNamedAccordingToSynchronicityTest
{
    private readonly VerifierBuilder builder = new VerifierBuilder<MethodShouldBeNamedAccordingToSynchronicity>();

    [TestMethod]
    [DataRow("4.0.0")]
    [DataRow(TestConstants.NuGetLatestVersion)]
    public void MethodShouldBeNamedAccordingToSynchronicity(string tasksVersion) =>
        builder.AddPaths("MethodShouldBeNamedAccordingToSynchronicity.cs")
            .AddReferences(MetadataReferenceFacade.SystemThreadingTasksExtensions(tasksVersion)
                .Union(NuGetMetadataReference.MicrosoftAspNetSignalRCore())
                .Union(MetadataReferenceFacade.SystemComponentModelPrimitives))
            .Verify();

    [TestMethod]
    [DataRow("3.0.20105.1")]
    [DataRow(TestConstants.NuGetLatestVersion)]
    public void MethodShouldBeNamedAccordingToSynchronicity_MVC(string mvcVersion) =>
        builder.AddPaths("MethodShouldBeNamedAccordingToSynchronicity.MVC.cs").AddReferences(NuGetMetadataReference.MicrosoftAspNetMvc(mvcVersion)).VerifyNoIssues();

    [TestMethod]
    [DataRow("2.0.4", "2.0.3")]
    [DataRow(TestConstants.NuGetLatestVersion, TestConstants.NuGetLatestVersion)]
    public void MethodShouldBeNamedAccordingToSynchronicity_MVC_Core(string aspNetCoreMvcVersion, string aspNetCoreRoutingVersion) =>
        builder.AddPaths("MethodShouldBeNamedAccordingToSynchronicity.MVC.Core.cs")
            .AddReferences(NuGetMetadataReference.MicrosoftAspNetCoreMvcCore(aspNetCoreMvcVersion)
                .Concat(NuGetMetadataReference.MicrosoftAspNetCoreMvcViewFeatures(aspNetCoreMvcVersion))
                .Concat(NuGetMetadataReference.MicrosoftAspNetCoreRoutingAbstractions(aspNetCoreRoutingVersion)))
            .Verify();

    [TestMethod]
    [DataRow(MsTest.Ver1_1)]
    [DataRow(MsTest.Ver3)]
    [DataRow(Latest)]
    public void MethodShouldBeNamedAccordingToSynchronicity_MsTest(string testFwkVersion) =>
        builder.AddPaths("MethodShouldBeNamedAccordingToSynchronicity.MsTest.cs").AddReferences(NuGetMetadataReference.MSTestTestFramework(testFwkVersion)).VerifyNoIssues();

    [TestMethod]
    [DataRow(NUnit.Ver25)]
    [DataRow(Latest)]
    public void MethodShouldBeNamedAccordingToSynchronicity_NUnit(string testFwkVersion) =>
        builder.AddPaths("MethodShouldBeNamedAccordingToSynchronicity.NUnit.cs").AddReferences(NuGetMetadataReference.NUnit(testFwkVersion)).VerifyNoIssues();

    [TestMethod]
    [DataRow("2.0.0")]
    [DataRow(TestConstants.NuGetLatestVersion)]
    public void MethodShouldBeNamedAccordingToSynchronicity_Xunit(string testFwkVersion) =>
        builder.AddPaths("MethodShouldBeNamedAccordingToSynchronicity.Xunit.cs").AddReferences(NuGetMetadataReference.XunitFramework(testFwkVersion)).VerifyNoIssues();

    [TestMethod]
    public void MethodShouldBeNamedAccordingToSynchronicity_CSharp8() =>
        builder.AddPaths("MethodShouldBeNamedAccordingToSynchronicity.CSharp8.cs").WithOptions(LanguageOptions.FromCSharp8).AddReferences(MetadataReferenceFacade.NetStandard21).Verify();

#if NET

    [TestMethod]
    public void MethodShouldBeNamedAccordingToSynchronicity_CSharp11() =>
        builder.AddPaths("MethodShouldBeNamedAccordingToSynchronicity.CSharp11.cs")
            .WithOptions(LanguageOptions.FromCSharp11)
            .AddReferences(NuGetMetadataReference.MicrosoftAspNetSignalRCore())
            .Verify();

#endif
}
