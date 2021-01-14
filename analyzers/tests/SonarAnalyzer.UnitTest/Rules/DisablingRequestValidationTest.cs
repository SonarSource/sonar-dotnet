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

using System.Collections.Generic;
using System.IO;
using System.Linq;
using FluentAssertions;
using Microsoft.CodeAnalysis.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SonarAnalyzer.Common;
using SonarAnalyzer.Helpers;
using SonarAnalyzer.UnitTest.MetadataReferences;
using SonarAnalyzer.UnitTest.TestFramework;
using CS = SonarAnalyzer.Rules.CSharp;
using VB = SonarAnalyzer.Rules.VisualBasic;

namespace SonarAnalyzer.UnitTest.Rules
{
    [TestClass]
    public class DisablingRequestValidationTest
    {
        private const string AspNetMvcVersion = "5.2.7";
        private const string WebConfig = "Web.config";
        // for the tests that don't test the XML logic, to avoid test failures caused by unexpected issues from Web.config files
        private const string FolderWithoutWebConfig = @"TestCases\WebConfig\Empty\";

        [TestMethod]
        [TestCategory("Rule")]
        public void DisablingRequestValidation_CS() =>
            Verifier.VerifyAnalyzer(@"TestCases\DisablingRequestValidation.cs",
                new CS.DisablingRequestValidation(AnalyzerConfiguration.AlwaysEnabled, FolderWithoutWebConfig),
                additionalReferences: NuGetMetadataReference.MicrosoftAspNetMvc(AspNetMvcVersion));

        [TestMethod]
        [TestCategory("Rule")]
        public void DisablingRequestValidation_CS_Disabled() =>
            Verifier.VerifyNoIssueReported(@"TestCases\DisablingRequestValidation.cs",
                new CS.DisablingRequestValidation(AnalyzerConfiguration.Hotspot, @"TestCases\WebConfig"),
                additionalReferences: NuGetMetadataReference.MicrosoftAspNetMvc(AspNetMvcVersion));

        [DataTestMethod]
        [DataRow(@"TestCases\WebConfig\S5753Values")]
        [DataRow(@"TestCases\WebConfig\Foo")]
        [DataRow(@"TestCases\WebConfig\Corrupt")]
        [TestCategory("Rule")]
        public void DisablingRequestValidation_CS_WebConfig(string root) =>
            DiagnosticVerifier.VerifyExternalFile(
                SolutionBuilder.Create().AddProject(AnalyzerLanguage.CSharp).GetCompilation(),
                new CS.DisablingRequestValidation(AnalyzerConfiguration.AlwaysEnabled, root),
                File.ReadAllText(Path.Combine(root, WebConfig)));

        [DataTestMethod]
        [DataRow(@"TestCases\WebConfig\MultipleFiles", "SubFolder")]
        [DataRow(@"TestCases\WebConfig\S5753EdgeValues", "3.9", "5.6")]
        [TestCategory("Rule")]
        public void DisablingRequestValidation_CS_WebConfig_SubFolders(string rootDirectory, params string[] subFolders)
        {
            var compilation = SolutionBuilder.Create().AddProject(AnalyzerLanguage.CSharp).GetCompilation();
            var allDiagnostics = DiagnosticVerifier.GetDiagnostics(
                compilation,
                new CS.DisablingRequestValidation(AnalyzerConfiguration.AlwaysEnabled, rootDirectory),
                CompilationErrorBehavior.Default);

            // verify root issues
            var rootWebConfig = Path.Combine(rootDirectory, WebConfig);
            var actualIssuesInRoot = allDiagnostics.Where(d => d.Location.GetLineSpan().Path.EndsWith(rootWebConfig));
            var expectedIssuesInRoot = new IssueLocationCollector().GetExpectedIssueLocations(GetLines(rootWebConfig)).ToList();
            DiagnosticVerifier.CompareActualToExpected(compilation.LanguageVersionString(), actualIssuesInRoot, expectedIssuesInRoot, false);

            // verify subfolder issues
            foreach (var subFolder in subFolders)
            {
                var subFolderWebConfig = Path.Combine(rootDirectory, subFolder, WebConfig);
                var actualSubFolderIssues = allDiagnostics.Where(d => d.Location.GetLineSpan().Path.EndsWith(subFolderWebConfig));
                var expectedSubFolderIssues = new IssueLocationCollector().GetExpectedIssueLocations(GetLines(subFolderWebConfig)).ToList();
                DiagnosticVerifier.CompareActualToExpected(compilation.LanguageVersionString(), actualSubFolderIssues, expectedSubFolderIssues, false);
            }

            allDiagnostics.Should().NotBeEmpty();
        }

        [TestMethod]
        [TestCategory("Rule")]
        public void DisablingRequestValidation_CS_WebConfig_LowerCase() =>
            DiagnosticVerifier.VerifyExternalFile(
                SolutionBuilder.Create().AddProject(AnalyzerLanguage.CSharp).GetCompilation(),
                new CS.DisablingRequestValidation(AnalyzerConfiguration.AlwaysEnabled, @"TestCases\WebConfig\LowerCase\"),
                File.ReadAllText(@"TestCases\WebConfig\LowerCase\web.config"));

        [TestMethod]
        [TestCategory("Rule")]
        public void DisablingRequestValidation_VB() =>
            Verifier.VerifyAnalyzer(@"TestCases\DisablingRequestValidation.vb",
                new VB.DisablingRequestValidation(AnalyzerConfiguration.AlwaysEnabled, FolderWithoutWebConfig),
                additionalReferences: NuGetMetadataReference.MicrosoftAspNetMvc(AspNetMvcVersion));

        [TestMethod]
        [TestCategory("Rule")]
        public void DisablingRequestValidation_VB_Disabled() =>
            Verifier.VerifyNoIssueReported(@"TestCases\DisablingRequestValidation.vb",
                new VB.DisablingRequestValidation(AnalyzerConfiguration.Hotspot, @"TestCases\WebConfig"),
                additionalReferences: NuGetMetadataReference.MicrosoftAspNetMvc(AspNetMvcVersion));

        [TestMethod]
        [TestCategory("Rule")]
        public void DisablingRequestValidation_VB_WebConfig() =>
            DiagnosticVerifier.VerifyExternalFile(
                SolutionBuilder.Create().AddProject(AnalyzerLanguage.VisualBasic).GetCompilation(),
                new VB.DisablingRequestValidation(AnalyzerConfiguration.AlwaysEnabled, @"TestCases\WebConfig\S5753Values\"),
                File.ReadAllText(Path.Combine(@"TestCases\WebConfig\S5753Values", WebConfig)));

        private static IEnumerable<TextLine> GetLines(string path) => SourceText.From(File.ReadAllText(path)).Lines;
    }
}
