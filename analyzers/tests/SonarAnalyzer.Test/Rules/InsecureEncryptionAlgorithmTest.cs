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
    public class InsecureEncryptionAlgorithmTest
    {
        private readonly VerifierBuilder builderCS = new VerifierBuilder<CS.InsecureEncryptionAlgorithm>();
        private readonly VerifierBuilder builderVB = new VerifierBuilder<VB.InsecureEncryptionAlgorithm>();

        [TestMethod]
        public void InsecureEncryptionAlgorithm_MainProject_CS() =>
            builderCS.AddPaths("InsecureEncryptionAlgorithm.cs")
                .AddReferences(GetAdditionalReferences())
                .Verify();

        [TestMethod]
        public void InsecureEncryptionAlgorithm_DoesNotRaiseIssuesForTestProject_CS() =>
            builderCS.AddPaths("InsecureEncryptionAlgorithm.cs")
                .AddTestReference()
                .AddReferences(GetAdditionalReferences())
                .VerifyNoIssuesIgnoreErrors();

#if NET

        [TestMethod]
        public void InsecureEncryptionAlgorithm_CSharp9() =>
            builderCS.AddPaths("InsecureEncryptionAlgorithm.CSharp9.cs")
                .WithTopLevelStatements()
                .AddReferences(GetAdditionalReferences())
                .WithOptions(ParseOptionsHelper.FromCSharp9)
                .Verify();

        [TestMethod]
        public void InsecureEncryptionAlgorithm_CSharp10() =>
            builderCS.AddPaths("InsecureEncryptionAlgorithm.CSharp10.cs")
                .AddReferences(GetAdditionalReferences())
                .WithOptions(ParseOptionsHelper.FromCSharp10)
                .Verify();

        [TestMethod]
        public void InsecureEncryptionAlgorithm_CSharp11() =>
            builderCS.AddPaths("InsecureEncryptionAlgorithm.CSharp11.cs")
                .WithTopLevelStatements()
                .AddReferences(GetAdditionalReferences())
                .WithOptions(ParseOptionsHelper.FromCSharp11)
                .Verify();

#endif

        [TestMethod]
        public void InsecureEncryptionAlgorithm_VB() =>
            builderVB.AddPaths("InsecureEncryptionAlgorithm.vb")
                .AddReferences(GetAdditionalReferences())
                .WithOptions(ParseOptionsHelper.FromVisualBasic14)
                .Verify();

        private static IEnumerable<MetadataReference> GetAdditionalReferences() =>
            MetadataReferenceFacade.SystemSecurityCryptography.Concat(NuGetMetadataReference.BouncyCastle());
    }
}
