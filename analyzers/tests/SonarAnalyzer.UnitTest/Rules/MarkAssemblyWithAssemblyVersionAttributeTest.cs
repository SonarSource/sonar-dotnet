/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2022 SonarSource SA
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

using System;
using System.Collections.Generic;
using FluentAssertions;
using Microsoft.CodeAnalysis;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SonarAnalyzer.UnitTest.MetadataReferences;
using SonarAnalyzer.UnitTest.TestFramework;
using CS = SonarAnalyzer.Rules.CSharp;
using VB = SonarAnalyzer.Rules.VisualBasic;

namespace SonarAnalyzer.UnitTest.Rules
{
    [TestClass]
    public class MarkAssemblyWithAssemblyVersionAttributeTest
    {
        private readonly VerifierBuilder builderCS = new VerifierBuilder<CS.MarkAssemblyWithAssemblyVersionAttribute>();
        private readonly VerifierBuilder builderVB = new VerifierBuilder<VB.MarkAssemblyWithAssemblyVersionAttribute>();

        [TestMethod]
        public void MarkAssemblyWithAssemblyVersionAttribute_CS() =>
            builderCS.AddPaths("MarkAssemblyWithAssemblyVersionAttribute.cs").WithConcurrentAnalysis(false).Verify();

        [TestMethod]
        public void MarkAssemblyWithAssemblyVersionAttributeRazor_CS() =>
            builderCS
                .AddPaths("MarkAssemblyWithAssemblyVersionAttributeRazor.cs")
                .WithConcurrentAnalysis(false)
                .AddReferences(GetAspNetCoreRazorReferences())
                .Verify();

        [TestMethod]
        public void MarkAssemblyWithAssemblyVersionAttribute_CS_Concurrent() =>
            builderCS
                .AddPaths("MarkAssemblyWithAssemblyVersionAttribute.cs", "MarkAssemblyWithAssemblyVersionAttributeRazor.cs")
                .AddReferences(GetAspNetCoreRazorReferences())
                .WithAutogenerateConcurrentFiles(false)
                .Verify();

        [TestMethod]
        public void MarkAssemblyWithAssemblyVersionAttributeNoncompliant_CS() =>
            builderCS.AddPaths("MarkAssemblyWithAssemblyVersionAttributeNoncompliant.cs")
                .WithConcurrentAnalysis(false)
                .Invoking(x => x.Verify())
                .Should().Throw<UnexpectedDiagnosticException>().WithMessage("*Provide an 'AssemblyVersion' attribute for assembly 'project0'.*");

        [TestMethod]
        public void MarkAssemblyWithAssemblyVersionAttributeNoncompliant_NoTargets_ShouldNotRaise_CS() =>
            builderCS.AddPaths("MarkAssemblyWithAssemblyVersionAttributeNoncompliant.cs")
                .AddReferences(NuGetMetadataReference.MicrosoftBuildNoTargets())
                .WithConcurrentAnalysis(false)
                .Invoking(x => x.Verify())
                // False positive. No assembly gets generated when Microsoft.Build.NoTargets is referenced.
                .Should().Throw<UnexpectedDiagnosticException>().WithMessage("*Provide an 'AssemblyVersion' attribute for assembly 'project0'.*");

        [TestMethod]
        public void MarkAssemblyWithAssemblyVersionAttribute_VB() =>
            builderVB.AddPaths("MarkAssemblyWithAssemblyVersionAttribute.vb").WithConcurrentAnalysis(false).Verify();

        [TestMethod]
        public void MarkAssemblyWithAssemblyVersionAttributeRazor_VB() =>
            builderVB
                .AddPaths("MarkAssemblyWithAssemblyVersionAttributeRazor.vb")
                .WithConcurrentAnalysis(false)
                .AddReferences(GetAspNetCoreRazorReferences())
                .Verify();

        [TestMethod]
        public void MarkAssemblyWithAssemblyVersionAttribute_VB_Concurrent() =>
            builderVB
                .AddPaths("MarkAssemblyWithAssemblyVersionAttribute.vb", "MarkAssemblyWithAssemblyVersionAttributeRazor.vb")
                .AddReferences(GetAspNetCoreRazorReferences())
                .WithAutogenerateConcurrentFiles(false)
                .Verify();

        [TestMethod]
        public void MarkAssemblyWithAssemblyVersionAttributeNoncompliant_VB() =>
            builderVB.AddPaths("MarkAssemblyWithAssemblyVersionAttributeNoncompliant.vb")
                .WithConcurrentAnalysis(false)
                .Invoking(x => x.Verify())
                .Should().Throw<UnexpectedDiagnosticException>().WithMessage("*Provide an 'AssemblyVersion' attribute for assembly 'project0'.*");

        [TestMethod]
        public void MarkAssemblyWithAssemblyVersionAttributeNoncompliant_NoTargets_ShouldNotRaise_VB() =>
            builderVB.AddPaths("MarkAssemblyWithAssemblyVersionAttributeNoncompliant.vb")
                .AddReferences(NuGetMetadataReference.MicrosoftBuildNoTargets())
                .WithConcurrentAnalysis(false)
                .Invoking(x => x.Verify())
                // False positive. No assembly gets generated when Microsoft.Build.NoTargets is referenced.
                .Should().Throw<UnexpectedDiagnosticException>().WithMessage("*Provide an 'AssemblyVersion' attribute for assembly 'project0'.*");

        private static IEnumerable<MetadataReference> GetAspNetCoreRazorReferences() =>
            NuGetMetadataReference.MicrosoftAspNetCoreMvcRazorRuntime(Constants.NuGetLatestVersion);
    }
}
