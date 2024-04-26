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

using ChecksCS = SonarAnalyzer.SymbolicExecution.Roslyn.RuleChecks.CSharp;
using ChecksVB = SonarAnalyzer.SymbolicExecution.Roslyn.RuleChecks.VisualBasic;
using CS = SonarAnalyzer.Rules.CSharp;
using VB = SonarAnalyzer.Rules.VisualBasic;

namespace SonarAnalyzer.Test.Rules
{
    [TestClass]
    public class PublicMethodArgumentsShouldBeCheckedForNullTest
    {
        public TestContext TestContext { get; set; }

        private readonly VerifierBuilder sonar = new VerifierBuilder()
            .AddAnalyzer(() => new CS.SymbolicExecutionRunner(AnalyzerConfiguration.AlwaysEnabledWithSonarCfg))
            .WithBasePath(@"SymbolicExecution\Sonar")
            .WithOnlyDiagnostics(ChecksCS.PublicMethodArgumentsShouldBeCheckedForNull.S3900);

        private readonly VerifierBuilder roslynCS = new VerifierBuilder()
            .AddAnalyzer(() => new CS.SymbolicExecutionRunner(AnalyzerConfiguration.AlwaysEnabled))
            .WithBasePath(@"SymbolicExecution\Roslyn")
            .WithOnlyDiagnostics(ChecksCS.PublicMethodArgumentsShouldBeCheckedForNull.S3900);

        private readonly VerifierBuilder roslynVB = new VerifierBuilder()
            .AddAnalyzer(() => new VB.SymbolicExecutionRunner())
            .WithBasePath(@"SymbolicExecution\Roslyn")
            .WithOnlyDiagnostics(ChecksVB.PublicMethodArgumentsShouldBeCheckedForNull.S3900);

        [DataTestMethod]
        [DataRow(ProjectType.Product)]
        [DataRow(ProjectType.Test)]
        public void PublicMethodArgumentsShouldBeCheckedForNull_Sonar_CS(ProjectType projectType) =>
            sonar.AddReferences(TestHelper.ProjectTypeReference(projectType))
                .AddPaths("PublicMethodArgumentsShouldBeCheckedForNull.cs")
                .Verify();

        [DataTestMethod]
        [DataRow(ProjectType.Product)]
        [DataRow(ProjectType.Test)]
        public void PublicMethodArgumentsShouldBeCheckedForNull_Roslyn_CS(ProjectType projectType) =>
            roslynCS.AddReferences(TestHelper.ProjectTypeReference(projectType))
                .AddPaths("PublicMethodArgumentsShouldBeCheckedForNull.cs")
                .Verify();

        [DataTestMethod]
        [DataRow(ProjectType.Product)]
        [DataRow(ProjectType.Test)]
        public void PublicMethodArgumentsShouldBeCheckedForNull_Roslyn_VB(ProjectType projectType) =>
            roslynVB.AddReferences(TestHelper.ProjectTypeReference(projectType))
                .AddPaths("PublicMethodArgumentsShouldBeCheckedForNull.vb")
                .WithOptions(ParseOptionsHelper.FromVisualBasic14)
                .Verify();

#if NET

        [TestMethod]
        public void PublicMethodArgumentsShouldBeCheckedForNull_Sonar_CSharp8() =>
            sonar.AddPaths("PublicMethodArgumentsShouldBeCheckedForNull.CSharp8.cs")
                .WithOptions(ParseOptionsHelper.FromCSharp8)
                .Verify();

        [TestMethod]
        public void PublicMethodArgumentsShouldBeCheckedForNull_Roslyn_CSharp8() =>
            roslynCS.AddPaths("PublicMethodArgumentsShouldBeCheckedForNull.CSharp8.cs")
                .WithOptions(ParseOptionsHelper.FromCSharp8)
                .Verify();

        [TestMethod]
        public void PublicMethodArgumentsShouldBeCheckedForNull_Sonar_CSharp9() =>
            sonar.AddPaths("PublicMethodArgumentsShouldBeCheckedForNull.CSharp9.cs")
                .AddReferences(NuGetMetadataReference.MicrosoftAspNetCoreMvcCore(Constants.NuGetLatestVersion))
                .WithOptions(ParseOptionsHelper.FromCSharp9)
                .Verify();

        [TestMethod]
        public void PublicMethodArgumentsShouldBeCheckedForNull_Roslyn_CSharp9() =>
            roslynCS.AddPaths("PublicMethodArgumentsShouldBeCheckedForNull.CSharp9.cs")
                .AddReferences(NuGetMetadataReference.MicrosoftAspNetCoreMvcCore(Constants.NuGetLatestVersion))
                .WithOptions(ParseOptionsHelper.FromCSharp9)
                .Verify();

        [TestMethod]
        public void PublicMethodArgumentsShouldBeCheckedForNull_Sonar_CSharp11() =>
            sonar.AddPaths("PublicMethodArgumentsShouldBeCheckedForNull.CSharp11.cs")
                .WithOptions(ParseOptionsHelper.FromCSharp11)
                .Verify();

        [TestMethod]
        public void PublicMethodArgumentsShouldBeCheckedForNull_Roslyn_CSharp11() =>
            roslynCS.AddPaths("PublicMethodArgumentsShouldBeCheckedForNull.CSharp11.cs")
                .WithOptions(ParseOptionsHelper.FromCSharp11)
                .Verify();

        [TestMethod]
        public void PublicMethodArgumentsShouldBeCheckedForNull_Roslyn_Razor() =>
            roslynCS.AddPaths("PublicMethodArgumentsShouldBeCheckedForNull.razor", "PublicMethodArgumentsShouldBeCheckedForNull_component.razor")
                    .WithAdditionalFilePath(AnalysisScaffolding.CreateSonarProjectConfig(TestContext, ProjectType.Product))
                    .Verify();

#endif

    }
}
