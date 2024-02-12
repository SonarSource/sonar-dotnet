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
    public class EncryptionAlgorithmsShouldBeSecureTest
    {
        private readonly VerifierBuilder<CS.EncryptionAlgorithmsShouldBeSecure> builderCS = new();
        private readonly VerifierBuilder<VB.EncryptionAlgorithmsShouldBeSecure> builderVB = new();

        [TestMethod]
        public void EncryptionAlgorithmsShouldBeSecure_CS() =>
            builderCS.AddPaths(@"EncryptionAlgorithmsShouldBeSecure.cs").AddReferences(GetAdditionalReferences()).Verify();

        [TestMethod]
        public void EncryptionAlgorithmsShouldBeSecure_CS_NetStandard21() =>
            builderCS.AddPaths(@"EncryptionAlgorithmsShouldBeSecure_NetStandard21.cs").AddReferences(MetadataReferenceFacade.NetStandard21.Concat(GetAdditionalReferences())).Verify();

        [TestMethod]
        public void EncryptionAlgorithmsShouldBeSecure_VB() =>
            builderVB.AddPaths(@"EncryptionAlgorithmsShouldBeSecure.vb").AddReferences(GetAdditionalReferences()).Verify();

        [TestMethod]
        public void EncryptionAlgorithmsShouldBeSecure_VB_NetStandard21() =>
            builderVB.AddPaths(@"EncryptionAlgorithmsShouldBeSecure_NetStandard21.vb")
                     .AddReferences(MetadataReferenceFacade.NetStandard21.Concat(GetAdditionalReferences()))
                     .Verify();

#if NET

        [TestMethod]
        public void EncryptionAlgorithmsShouldBeSecure_VB_NetStandard21_Net7() =>
            builderVB.AddPaths(@"EncryptionAlgorithmsShouldBeSecure_NetStandard21.Net7.vb")
                .WithOptions(ParseOptionsHelper.VisualBasicLatest)
                .AddReferences(MetadataReferenceFacade.NetStandard21.Concat(GetAdditionalReferences()))
                .Verify();

#else

        [TestMethod]
        public void EncryptionAlgorithmsShouldBeSecure_VB_NetStandard21_Net48() =>
            builderVB.AddPaths(@"EncryptionAlgorithmsShouldBeSecure_NetStandard21.Net48.vb")
                     .WithOptions(ParseOptionsHelper.VisualBasicLatest)
                     .AddReferences(MetadataReferenceFacade.NetStandard21.Concat(GetAdditionalReferences()))
                     .Verify();

#endif

        private static IEnumerable<MetadataReference> GetAdditionalReferences() =>
            MetadataReferenceFacade.SystemSecurityCryptography;
    }
}
