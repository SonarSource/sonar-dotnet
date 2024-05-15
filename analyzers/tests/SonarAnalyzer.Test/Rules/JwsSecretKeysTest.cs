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

using SonarAnalyzer.Rules.CSharp;

namespace SonarAnalyzer.Test.Rules;

[TestClass]
public class JwsSecretKeysTest
{
    private readonly VerifierBuilder builder = new VerifierBuilder<JwsSecretKeys>();

#if NET

    [TestMethod]
    public void JwsSecretKeys_CS_AspNetCore() =>
        builder
            .WithOptions(ParseOptionsHelper.FromCSharp8) // Recursive pattern requires C#8 or later.
            .AddPaths("JwsSecretKeys.AspNet.Core.cs")
            .AddReferences(NuGetMetadataReference.MicrosoftExtensionsConfigurationAbstractions(Constants.NuGetLatestVersion))
            .AddReferences(NuGetMetadataReference.MicrosoftIdentityModelTokens())
            .AddReferences(NuGetMetadataReference.SystemIdentityModelTokensJwt())
            .AddReferences(new[] { CoreMetadataReference.SystemSecurityClaims })
            .Verify();

#else

    [TestMethod]
    public void JwsSecretKeys_CS_AspNet() =>
        builder
            .WithOptions(ParseOptionsHelper.FromCSharp8) // Recursive pattern requires C#8 or later.
            .AddPaths("JwsSecretKeys.AspNet.cs")
            .AddReferences(NuGetMetadataReference.MicrosoftExtensionsConfigurationAbstractions(Constants.NuGetLatestVersion))
            .AddReferences(NuGetMetadataReference.MicrosoftIdentityModelTokens())
            .AddReferences(NuGetMetadataReference.SystemConfigurationConfigurationManager())
            .AddReferences(NuGetMetadataReference.SystemIdentityModelTokensJwt())
            .Verify();

#endif

}
