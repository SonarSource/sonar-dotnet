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
using SonarAnalyzer.Rules.CSharp;
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
        DoNotHardcodeCredentials_ExternalFiles(AnalyzerLanguage.CSharp, new DoNotHardcodeSecrets(), "WebConfig", "*.config");

    [TestMethod]
    public void DoNotHardcodeSecrets_AppSettings() =>
    DoNotHardcodeCredentials_ExternalFiles(AnalyzerLanguage.CSharp, new DoNotHardcodeSecrets(), "AppSettings", "*.json");

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
