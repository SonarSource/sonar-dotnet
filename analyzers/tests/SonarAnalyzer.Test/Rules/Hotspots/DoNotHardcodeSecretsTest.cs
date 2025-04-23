/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2014-2025 SonarSource SA
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
using SonarAnalyzer.CSharp.Rules;

namespace SonarAnalyzer.Test.Rules;

[TestClass]
public class DoNotHardcodeSecretsTest
{
    private readonly VerifierBuilder builder = new VerifierBuilder().AddAnalyzer(() => new DoNotHardcodeSecrets(AnalyzerConfiguration.AlwaysEnabled))
            .WithBasePath("Hotspots");

    public TestContext TestContext { get; set; }

    [TestMethod]
    public void DoNotHardcodeSecrets_DefaultValues() =>
        builder.AddPaths("DoNotHardcodeSecrets.cs").Verify();

    // TODO: Add snippet with parametrisation
    [TestMethod]
    public void DoNotHardcodeSecrets_WebConfig() =>
        DoNotHardcodeCredentials_ExternalFiles(AnalyzerLanguage.CSharp, new DoNotHardcodeSecrets(AnalyzerConfiguration.AlwaysEnabled), "WebConfig", "*.config");

    [TestMethod]
    public void DoNotHardcodeSecrets_AppSettings() =>
        DoNotHardcodeCredentials_ExternalFiles(AnalyzerLanguage.CSharp, new DoNotHardcodeSecrets(AnalyzerConfiguration.AlwaysEnabled), "AppSettings", "*.json");

    [TestMethod]
    public void DoNotHardcodeSecrets_LaunchSettings() =>
        DoNotHardcodeCredentials_ExternalFiles(AnalyzerLanguage.CSharp, new DoNotHardcodeSecrets(AnalyzerConfiguration.AlwaysEnabled), "LaunchSettings", "*.json");

    private void DoNotHardcodeCredentials_ExternalFiles(AnalyzerLanguage language, DiagnosticAnalyzer analyzer, string testDirectory, string pattern)
    {
        var root = @$"TestCases\{testDirectory}\DoNotHardcodeSecrets";
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
