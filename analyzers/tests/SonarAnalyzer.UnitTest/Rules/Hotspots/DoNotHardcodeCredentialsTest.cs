/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2022 SonarSource SA
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

using SonarAnalyzer.Common;
using CS = SonarAnalyzer.Rules.CSharp;
using VB = SonarAnalyzer.Rules.VisualBasic;

namespace SonarAnalyzer.UnitTest.Rules
{
    [TestClass]
    public class DoNotHardcodeCredentialsTest
    {
        private readonly VerifierBuilder builderCS = CreateVerifierCS();
        private readonly VerifierBuilder builderVB = CreateVerifierVB();

        [TestMethod]
        public void DoNotHardcodeCredentials_CS_DefaultValues() =>
            builderCS.AddPaths("DoNotHardcodeCredentials.DefaultValues.cs").Verify();

        [TestMethod]
        public void DoNotHardcodeCredentials_CSharp8_DefaultValues() =>
            builderCS.AddPaths("DoNotHardcodeCredentials.DefaultValues.CSharp8.cs").WithOptions(ParseOptionsHelper.FromCSharp8).Verify();

#if NET

        [TestMethod]
        public void DoNotHardcodeCredentials_CSharp10_DefaultValues() =>
            builderCS.AddPaths("DoNotHardcodeCredentials.DefaultValues.CSharp10.cs").WithOptions(ParseOptionsHelper.FromCSharp10).Verify();

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
        public void DoNotHardcodeCredentials_VB_DefaultValues() =>
            builderVB.AddPaths("DoNotHardcodeCredentials.DefaultValues.vb").Verify();

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
    }
}
