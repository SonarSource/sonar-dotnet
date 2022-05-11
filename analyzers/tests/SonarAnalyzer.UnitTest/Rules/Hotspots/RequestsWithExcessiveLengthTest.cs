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

using System.IO;
using SonarAnalyzer.Common;
using SonarAnalyzer.UnitTest.MetadataReferences;
using CS = SonarAnalyzer.Rules.CSharp;
using VB = SonarAnalyzer.Rules.VisualBasic;

namespace SonarAnalyzer.UnitTest.Rules
{
    [TestClass]
    public class RequestsWithExcessiveLengthTest
    {
        private readonly VerifierBuilder builderCS = new VerifierBuilder()
                                                     .AddAnalyzer(() => new CS.RequestsWithExcessiveLength(AnalyzerConfiguration.AlwaysEnabled))
                                                     .WithBasePath(@"Hotspots")
                                                     .AddReferences(GetAdditionalReferences());

        private readonly VerifierBuilder builderVB = new VerifierBuilder()
                                                     .AddAnalyzer(() => new VB.RequestsWithExcessiveLength(AnalyzerConfiguration.AlwaysEnabled))
                                                     .WithBasePath(@"Hotspots")
                                                     .AddReferences(GetAdditionalReferences());

        [TestMethod]
        public void RequestsWithExcessiveLength_CS() =>
            builderCS.AddPaths(@"RequestsWithExcessiveLength.cs").Verify();

        [TestMethod]
        public void RequestsWithExcessiveLength_CS_CustomValues() =>
            new VerifierBuilder()
                .AddAnalyzer(() => new CS.RequestsWithExcessiveLength(AnalyzerConfiguration.AlwaysEnabled) { FileUploadSizeLimit = 42 })
                .AddPaths(@"Hotspots\RequestsWithExcessiveLength_CustomValues.cs")
                .AddReferences(GetAdditionalReferences())
                .Verify();

#if NET
        [TestMethod]
        public void RequestsWithExcessiveLength_Csharp9() =>
            builderCS.AddPaths(@"RequestsWithExcessiveLength.CSharp9.cs").WithOptions(ParseOptionsHelper.FromCSharp9).Verify();

        [TestMethod]
        public void RequestsWithExcessiveLength_Csharp10() =>
            builderCS.AddPaths(@"RequestsWithExcessiveLength.CSharp10.cs").WithOptions(ParseOptionsHelper.FromCSharp10).Verify();

        [TestMethod]
        public void RequestsWithExcessiveLength_CsharpPreview() =>
            builderCS
                .AddPaths(@"RequestsWithExcessiveLength.CSharp.Preview.cs")
                .WithConcurrentAnalysis(false)
                .WithOptions(ParseOptionsHelper.CSharpPreview).Verify();
#endif

        [TestMethod]
        public void RequestsWithExcessiveLength_VB() =>
            builderVB.AddPaths(@"RequestsWithExcessiveLength.vb").Verify();

        [TestMethod]
        public void RequestsWithExcessiveLength_VB_CustomValues() =>
            new VerifierBuilder()
                .AddAnalyzer(() => new VB.RequestsWithExcessiveLength(AnalyzerConfiguration.AlwaysEnabled) { FileUploadSizeLimit = 42 })
                .AddPaths(@"Hotspots\RequestsWithExcessiveLength_CustomValues.vb")
                .AddReferences(GetAdditionalReferences())
                .Verify();

        [DataTestMethod]
        [DataRow(@"TestCases\WebConfig\RequestsWithExcessiveLength\Values\ContentLength")]
        [DataRow(@"TestCases\WebConfig\RequestsWithExcessiveLength\Values\DefaultSettings")]
        [DataRow(@"TestCases\WebConfig\RequestsWithExcessiveLength\Values\RequestLength")]
        [DataRow(@"TestCases\WebConfig\RequestsWithExcessiveLength\Values\RequestAndContentLength")]
        [DataRow(@"TestCases\WebConfig\RequestsWithExcessiveLength\Values\CornerCases")]
        [DataRow(@"TestCases\WebConfig\RequestsWithExcessiveLength\Values\ValidValues")]
        [DataRow(@"TestCases\WebConfig\RequestsWithExcessiveLength\Values\EmptySystemWeb")]
        [DataRow(@"TestCases\WebConfig\RequestsWithExcessiveLength\Values\EmptySystemWebServer")]
        [DataRow(@"TestCases\WebConfig\RequestsWithExcessiveLength\Values\SmallValues")]
        [DataRow(@"TestCases\WebConfig\RequestsWithExcessiveLength\Values\InvalidConfig")]
        [DataRow(@"TestCases\WebConfig\RequestsWithExcessiveLength\Values\NoSystemWeb")]
        [DataRow(@"TestCases\WebConfig\RequestsWithExcessiveLength\Values\NoSystemWebServer")]
        [DataRow(@"TestCases\WebConfig\RequestsWithExcessiveLength\UnexpectedContent")]
        public void RequestsWithExcessiveLength_CS_WebConfig(string root)
        {
            var webConfigPath = GetWebConfigPath(root);
            DiagnosticVerifier.VerifyExternalFile(
                CreateCompilation(),
                new CS.RequestsWithExcessiveLength(),
                webConfigPath,
                TestHelper.CreateSonarProjectConfig(root, TestHelper.CreateFilesToAnalyze(root, webConfigPath)));
        }

        [TestMethod]
        public void RequestsWithExcessiveLength_CS_CorruptAndNonExistingWebConfigs_ShouldNotFail()
        {
            const string root = @"TestCases\WebConfig\RequestsWithExcessiveLength\Corrupt";
            const string missingDirectory = @"TestCases\WebConfig\RequestsWithExcessiveLength\NonExistingDirectory";
            var corruptFilePath = GetWebConfigPath(root);
            var nonExistingFilePath = GetWebConfigPath(missingDirectory);
            DiagnosticVerifier.VerifyExternalFile(
                CreateCompilation(),
                new CS.RequestsWithExcessiveLength(),
                corruptFilePath,
                TestHelper.CreateSonarProjectConfig(root, TestHelper.CreateFilesToAnalyze(root, corruptFilePath, nonExistingFilePath)));
        }

        private static string GetWebConfigPath(string rootFolder) => Path.Combine(rootFolder, "Web.config");

        private static Compilation CreateCompilation() => SolutionBuilder.Create().AddProject(AnalyzerLanguage.CSharp).GetCompilation();

        internal static IEnumerable<MetadataReference> GetAdditionalReferences() =>
            NuGetMetadataReference.MicrosoftAspNetCoreMvcCore(Constants.NuGetLatestVersion)
                .Concat(NuGetMetadataReference.MicrosoftAspNetCoreMvcViewFeatures(Constants.NuGetLatestVersion));
    }
}
