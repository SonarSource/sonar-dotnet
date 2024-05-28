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

using ChecksCS = SonarAnalyzer.SymbolicExecution.Roslyn.RuleChecks.CSharp;
using CS = SonarAnalyzer.Rules.CSharp;

namespace SonarAnalyzer.Test.Rules;

[TestClass]
public class JwtSecretKeysTest
{
    private readonly VerifierBuilder builder = new VerifierBuilder()
                                               .AddAnalyzer(() => new CS.SymbolicExecutionRunner(AnalyzerConfiguration.AlwaysEnabled))
                                               .WithOnlyDiagnostics(ChecksCS.JwtSecretKeys.S6781)
                                               .AddReferences([
                                                   ..NuGetMetadataReference.MicrosoftExtensionsConfigurationAbstractions(Constants.NuGetLatestVersion),
                                                   ..NuGetMetadataReference.MicrosoftIdentityModelTokens(),
                                                   ..NuGetMetadataReference.SystemIdentityModelTokensJwt()]);

#if NET

    [TestMethod]
    public void JwtSecretKeys_CS_AspNetCore() =>
        builder
            .WithOptions(ParseOptionsHelper.FromCSharp8) // Recursive pattern requires C# 8 or later.
            .AddPaths("JwtSecretKeys.AspNet.Core.cs")
            .AddReferences([CoreMetadataReference.SystemSecurityClaims])
            .Verify();

#else

    [TestMethod]
    public void JwtSecretKeys_CS_AspNet() =>
        builder
            .AddPaths("JwtSecretKeys.AspNet.cs")
            .AddReferences(NuGetMetadataReference.SystemConfigurationConfigurationManager())
            .Verify();

#endif

}
