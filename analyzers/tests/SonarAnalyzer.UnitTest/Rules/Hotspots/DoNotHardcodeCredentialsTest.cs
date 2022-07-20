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
        [Ignore][TestMethod]
        public void DoNotHardcodeCredentials_CS_DefaultValues() =>
            OldVerifier.VerifyAnalyzer(
                @"TestCases\Hotspots\DoNotHardcodeCredentials_DefaultValues.cs",
                new CS.DoNotHardcodeCredentials(AnalyzerConfiguration.AlwaysEnabled),
                AdditionalReferences);

        [Ignore][TestMethod]
        public void DoNotHardcodeCredentials_CSharp8_DefaultValues() =>
            OldVerifier.VerifyAnalyzer(
                @"TestCases\Hotspots\DoNotHardcodeCredentials_DefaultValues.CSharp8.cs",
                new CS.DoNotHardcodeCredentials(AnalyzerConfiguration.AlwaysEnabled),
                ParseOptionsHelper.FromCSharp8,
                AdditionalReferences);

#if NET
        [Ignore][TestMethod]
        public void DoNotHardcodeCredentials_CSharp10_DefaultValues() =>
            OldVerifier.VerifyAnalyzerFromCSharp10Library(
                @"TestCases\Hotspots\DoNotHardcodeCredentials_DefaultValues.CSharp10.cs",
                new CS.DoNotHardcodeCredentials(AnalyzerConfiguration.AlwaysEnabled),
                AdditionalReferences);

#endif

        [Ignore][TestMethod]
        public void DoNotHardcodeCredentials_CS_CustomValues() =>
            OldVerifier.VerifyAnalyzer(
                @"TestCases\Hotspots\DoNotHardcodeCredentials_CustomValues.cs",
                new CS.DoNotHardcodeCredentials(AnalyzerConfiguration.AlwaysEnabled) { CredentialWords = @"kode,facal-faire,*,x\*+?|}{][)(^$.# " },
                AdditionalReferences);

        [Ignore][TestMethod]
        public void DoNotHardcodeCredentials_CS_CustomValues_CaseInsensitive() =>
            OldVerifier.VerifyAnalyzer(
                @"TestCases\Hotspots\DoNotHardcodeCredentials_CustomValues.cs",
                new CS.DoNotHardcodeCredentials(AnalyzerConfiguration.AlwaysEnabled) { CredentialWords = @"KODE ,,,, FaCaL-FaIrE, x\*+?|}{][)(^$.# " },
                AdditionalReferences);

        [Ignore][TestMethod]
        public void DoNotHardcodeCredentials_VB_DefaultValues() =>
            OldVerifier.VerifyAnalyzer(
                @"TestCases\Hotspots\DoNotHardcodeCredentials_DefaultValues.vb",
                new VB.DoNotHardcodeCredentials(AnalyzerConfiguration.AlwaysEnabled),
                AdditionalReferences);

        [Ignore][TestMethod]
        public void DoNotHardcodeCredentials_VB_CustomValues() =>
            OldVerifier.VerifyAnalyzer(
                @"TestCases\Hotspots\DoNotHardcodeCredentials_CustomValues.vb",
                new VB.DoNotHardcodeCredentials(AnalyzerConfiguration.AlwaysEnabled) { CredentialWords = @"kode,facal-faire,*,x\*+?|}{][)(^$.# " },
                AdditionalReferences);

        [Ignore][TestMethod]
        public void DoNotHardcodeCredentials_VB_CustomValues_CaseInsensitive() =>
            OldVerifier.VerifyAnalyzer(
                @"TestCases\Hotspots\DoNotHardcodeCredentials_CustomValues.vb",
                new VB.DoNotHardcodeCredentials(AnalyzerConfiguration.AlwaysEnabled) { CredentialWords = @"KODE ,,,, FaCaL-FaIrE,x\*+?|}{][)(^$.# " },
                AdditionalReferences);

        [Ignore][TestMethod]
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
