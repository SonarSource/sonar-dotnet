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
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using FluentAssertions;
using Microsoft.CodeAnalysis;
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

        [TestMethod]
        [TestCategory("Rule")]
        public void DisablingRequestValidation_CS() =>
            Verifier.VerifyAnalyzer(@"TestCases\Hotspots\DisablingRequestValidation.cs",
                new CS.DisablingRequestValidation(AnalyzerConfiguration.AlwaysEnabled),
                additionalReferences: NuGetMetadataReference.MicrosoftAspNetMvc(AspNetMvcVersion));

        [TestMethod]
        [TestCategory("Rule")]
        public void DisablingRequestValidation_CS_Disabled() =>
            Verifier.VerifyNoIssueReported(@"TestCases\Hotspots\DisablingRequestValidation.cs",
                new CS.DisablingRequestValidation(AnalyzerConfiguration.Hotspot),
                additionalReferences: NuGetMetadataReference.MicrosoftAspNetMvc(AspNetMvcVersion));

        [DataTestMethod]
        [DataRow(@"TestCases\WebConfig\DisablingRequestValidation\Values")]
        [DataRow(@"TestCases\WebConfig\DisablingRequestValidation\Formatting")]
        [DataRow(@"TestCases\WebConfig\DisablingRequestValidation\UnexpectedContent")]
        [TestCategory("Rule")]
        public void DisablingRequestValidation_CS_WebConfig(string root)
        {
            var webConfigPath = Path.Combine(root, WebConfig);
            DiagnosticVerifier.VerifyExternalFile(
                SolutionBuilder.Create().AddProject(AnalyzerLanguage.CSharp).GetCompilation(),
                new CS.DisablingRequestValidation(AnalyzerConfiguration.AlwaysEnabled),
                webConfigPath,
                TestHelper.CreateSonarProjectConfig(root, TestHelper.CreateFilesToAnalyze(root, webConfigPath)));
        }

        [TestMethod]
        [TestCategory("Rule")]
        public void DisablingRequestValidation_CS_CorruptAndNonExistingWebConfigs()
        {
            var root = @"TestCases\WebConfig\DisablingRequestValidation\Corrupt";
            var nonexisting = @"TestCases\WebConfig\DisablingRequestValidation\NonExsitingDirectory";
            var corruptFilePath = Path.Combine(root, WebConfig);
            var nonExistingFilePath = Path.Combine(nonexisting, WebConfig);
            DiagnosticVerifier.VerifyExternalFile(
                SolutionBuilder.Create().AddProject(AnalyzerLanguage.CSharp).GetCompilation(),
                new CS.DisablingRequestValidation(AnalyzerConfiguration.AlwaysEnabled),
                corruptFilePath,
                TestHelper.CreateSonarProjectConfig(root, TestHelper.CreateFilesToAnalyze(root, corruptFilePath, nonExistingFilePath)));
        }

        [DataTestMethod]
        [DataRow(@"TestCases\WebConfig\DisablingRequestValidation\MultipleFiles", "SubFolder")]
        [DataRow(@"TestCases\WebConfig\DisablingRequestValidation\EdgeValues", "3.9", "5.6")]
        [TestCategory("Rule")]
        public void DisablingRequestValidation_CS_WebConfig_SubFolders(string rootDirectory, params string[] subFolders)
        {
            List<Diagnostic> allDiagnostics;
            var compilation = SolutionBuilder.Create().AddProject(AnalyzerLanguage.CSharp).GetCompilation();
            var languageVersion = compilation.LanguageVersionString();
            var oldCulture = Thread.CurrentThread.CurrentCulture;
            var newCulture = (CultureInfo)oldCulture.Clone();
            // decimal.TryParse() from the implementation might not recognize "1.2" under different culture
            newCulture.NumberFormat.NumberDecimalSeparator = ",";
            Thread.CurrentThread.CurrentCulture = newCulture;
            var rootFile = Path.Combine(rootDirectory, WebConfig);
            var filesToAnalyze = new List<string> { rootFile };
            foreach (var subFolder in subFolders)
            {
                filesToAnalyze.Add(Path.Combine(rootDirectory, subFolder, WebConfig));
            }

            try
            {
                allDiagnostics = DiagnosticVerifier.GetDiagnostics(
                    compilation,
                    new CS.DisablingRequestValidation(AnalyzerConfiguration.AlwaysEnabled),
                    CompilationErrorBehavior.Default,
                    sonarProjectConfigPath: TestHelper.CreateSonarProjectConfig(rootDirectory, TestHelper.CreateFilesToAnalyze(rootDirectory, filesToAnalyze.ToArray()))).ToList();
            }
            finally
            {
                Thread.CurrentThread.CurrentCulture = oldCulture; // Restore, don't mess with other UTs using the same thread
            }

            allDiagnostics.Should().NotBeEmpty();
            var rootWebConfig = Path.Combine(rootDirectory, WebConfig);
            VerifyResults(rootWebConfig, allDiagnostics, languageVersion);
            foreach (var subFolder in subFolders)
            {
                var subFolderWebConfig = Path.Combine(rootDirectory, subFolder, WebConfig);
                VerifyResults(subFolderWebConfig, allDiagnostics, languageVersion);
            }
        }

        [TestMethod]
        [TestCategory("Rule")]
        public void DisablingRequestValidation_CS_WebConfig_LowerCase()
        {
            var root = @"TestCases\WebConfig\DisablingRequestValidation\LowerCase";
            var webConfigPath = Path.Combine(root, "web.config");
            DiagnosticVerifier.VerifyExternalFile(
                SolutionBuilder.Create().AddProject(AnalyzerLanguage.CSharp).GetCompilation(),
                new CS.DisablingRequestValidation(AnalyzerConfiguration.AlwaysEnabled),
                webConfigPath,
                TestHelper.CreateSonarProjectConfig(root, TestHelper.CreateFilesToAnalyze(root, webConfigPath)));
        }

        [DataTestMethod]
        [DataRow(@"TestCases\WebConfig\DisablingRequestValidation\TransformCustom\Web.Custom.config")]
        [DataRow(@"TestCases\WebConfig\DisablingRequestValidation\TransformDebug\Web.Debug.config")]
        [DataRow(@"TestCases\WebConfig\DisablingRequestValidation\TransformRelease\Web.Release.config")]
        [TestCategory("Rule")]
        public void DisablingRequestValidation_CS_WebConfig_Transformation(string configPath) =>
            DiagnosticVerifier.VerifyExternalFile(
                SolutionBuilder.Create().AddProject(AnalyzerLanguage.CSharp).GetCompilation(),
                new CS.DisablingRequestValidation(AnalyzerConfiguration.AlwaysEnabled),
                configPath,
                TestHelper.CreateSonarProjectConfig(Path.GetDirectoryName(configPath), TestHelper.CreateFilesToAnalyze(Path.GetDirectoryName(configPath), configPath)));

        [TestMethod]
        [TestCategory("Rule")]
        public void DisablingRequestValidation_VB() =>
            Verifier.VerifyAnalyzer(@"TestCases\Hotspots\DisablingRequestValidation.vb",
                new VB.DisablingRequestValidation(AnalyzerConfiguration.AlwaysEnabled),
                additionalReferences: NuGetMetadataReference.MicrosoftAspNetMvc(AspNetMvcVersion));

        [TestMethod]
        [TestCategory("Rule")]
        public void DisablingRequestValidation_VB_Disabled() =>
            Verifier.VerifyNoIssueReported(@"TestCases\Hotspots\DisablingRequestValidation.vb",
                new VB.DisablingRequestValidation(AnalyzerConfiguration.Hotspot),
                additionalReferences: NuGetMetadataReference.MicrosoftAspNetMvc(AspNetMvcVersion));

        [TestMethod]
        [TestCategory("Rule")]
        public void DisablingRequestValidation_VB_WebConfig()
        {
            var root = @"TestCases\WebConfig\DisablingRequestValidation\Values";
            var webConfigPath = Path.Combine(root, WebConfig);
            DiagnosticVerifier.VerifyExternalFile(
                SolutionBuilder.Create().AddProject(AnalyzerLanguage.VisualBasic).GetCompilation(),
                new VB.DisablingRequestValidation(AnalyzerConfiguration.AlwaysEnabled),
                webConfigPath,
                TestHelper.CreateSonarProjectConfig(root, TestHelper.CreateFilesToAnalyze(root, webConfigPath)));
        }

        // Verifies the results for the given web.config file path.
        private static void VerifyResults(string webConfigPath, IList<Diagnostic> allDiagnostics, string languageVersion)
        {
            var actualIssues = allDiagnostics.Where(d => d.Location.GetLineSpan().Path.EndsWith(webConfigPath));
            var fileNameSourceText = new DiagnosticVerifier.File(webConfigPath);
            var expectedIssueLocations = fileNameSourceText.ToExpectedIssueLocations();
            DiagnosticVerifier.CompareActualToExpected(languageVersion, actualIssues, new[] { expectedIssueLocations }, false);
        }
    }
}
