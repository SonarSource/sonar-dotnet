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

using CS = SonarAnalyzer.Rules.CSharp;
using VB = SonarAnalyzer.Rules.VisualBasic;

namespace SonarAnalyzer.Test.Rules
{
    [TestClass]
    public class CreatingHashAlgorithmsTest
    {
        private readonly VerifierBuilder builderCS = new VerifierBuilder().WithBasePath("Hotspots")
            .AddReferences(MetadataReferenceFacade.SystemSecurityCryptography)
            .AddAnalyzer(() => new CS.CreatingHashAlgorithms(AnalyzerConfiguration.AlwaysEnabled));
        private readonly VerifierBuilder builderVB = new VerifierBuilder().WithBasePath("Hotspots")
            .AddReferences(MetadataReferenceFacade.SystemSecurityCryptography)
            .AddAnalyzer(() => new VB.CreatingHashAlgorithms(AnalyzerConfiguration.AlwaysEnabled));

        [TestMethod]
        public void CreatingHashAlgorithms_CSharp8() =>
            builderCS.AddPaths("CreatingHashAlgorithms.cs")
                .WithOptions(ParseOptionsHelper.FromCSharp8)
                .Verify();

#if NETFRAMEWORK // HMACRIPEMD160, MD5Cng, RIPEMD160Managed and RIPEMD160 are available only for .Net Framework

        [TestMethod]
        public void CreatingHashAlgorithms_CS_NetFx() =>
            builderCS.AddPaths("CreatingHashAlgorithms.NetFramework.cs")
                .WithOptions(ParseOptionsHelper.FromCSharp8)
                .Verify();

#endif

        [TestMethod]
        public void CreatingHashAlgorithms_VB() =>
            builderVB.AddPaths("CreatingHashAlgorithms.vb").Verify();

#if NETFRAMEWORK // HMACRIPEMD160, MD5Cng, RIPEMD160Managed and RIPEMD160 are available only for .Net Framework

        [TestMethod]
        public void CreatingHashAlgorithms_VB_NetFx() =>
            builderVB.AddPaths("CreatingHashAlgorithms.NetFramework.vb").Verify();

#endif

#if NET

        [TestMethod]
        public void CreatingHashAlgorithms_CSharp11() =>
            builderCS.AddPaths("CreatingHashAlgorithms.CSharp11.cs").WithOptions(ParseOptionsHelper.FromCSharp11).Verify();

        [TestMethod]
        public void CreatingHashAlgorithms_CSharp12() =>
            builderCS.AddPaths("CreatingHashAlgorithms.CSharp12.cs").WithOptions(ParseOptionsHelper.FromCSharp12).VerifyNoIssues();  // primary constructors are not supported yet

#endif

    }
}
