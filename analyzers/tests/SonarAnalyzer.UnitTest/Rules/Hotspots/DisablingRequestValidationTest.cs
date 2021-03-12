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
        private const string ProjectConfigTemplate = @"
<SonarProjectConfig xmlns=""http://www.sonarsource.com/msbuild/analyzer/2021/1"">
    <FilesToAnalyzePath>{0}\FilesToAnalyze.txt</FilesToAnalyzePath>
</SonarProjectConfig>";

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
        [DataRow(@"TestCases\WebConfig\S5753Values")]
        [DataRow(@"TestCases\WebConfig\UnexpectedContent")]
        [DataRow(@"TestCases\WebConfig\Corrupt")]
        [TestCategory("Rule")]
        public void DisablingRequestValidation_CS_WebConfig(string root) =>
            DiagnosticVerifier.VerifyExternalFile(
                SolutionBuilder.Create().AddProject(AnalyzerLanguage.CSharp).GetCompilation(),
                new CS.DisablingRequestValidation(AnalyzerConfiguration.AlwaysEnabled),
                File.ReadAllText(Path.Combine(root, WebConfig)),
                CreateSonarProjectConfig(root));

        [DataTestMethod]
        [DataRow(@"TestCases\WebConfig\MultipleFiles", "SubFolder")]
        [DataRow(@"TestCases\WebConfig\S5753EdgeValues", "3.9", "5.6")]
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
            try
            {
                allDiagnostics = DiagnosticVerifier.GetDiagnostics(
                    compilation,
                    new CS.DisablingRequestValidation(AnalyzerConfiguration.AlwaysEnabled),
                    CompilationErrorBehavior.Default,
                    sonarProjectConfigPath: CreateSonarProjectConfig(rootDirectory)).ToList();
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
            var root = @"TestCases\WebConfig\LowerCase";
            DiagnosticVerifier.VerifyExternalFile(
                SolutionBuilder.Create().AddProject(AnalyzerLanguage.CSharp).GetCompilation(),
                new CS.DisablingRequestValidation(AnalyzerConfiguration.AlwaysEnabled),
                File.ReadAllText(Path.Combine(root, "web.config")),
                CreateSonarProjectConfig(root));
        }

        [DataTestMethod]
        [DataRow(@"TestCases\WebConfig\TransformCustom\Web.Custom.config")]
        [DataRow(@"TestCases\WebConfig\TransformDebug\Web.Debug.config")]
        [DataRow(@"TestCases\WebConfig\TransformRelease\Web.Release.config")]
        [TestCategory("Rule")]
        public void DisablingRequestValidation_CS_WebConfig_Transformation(string configPath) =>
            DiagnosticVerifier.VerifyExternalFile(
                SolutionBuilder.Create().AddProject(AnalyzerLanguage.CSharp).GetCompilation(),
                new CS.DisablingRequestValidation(AnalyzerConfiguration.AlwaysEnabled),
                File.ReadAllText(configPath),
                CreateSonarProjectConfig(Path.GetDirectoryName(configPath)));

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
            var root = @"TestCases\WebConfig\S5753Values";
            DiagnosticVerifier.VerifyExternalFile(
                SolutionBuilder.Create().AddProject(AnalyzerLanguage.VisualBasic).GetCompilation(),
                new VB.DisablingRequestValidation(AnalyzerConfiguration.AlwaysEnabled),
                File.ReadAllText(Path.Combine(root, WebConfig)),
                CreateSonarProjectConfig(root));
        }

        // Verifies the results for the given web.config file path.
        private static void VerifyResults(string webConfigPath, IList<Diagnostic> allDiagnostics, string languageVersion)
        {
            var actualIssues = allDiagnostics.Where(d => d.Location.GetLineSpan().Path.EndsWith(webConfigPath));
            var expectedIssues = new IssueLocationCollector().GetExpectedIssueLocations(SourceText.From(File.ReadAllText(webConfigPath)).Lines).ToList();
            DiagnosticVerifier.CompareActualToExpected(languageVersion, actualIssues, expectedIssues, false);
        }

        private static string CreateSonarProjectConfig(string filesToAnalyzeDirectory)
        {
            var sonarProjectConfigPath = Path.Combine(filesToAnalyzeDirectory, "SonarProjectConfig.xml");
            File.WriteAllText(sonarProjectConfigPath, string.Format(ProjectConfigTemplate, filesToAnalyzeDirectory));
            return sonarProjectConfigPath;
        }
    }
}
