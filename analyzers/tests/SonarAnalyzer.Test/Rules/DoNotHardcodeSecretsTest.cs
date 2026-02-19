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

using System.IO;
using CS = SonarAnalyzer.CSharp.Rules;
using VB = SonarAnalyzer.VisualBasic.Rules;

namespace SonarAnalyzer.Test.Rules;

[TestClass]
public class DoNotHardcodeSecretsTest
{
    private readonly VerifierBuilder builderCS = new VerifierBuilder().AddAnalyzer(() => new CS.DoNotHardcodeSecrets());
    private readonly VerifierBuilder builderVB = new VerifierBuilder().AddAnalyzer(() => new VB.DoNotHardcodeSecrets());

    public TestContext TestContext { get; set; }

    [TestMethod]
    public void DoNotHardcodeSecrets_DefaultValues_CS() =>
        builderCS.AddPaths("DoNotHardcodeSecrets.cs").Verify();

    [TestMethod]
    public void DoNotHardcodeSecrets_DefaultValues_CS_Latest() =>
        builderCS.AddPaths("DoNotHardcodeSecrets.Latest.cs").WithOptions(LanguageOptions.CSharpLatest).Verify();

    [TestMethod]
    public void DoNotHardcodeSecrets_DefaultValues_VB() =>
        builderVB.AddPaths("DoNotHardcodeSecrets.vb").Verify();

    [TestMethod]
    public void DoNotHardcodeSecrets_WebConfig_CS() =>
        DoNotHardcodeCredentials_ExternalFiles(builderCS, "WebConfig", "*.config");

    [TestMethod]
    public void DoNotHardcodeSecrets_WebConfig_VB() =>
        DoNotHardcodeCredentials_ExternalFiles(builderVB, "WebConfig", "*.config");

    [TestMethod]
    public void DoNotHardcodeSecrets_AppSettings_CS() =>
        DoNotHardcodeCredentials_ExternalFiles(builderCS, "AppSettings", "*.json");

    [TestMethod]
    public void DoNotHardcodeSecrets_AppSettings_VB() =>
        DoNotHardcodeCredentials_ExternalFiles(builderVB, "AppSettings", "*.json");

    [TestMethod]
    public void DoNotHardcodeSecrets_LaunchSettings_CS() =>
        DoNotHardcodeCredentials_ExternalFiles(builderCS, "LaunchSettings", "*.json");

    [TestMethod]
    public void DoNotHardcodeSecrets_LaunchSettings_VB() =>
        DoNotHardcodeCredentials_ExternalFiles(builderVB, "LaunchSettings", "*.json");

    private void DoNotHardcodeCredentials_ExternalFiles(VerifierBuilder builder, string testDirectory, string pattern)
    {
        var root = @$"TestCases\{testDirectory}\DoNotHardcodeSecrets";
        var paths = Directory.GetFiles(root, pattern, SearchOption.AllDirectories);
        paths.Should().NotBeEmpty();
        builder
            .AddSnippet(string.Empty)
            .WithAdditionalFilePath(AnalysisScaffolding.CreateSonarProjectConfigWithFilesToAnalyze(TestContext, paths))
            .AddAdditionalSourceFiles(paths)
            .Verify();
    }
}
