/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2021 SonarSource SA
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

using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Microsoft.CodeAnalysis;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SonarAnalyzer.Common;
using SonarAnalyzer.UnitTest.MetadataReferences;
using SonarAnalyzer.UnitTest.TestFramework;
using CS = SonarAnalyzer.Rules.CSharp;
using VB = SonarAnalyzer.Rules.VisualBasic;

namespace SonarAnalyzer.UnitTest.Rules
{
    [TestClass]
    public class DoNotHardcodeCredentialsTest
    {
        [TestMethod]
        [TestCategory("Rule")]
        [TestCategory("Hotspot")]
        public void DoNotHardcodeCredentials_CS_DefaultValues() =>
            Verifier.VerifyConcurrentAnalyzer(@"TestCases\Hotspots\DoNotHardcodeCredentials_DefaultValues.cs",
                                              new CS.DoNotHardcodeCredentials(AnalyzerConfiguration.AlwaysEnabled),
                                              AdditionalReferences);

        [TestMethod]
        [TestCategory("Rule")]
        [TestCategory("Hotspot")]
        public void DoNotHardcodeCredentials_CSharp8_DefaultValues() =>
            Verifier.VerifyAnalyzer(@"TestCases\Hotspots\DoNotHardcodeCredentials_DefaultValues.CSharp8.cs",
                new CS.DoNotHardcodeCredentials(AnalyzerConfiguration.AlwaysEnabled),
                ParseOptionsHelper.FromCSharp8,
                AdditionalReferences);

        [TestMethod]
        [TestCategory("Rule")]
        [TestCategory("Hotspot")]
        public void DoNotHardcodeCredentials_CS_CustomValues() =>
            Verifier.VerifyAnalyzer(@"TestCases\Hotspots\DoNotHardcodeCredentials_CustomValues.cs",
                new CS.DoNotHardcodeCredentials(AnalyzerConfiguration.AlwaysEnabled) { CredentialWords = @"kode,facal-faire,*,x\*+?|}{][)(^$.# " },
                AdditionalReferences);

        [TestMethod]
        [TestCategory("Rule")]
        [TestCategory("Hotspot")]
        public void DoNotHardcodeCredentials_CS_CustomValues_CaseInsensitive() =>
            Verifier.VerifyAnalyzer(@"TestCases\Hotspots\DoNotHardcodeCredentials_CustomValues.cs",
                new CS.DoNotHardcodeCredentials(AnalyzerConfiguration.AlwaysEnabled) { CredentialWords = @"KODE ,,,, FaCaL-FaIrE, x\*+?|}{][)(^$.# " },
                AdditionalReferences);

        [TestMethod]
        [TestCategory("Rule")]
        [TestCategory("Hotspot")]
        public void DoNotHardcodeCredentials_VB_DefaultValues() =>
            Verifier.VerifyConcurrentAnalyzer(@"TestCases\Hotspots\DoNotHardcodeCredentials_DefaultValues.vb",
                                              new VB.DoNotHardcodeCredentials(AnalyzerConfiguration.AlwaysEnabled),
                                              AdditionalReferences);

        [TestMethod]
        [TestCategory("Rule")]
        [TestCategory("Hotspot")]
        public void DoNotHardcodeCredentials_VB_CustomValues() =>
            Verifier.VerifyAnalyzer(@"TestCases\Hotspots\DoNotHardcodeCredentials_CustomValues.vb",
                new VB.DoNotHardcodeCredentials(AnalyzerConfiguration.AlwaysEnabled) { CredentialWords = @"kode,facal-faire,*,x\*+?|}{][)(^$.# " },
                AdditionalReferences);

        [TestMethod]
        [TestCategory("Rule")]
        [TestCategory("Hotspot")]
        public void DoNotHardcodeCredentials_VB_CustomValues_CaseInsensitive() =>
            Verifier.VerifyAnalyzer(@"TestCases\Hotspots\DoNotHardcodeCredentials_CustomValues.vb",
                new VB.DoNotHardcodeCredentials(AnalyzerConfiguration.AlwaysEnabled) { CredentialWords = @"KODE ,,,, FaCaL-FaIrE,x\*+?|}{][)(^$.# " },
                AdditionalReferences);

        [TestMethod]
        public void DoNotHardcodeCredentials_ConfiguredCredentialsAreRead()
        {
            var cs = new CS.DoNotHardcodeCredentials();
            cs.CredentialWords = "Lorem, ipsum";
            cs.CredentialWords.Should().Be("Lorem, ipsum");

            var vb = new CS.DoNotHardcodeCredentials();
            vb.CredentialWords = "Lorem, ipsum";
            vb.CredentialWords.Should().Be("Lorem, ipsum");
        }

        internal static IEnumerable<MetadataReference> AdditionalReferences =>
            MetadataReferenceFacade.SystemSecurityCryptography.Concat(MetadataReferenceFacade.SystemNetHttp);
    }
}
