/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2014-2025 SonarSource Sàrl
 * mailto:info AT sonarsource DOT com
 * This program is free software; you can redistribute it and/or
 * modify it under the terms of the Sonar Source-Available License Version 1, as published by SonarSource Sàrl.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
 * See the Sonar Source-Available License for more details.
 *
 * You should have received a copy of the Sonar Source-Available License
 * along with this program; if not, see https://sonarsource.com/license/ssal/
 */

using CS = SonarAnalyzer.CSharp.Rules;
using VB = SonarAnalyzer.VisualBasic.Rules;

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
        public void InsecureEncryptionAlgorithm_CS_Latest() =>
            builderCS.AddPaths("InsecureEncryptionAlgorithm.Latest.cs")
                .WithTopLevelStatements()
                .AddReferences(GetAdditionalReferences())
                .WithOptions(LanguageOptions.CSharpLatest)
                .Verify();

#endif

        [TestMethod]
        public void InsecureEncryptionAlgorithm_VB() =>
            builderVB.AddPaths("InsecureEncryptionAlgorithm.vb")
                .AddReferences(GetAdditionalReferences())
                .WithOptions(LanguageOptions.FromVisualBasic14)
                .Verify();

        private static IEnumerable<MetadataReference> GetAdditionalReferences() =>
            MetadataReferenceFacade.SystemSecurityCryptography.Concat(NuGetMetadataReference.BouncyCastle());
    }
}
