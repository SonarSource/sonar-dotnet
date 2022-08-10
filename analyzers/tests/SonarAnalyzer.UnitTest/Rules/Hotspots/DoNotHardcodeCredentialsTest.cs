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
        private readonly VerifierBuilder builderCS = new VerifierBuilder().AddAnalyzer(() => new CS.DoNotHardcodeCredentials(AnalyzerConfiguration.AlwaysEnabled));
        private readonly VerifierBuilder builderVB = new VerifierBuilder().AddAnalyzer(() => new VB.DoNotHardcodeCredentials(AnalyzerConfiguration.AlwaysEnabled));

        [TestMethod]
        public void DoNotHardcodeCredentials_CS_DefaultValues() =>
            builderCS.AddPaths(@"Hotspots\DoNotHardcodeCredentials_DefaultValues.cs")
                .AddReferences(AdditionalReferences)
                .Verify();

        [TestMethod]
        public void DoNotHardcodeCredentials_CSharp8_DefaultValues() =>
            builderCS.AddPaths(@"Hotspots\DoNotHardcodeCredentials_DefaultValues.CSharp8.cs")
                .AddReferences(AdditionalReferences)
                .WithOptions(ParseOptionsHelper.FromCSharp8)
                .Verify();

#if NET

        [TestMethod]
        public void DoNotHardcodeCredentials_CSharp10_DefaultValues() =>
            builderCS.AddPaths(@"Hotspots\DoNotHardcodeCredentials_DefaultValues.CSharp10.cs")
                .AddReferences(AdditionalReferences)
                .WithOptions(ParseOptionsHelper.FromCSharp10)
                .Verify();

#endif

        [TestMethod]
        public void DoNotHardcodeCredentials_CS_CustomValues() =>
            new VerifierBuilder().AddAnalyzer(() => new CS.DoNotHardcodeCredentials(AnalyzerConfiguration.AlwaysEnabled) { CredentialWords = @"kode,facal-faire,*,x\*+?|}{][)(^$.# " })
                .AddPaths(@"Hotspots\DoNotHardcodeCredentials_CustomValues.cs")
                .AddReferences(AdditionalReferences)
                .Verify();

        [TestMethod]
        public void DoNotHardcodeCredentials_CS_CustomValues_CaseInsensitive() =>
            new VerifierBuilder().AddAnalyzer(() => new CS.DoNotHardcodeCredentials(AnalyzerConfiguration.AlwaysEnabled) { CredentialWords = @"KODE ,,,, FaCaL-FaIrE, x\*+?|}{][)(^$.# " })
                .AddPaths(@"Hotspots\DoNotHardcodeCredentials_CustomValues.cs")
                .AddReferences(AdditionalReferences)
                .Verify();

        [TestMethod]
        public void DoNotHardcodeCredentials_VB_DefaultValues() =>
            builderVB.AddPaths(@"Hotspots\DoNotHardcodeCredentials_DefaultValues.vb")
                .AddReferences(AdditionalReferences)
                .Verify();

        [TestMethod]
        public void DoNotHardcodeCredentials_VB_CustomValues() =>
            new VerifierBuilder().AddAnalyzer(() => new VB.DoNotHardcodeCredentials(AnalyzerConfiguration.AlwaysEnabled) { CredentialWords = @"kode,facal-faire,*,x\*+?|}{][)(^$.# " })
                .AddPaths(@"Hotspots\DoNotHardcodeCredentials_CustomValues.vb")
                .AddReferences(AdditionalReferences)
                .Verify();

        [TestMethod]
        public void DoNotHardcodeCredentials_VB_CustomValues_CaseInsensitive() =>
            new VerifierBuilder().AddAnalyzer(() => new VB.DoNotHardcodeCredentials(AnalyzerConfiguration.AlwaysEnabled) { CredentialWords = @"KODE ,,,, FaCaL-FaIrE,x\*+?|}{][)(^$.# " })
                .AddPaths(@"Hotspots\DoNotHardcodeCredentials_CustomValues.vb")
                .AddReferences(AdditionalReferences)
                .Verify();

        [TestMethod]
        public void DoNotHardcodeCredentials_ConfiguredCredentialsAreRead()
        {
            var cs = new CS.DoNotHardcodeCredentials
            {
                CredentialWords = "Lorem, ipsum"
            };
            cs.CredentialWords.Should().Be("Lorem, ipsum");

            var vb = new CS.DoNotHardcodeCredentials
            {
                CredentialWords = "Lorem, ipsum"
            };
            vb.CredentialWords.Should().Be("Lorem, ipsum");
        }

        internal static IEnumerable<MetadataReference> AdditionalReferences =>
            MetadataReferenceFacade.SystemSecurityCryptography.Concat(MetadataReferenceFacade.SystemNetHttp);
    }
}
