/*
 * SonarAnalyzer for .NET
 * Copyright (C) SonarSource Sàrl
 * mailto:info AT sonarsource DOT com
 *
 * You can redistribute and/or modify this program under the terms of
 * the Sonar Source-Available License Version 1, as published by SonarSource Sàrl.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
 * See the Sonar Source-Available License for more details.
 *
 * You should have received a copy of the Sonar Source-Available License
 * along with this program; if not, see https://sonarsource.com/license/ssal/
 */

using System.IO;
using CS = SonarAnalyzer.CSharp.Rules;
using VB = SonarAnalyzer.VisualBasic.Rules;

namespace SonarAnalyzer.Test.Rules;

[TestClass]
public class RequestsWithExcessiveLengthTest
{
    private readonly VerifierBuilder builderCS = new VerifierBuilder()
        .AddAnalyzer(() => new CS.RequestsWithExcessiveLength())
        .AddReferences(AdditionalReferences());

    private readonly VerifierBuilder builderVB = new VerifierBuilder()
        .AddAnalyzer(() => new VB.RequestsWithExcessiveLength())
        .AddReferences(AdditionalReferences());

    public TestContext TestContext { get; set; }

    [TestMethod]
    public void RequestsWithExcessiveLength_CS() =>
        builderCS.AddPaths(@"RequestsWithExcessiveLength.cs").Verify();

    [TestMethod]
    public void RequestsWithExcessiveLength_CS_CustomValues() =>
        new VerifierBuilder()
            .AddAnalyzer(() => new CS.RequestsWithExcessiveLength { FileUploadSizeLimit = 42 })
            .AddPaths(@"RequestsWithExcessiveLength_CustomValues.cs")
            .AddReferences(AdditionalReferences())
            .Verify();

    [TestMethod]
    public void RequestsWithExcessiveLength_CSharp_Latest() =>
        builderCS
            .AddPaths("RequestsWithExcessiveLength.Latest.cs")
            .AddReferences(OpenReadStreamReferences())
            .WithConcurrentAnalysis(false)
            .WithOptions(LanguageOptions.FromCSharp12)
            .Verify();

    [TestMethod]
    public void RequestsWithExcessiveLength_CSharp_Latest_Params() =>
        new VerifierBuilder()
            .AddAnalyzer(() => new CS.RequestsWithExcessiveLength { FileUploadSizeLimit = 1024 })
            .AddSnippet("""
                        using System;
                        using System.Threading.Tasks;
                        using Microsoft.AspNetCore.Components.Forms;
                        public class TestCase
                        {
                            public static void OpenReadStream(IBrowserFile file, InputFileChangeEventArgs inputFileChange)
                            {
                                file.OpenReadStream(); // Noncompliant - Default size is 500 KB

                                Parallel.ForEach(inputFileChange.GetMultipleFiles(9), file =>
                                {
                                    file.OpenReadStream(1024 * 1024); // Noncompliant
                                });
                            }
                        }
                """)
            .AddReferences(OpenReadStreamReferences())
            .WithConcurrentAnalysis(false)
            .WithOptions(LanguageOptions.FromCSharp12)
            .Verify();

    [TestMethod]
    public void RequestsWithExcessiveLength_VB() =>
        builderVB.AddPaths(@"RequestsWithExcessiveLength.vb").WithOptions(LanguageOptions.FromVisualBasic15).Verify();

    [TestMethod]
    public void RequestsWithExcessiveLength_VB_CustomValues() =>
        new VerifierBuilder()
            .AddAnalyzer(() => new VB.RequestsWithExcessiveLength { FileUploadSizeLimit = 42 })
            .AddPaths(@"RequestsWithExcessiveLength_CustomValues.vb")
            .AddReferences(AdditionalReferences())
            .Verify();

    [TestMethod]
    [DataRow(true, @"TestCases\WebConfig\RequestsWithExcessiveLength\Values\ContentLength")]
    [DataRow(false, @"TestCases\WebConfig\RequestsWithExcessiveLength\Values\DefaultSettings")]
    [DataRow(true, @"TestCases\WebConfig\RequestsWithExcessiveLength\Values\RequestLength")]
    [DataRow(true, @"TestCases\WebConfig\RequestsWithExcessiveLength\Values\RequestAndContentLength")]
    [DataRow(false, @"TestCases\WebConfig\RequestsWithExcessiveLength\Values\CornerCases")]
    [DataRow(false, @"TestCases\WebConfig\RequestsWithExcessiveLength\Values\ValidValues")]
    [DataRow(false, @"TestCases\WebConfig\RequestsWithExcessiveLength\Values\EmptySystemWeb")]
    [DataRow(false, @"TestCases\WebConfig\RequestsWithExcessiveLength\Values\EmptySystemWebServer")]
    [DataRow(false, @"TestCases\WebConfig\RequestsWithExcessiveLength\Values\SmallValues")]
    [DataRow(false, @"TestCases\WebConfig\RequestsWithExcessiveLength\Values\InvalidConfig")]
    [DataRow(true, @"TestCases\WebConfig\RequestsWithExcessiveLength\Values\NoSystemWeb")]
    [DataRow(true, @"TestCases\WebConfig\RequestsWithExcessiveLength\Values\NoSystemWebServer")]
    [DataRow(false, @"TestCases\WebConfig\RequestsWithExcessiveLength\UnexpectedContent")]
    public void RequestsWithExcessiveLength_CS_WebConfig(bool expectIssues, string root)
    {
        var webConfigPath = WebConfigPath(root);
        var withAdditionalSourceFiles = builderCS
            .AddSnippet("// Nothing to see here")
            .WithAdditionalFilePath(AnalysisScaffolding.CreateSonarProjectConfigWithFilesToAnalyze(TestContext, webConfigPath))
            .AddAdditionalSourceFiles(webConfigPath);
        if (expectIssues)
        {
            withAdditionalSourceFiles.Verify();
        }
        else
        {
            withAdditionalSourceFiles.VerifyNoIssues();
        }
    }

    [TestMethod]
    public void RequestsWithExcessiveLength_CS_WebConfig_CustomParameterValue()
    {
        // Reproducer for https://github.com/SonarSource/sonar-dotnet/issues/7867
        var webConfigPath = WebConfigPath(@"TestCases\WebConfig\RequestsWithExcessiveLength\Values\ContentLength_Compliant"); // 83886080
        new VerifierBuilder()
            .AddAnalyzer(() => new CS.RequestsWithExcessiveLength { FileUploadSizeLimit = 83_886_080 })
            .AddReferences(AdditionalReferences())
            .AddSnippet("// Nothing to see here")
            .WithAdditionalFilePath(AnalysisScaffolding.CreateSonarProjectConfigWithFilesToAnalyze(TestContext, webConfigPath))
            .AddAdditionalSourceFiles(webConfigPath)
            .VerifyNoIssues();
    }

    [TestMethod]
    public void RequestsWithExcessiveLength_CS_CorruptAndNonExistingWebConfigs_ShouldNotFail()
    {
        const string root = @"TestCases\WebConfig\RequestsWithExcessiveLength\Corrupt";
        const string missingDirectory = @"TestCases\WebConfig\RequestsWithExcessiveLength\NonExistingDirectory";
        var corruptFilePath = WebConfigPath(root);
        var nonExistingFilePath = WebConfigPath(missingDirectory);
        builderCS
            .AddSnippet("// Nothing to see here")
            .WithAdditionalFilePath(AnalysisScaffolding.CreateSonarProjectConfigWithFilesToAnalyze(TestContext, corruptFilePath, nonExistingFilePath))
            .AddAdditionalSourceFiles(corruptFilePath)
            .VerifyNoIssues();
    }

    internal static IEnumerable<MetadataReference> AdditionalReferences() =>
        NuGetMetadataReference.MicrosoftAspNetCoreMvcCore(TestConstants.NuGetLatestVersion)
            .Concat(NuGetMetadataReference.MicrosoftAspNetCoreMvcViewFeatures(TestConstants.NuGetLatestVersion));

    private static string WebConfigPath(string rootFolder) =>
        Path.Combine(rootFolder, "Web.config");

    private static IEnumerable<MetadataReference> OpenReadStreamReferences() =>
    [
        ..NuGetMetadataReference.MicrosoftAspNetCoreComponentsWeb(),
        ..MetadataReferenceFacade.SystemThreadingTasks,
        ..MetadataReferenceFacade.SystemCollections
    ];
}
