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

using CS = SonarAnalyzer.CSharp.Rules;
using VB = SonarAnalyzer.VisualBasic.Rules;

namespace SonarAnalyzer.Test.Rules;

[TestClass]
public class TooManyParametersTest
{
    private static readonly MetadataReference[] DependencyInjectionReferences =
        [
            ..NuGetMetadataReference.MicrosoftAspNetCoreMvcCore(TestConstants.DotNetCore220Version),        // For FromServicesAttribute
            ..NuGetMetadataReference.MicrosoftExtensionsDependencyInjectionAbstractions("8.0.1")            // For FromKeyedServicesAttribute
        ];

    private static readonly MetadataReference[] AzureFunctionsReferences =
        [
            ..NuGetMetadataReference.Package("Microsoft.Azure.WebJobs.Extensions.Storage.Blobs", TestConstants.NuGetLatestVersion), // For BlobAttribute and BlobTriggerAttribute
            ..NuGetMetadataReference.Package("Microsoft.Azure.WebJobs.Extensions.CosmosDB", TestConstants.NuGetLatestVersion)       // For CosmosDBAttribute
        ];

    private readonly VerifierBuilder builderCSMax3 = new VerifierBuilder().AddAnalyzer(() => new CS.TooManyParameters { Maximum = 3 });
    private readonly VerifierBuilder builderVBMax3 = new VerifierBuilder().AddAnalyzer(() => new VB.TooManyParameters { Maximum = 3 });

    [TestMethod]
    public void TooManyParameters_CS_CustomValues() =>
        builderCSMax3.AddPaths("TooManyParameters_CustomValues.cs")
            .WithOptions(LanguageOptions.FromCSharp8)
            .Verify();

    [TestMethod]
    public void TooManyParameters_CS_CustomValues_TopLevelStatements() =>
         builderCSMax3.AddPaths("TooManyParameters_CustomValues.TopLevelStatements.cs")
            .WithTopLevelStatements()
            .Verify();

    [TestMethod]
    public void TooManyParameters_CS_CustomValues_Latest() =>
        builderCSMax3.AddPaths("TooManyParameters_CustomValues.Latest.cs")
            .WithOptions(LanguageOptions.CSharpLatest)
            .Verify();

    [TestMethod]
    public void TooManyParameters_CS_FromServices() =>
        builderCSMax3.AddPaths("TooManyParameters_FromServices.cs")
            .AddReferences(DependencyInjectionReferences)
            .Verify();

    [TestMethod]
    public void TooManyParameters_CS_FromServices_Latest() =>
        builderCSMax3.AddPaths("TooManyParameters_FromServices.Latest.cs")
            .AddReferences(DependencyInjectionReferences)
            .WithOptions(LanguageOptions.CSharpLatest)
            .Verify();

    [TestMethod]
    public void TooManyParameters_CS_HotChocolate() =>
        builderCSMax3.AddPaths("TooManyParameters_HotChocolate.cs")
            .AddReferences(NuGetMetadataReference.HotChocolateAbstractions("13.9.14"))   // Pinned to 13.x, the last major version declaring ScopedServiceAttribute
            .Verify();

    [TestMethod]
    public void TooManyParameters_CS_Orleans() =>
        builderCSMax3.AddPaths("TooManyParameters_Orleans.cs")
            .AddReferences(NuGetMetadataReference.Package("Microsoft.Orleans.Runtime", TestConstants.NuGetLatestVersion))  // For PersistentStateAttribute
            .Verify();

    [TestMethod]
    public void TooManyParameters_CS_Dapr() =>
        builderCSMax3.AddPaths("TooManyParameters_Dapr.cs")
            .AddReferences(NuGetMetadataReference.Package("Dapr.AspNetCore", TestConstants.NuGetLatestVersion)) // For FromStateAttribute
            .Verify();

    [TestMethod]
    public void TooManyParameters_CS_AzureFunctions() =>
        builderCSMax3.AddPaths("TooManyParameters_AzureFunctions.cs")
            .AddReferences(AzureFunctionsReferences)
            .Verify();

    [TestMethod]
    public void TooManyParameters_VB_CustomValues() =>
        builderVBMax3.AddPaths("TooManyParameters_CustomValues.vb").Verify();

    [TestMethod]
    public void TooManyParameters_VB_FromServices() =>
        builderVBMax3.AddPaths("TooManyParameters_FromServices.vb")
            .AddReferences(DependencyInjectionReferences)
            .Verify();

    [TestMethod]
    public void TooManyParameters_CS_DefaultValues() =>
        new VerifierBuilder<CS.TooManyParameters>().AddPaths("TooManyParameters_DefaultValues.cs")
            .WithOptions(LanguageOptions.FromCSharp8)
            .Verify();

    [TestMethod]
    public void TooManyParameters_VB_DefaultValues() =>
        new VerifierBuilder<VB.TooManyParameters>().AddPaths("TooManyParameters_DefaultValues.vb").Verify();
}
