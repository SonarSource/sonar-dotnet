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

using System.IO;
using CS = SonarAnalyzer.Rules.CSharp;
using VB = SonarAnalyzer.Rules.VisualBasic;

namespace SonarAnalyzer.Test.Rules;

[TestClass]
public class DoNotHardcodeCredentialsTest
{
    private readonly VerifierBuilder builderCS = CreateVerifierCS();
    private readonly VerifierBuilder builderVB = CreateVerifierVB();

    public TestContext TestContext { get; set; }

    [TestMethod]
    public void DoNotHardcodeCredentials_CS_DefaultValues() =>
        builderCS.AddPaths("DoNotHardcodeCredentials.DefaultValues.cs").Verify();

    [TestMethod]
    public void DoNotHardcodeCredentials_CS_SecureString() =>
        builderCS.AddPaths("DoNotHardcodeCredentials.SecureString.cs").Verify();

    [TestMethod]
    public void DoNotHardcodeCredentials_VB_SecureString() =>
        builderVB.AddPaths("DoNotHardcodeCredentials.SecureString.vb").Verify();

#if NET

    [TestMethod]
    public void DoNotHardcodeCredentials_CS_Latest() =>
        builderCS.AddPaths("DoNotHardcodeCredentials.DefaultValues.Latest.cs")
            .AddReferences(AdditionalReferences)
            .WithOptions(ParseOptionsHelper.CSharpLatest)
            .Verify();

#endif

    [TestMethod]
    public void DoNotHardcodeCredentials_CS_CustomValues() =>
        CreateVerifierCS(@"kode,facal-faire,*,x\*+?|}{][)(^$.# ")
            .AddPaths("DoNotHardcodeCredentials.CustomValues.cs")
            .Verify();

    [TestMethod]
    public void DoNotHardcodeCredentials_CS_CustomValues_CaseInsensitive() =>
        CreateVerifierCS(@"KODE ,,,, FaCaL-FaIrE, x\*+?|}{][)(^$.# ")
            .AddPaths("DoNotHardcodeCredentials.CustomValues.cs")
            .Verify();

    [TestMethod]
    public void DoNotHardcodeCredentials_CS_WebConfig() =>
        DoNotHardcodeCredentials_ExternalFiles(AnalyzerLanguage.CSharp, new CS.DoNotHardcodeCredentials(AnalyzerConfiguration.AlwaysEnabled), "WebConfig", "*.config");

    [TestMethod]
    public void DoNotHardcodeCredentials_CS_AppSettings() =>
        DoNotHardcodeCredentials_ExternalFiles(AnalyzerLanguage.CSharp, new CS.DoNotHardcodeCredentials(AnalyzerConfiguration.AlwaysEnabled), "AppSettings", "*.json");

    [TestMethod]
    public void DoNotHardcodeCredentials_VB_DefaultValues() =>
        builderVB.AddPaths("DoNotHardcodeCredentials.DefaultValues.vb").WithOptions(ParseOptionsHelper.FromVisualBasic14).Verify();

    [TestMethod]
    public void DoNotHardcodeCredentials_VB_CustomValues() =>
        CreateVerifierVB(@"kode,facal-faire,*,x\*+?|}{][)(^$.# ")
            .AddPaths("DoNotHardcodeCredentials.CustomValues.vb")
            .Verify();

    [TestMethod]
    public void DoNotHardcodeCredentials_VB_CustomValues_CaseInsensitive() =>
        CreateVerifierVB(@"KODE ,,,, FaCaL-FaIrE,x\*+?|}{][)(^$.# ")
            .AddPaths("DoNotHardcodeCredentials.CustomValues.vb")
            .Verify();

    [TestMethod]
    public void DoNotHardcodeCredentials_VB_WebConfig() =>
        DoNotHardcodeCredentials_ExternalFiles(AnalyzerLanguage.VisualBasic, new VB.DoNotHardcodeCredentials(AnalyzerConfiguration.AlwaysEnabled), "WebConfig", "*.config");

    [TestMethod]
    public void DoNotHardcodeCredentials_VB_AppSettings() =>
        DoNotHardcodeCredentials_ExternalFiles(AnalyzerLanguage.VisualBasic, new VB.DoNotHardcodeCredentials(AnalyzerConfiguration.AlwaysEnabled), "AppSettings", "*.json");

    [TestMethod]
    public void DoNotHardcodeCredentials_ConfiguredCredentialsAreRead()
    {
        var cs = new CS.DoNotHardcodeCredentials { CredentialWords = "Lorem, ipsum" };
        cs.CredentialWords.Should().Be("Lorem, ipsum");

        var vb = new CS.DoNotHardcodeCredentials { CredentialWords = "Lorem, ipsum" };
        vb.CredentialWords.Should().Be("Lorem, ipsum");
    }

    internal static IEnumerable<MetadataReference> AdditionalReferences =>
        MetadataReferenceFacade.SystemSecurityCryptography.Concat(MetadataReferenceFacade.SystemNetHttp);

    private static VerifierBuilder CreateVerifierCS(string credentialWords = null) =>
        new VerifierBuilder().AddAnalyzer(() => credentialWords is null
                                                    ? new CS.DoNotHardcodeCredentials(AnalyzerConfiguration.AlwaysEnabled)
                                                    : new CS.DoNotHardcodeCredentials(AnalyzerConfiguration.AlwaysEnabled) { CredentialWords = credentialWords })
            .WithBasePath("Hotspots")
            .AddReferences(AdditionalReferences);

    private static VerifierBuilder CreateVerifierVB(string credentialWords = null) =>
        new VerifierBuilder().AddAnalyzer(() => credentialWords is null
                                                    ? new VB.DoNotHardcodeCredentials(AnalyzerConfiguration.AlwaysEnabled)
                                                    : new VB.DoNotHardcodeCredentials(AnalyzerConfiguration.AlwaysEnabled) { CredentialWords = credentialWords })
            .WithBasePath("Hotspots")
            .AddReferences(AdditionalReferences);

    private void DoNotHardcodeCredentials_ExternalFiles(AnalyzerLanguage language, DiagnosticAnalyzer analyzer, string testDirectory, string pattern)
    {
        var root = @$"TestCases\{testDirectory}\DoNotHardcodeCredentials";
        var paths = Directory.GetFiles(root, pattern, SearchOption.AllDirectories);
        paths.Should().NotBeEmpty();
        var compilation = CreateCompilation(language);
        foreach (var path in paths)
        {
            DiagnosticVerifier.Verify(
                compilation,
                analyzer,
                AnalysisScaffolding.CreateSonarProjectConfigWithFilesToAnalyze(TestContext, path),
                null,
                [path]);
        }
    }

    private static Compilation CreateCompilation(AnalyzerLanguage language) =>
        SolutionBuilder.Create().AddProject(language).GetCompilation();
}
