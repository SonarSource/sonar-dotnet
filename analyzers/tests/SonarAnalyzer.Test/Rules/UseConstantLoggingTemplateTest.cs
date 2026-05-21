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

namespace SonarAnalyzer.Test.Rules;

[TestClass]
public class UseConstantLoggingTemplateTest
{
    private readonly VerifierBuilder builder = CreateVerifier<CS.UseConstantLoggingTemplate>();

    [TestMethod]
    public void UseConstantLoggingTemplate_CS() =>
        builder.AddPaths("UseConstantLoggingTemplate.cs").Verify();

    private static VerifierBuilder CreateVerifier<TAnalyzer>()
        where TAnalyzer : DiagnosticAnalyzer, new() =>
        new VerifierBuilder<TAnalyzer>()
            .AddReferences(NuGetMetadataReference.MicrosoftExtensionsLoggingPackages(TestConstants.NuGetLatestVersion))
            .AddReferences(NuGetMetadataReference.CastleCore(TestConstants.NuGetLatestVersion))
            .AddReferences(NuGetMetadataReference.Serilog())
            .AddReferences(NuGetMetadataReference.Log4Net("2.0.8", "net45-full"))
            .AddReferences(NuGetMetadataReference.NLog());
}
