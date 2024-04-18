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

#if NET

using SonarAnalyzer.Rules.CSharp;

namespace SonarAnalyzer.Test.Rules
{
    [TestClass]
    public class DisablingCsrfProtectionTest
    {
        private readonly VerifierBuilder builder = new VerifierBuilder().WithBasePath("Hotspots")
                                                                        .AddAnalyzer(() => new DisablingCsrfProtection(AnalyzerConfiguration.AlwaysEnabled))
                                                                        .AddReferences(AdditionalReferences());

        [TestMethod]
        public void DisablingCSRFProtection_CSharp9() =>
            builder.AddPaths("DisablingCsrfProtection.cs").WithOptions(ParseOptionsHelper.FromCSharp9).Verify();

        [TestMethod]
        public void DisablingCSRFProtection_CSharp10() =>
            builder.AddPaths("DisablingCsrfProtection.CSharp10.cs").WithOptions(ParseOptionsHelper.FromCSharp10).Verify();

        [TestMethod]
        public void DisablingCSRFProtection_CSharp11() =>
            builder.AddPaths("DisablingCsrfProtection.CSharp11.cs").WithOptions(ParseOptionsHelper.FromCSharp11).Verify();

        [TestMethod]
        public void DisablingCSRFProtection_CSharp12() =>
            builder.AddPaths("DisablingCsrfProtection.CSharp12.cs").WithOptions(ParseOptionsHelper.FromCSharp12).Verify();

        internal static IEnumerable<MetadataReference> AdditionalReferences() =>
            new[]
            {
                AspNetCoreMetadataReference.MicrosoftAspNetCoreMvc,
                AspNetCoreMetadataReference.MicrosoftAspNetCoreMvcAbstractions,
                AspNetCoreMetadataReference.MicrosoftAspNetCoreMvcCore,
                AspNetCoreMetadataReference.MicrosoftAspNetCoreMvcViewFeatures,
                AspNetCoreMetadataReference.MicrosoftExtensionsDependencyInjectionAbstractions
            };
    }
}

#endif
