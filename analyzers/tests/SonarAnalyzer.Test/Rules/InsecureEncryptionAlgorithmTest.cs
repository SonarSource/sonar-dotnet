/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2014-2025 SonarSource Sàrl
 * mailto:info AT sonarsource DOT com
 * This program is free software; you can redistribute it and/or
 * modify it under the terms of the Sonar Source-Available License Version 1, as published by SonarSource SA.
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

namespace SonarAnalyzer.Test.Rules;

[TestClass]
public class InsecureEncryptionAlgorithmTest
{
    private readonly VerifierBuilder builderCS = new VerifierBuilder<CS.InsecureEncryptionAlgorithm>().AddReferences(AdditionalReferences());
    private readonly VerifierBuilder builderVB = new VerifierBuilder<VB.InsecureEncryptionAlgorithm>().AddReferences(AdditionalReferences());

    [TestMethod]
    public void InsecureEncryptionAlgorithm_MainProject_CS() =>
        builderCS.AddPaths("InsecureEncryptionAlgorithm.cs").Verify();

    [TestMethod]
    public void InsecureEncryptionAlgorithm_DoesNotRaiseIssuesForTestProject_CS() =>
        builderCS.AddPaths("InsecureEncryptionAlgorithm.cs").AddTestReference().VerifyNoIssuesIgnoreErrors();

    [TestMethod]
    public void InsecureEncryptionAlgorithm_CS_Latest() =>
        builderCS.AddPaths("InsecureEncryptionAlgorithm.Latest.cs").WithTopLevelStatements().WithOptions(LanguageOptions.CSharpLatest).Verify();

    [TestMethod]
    public void InsecureEncryptionAlgorithm_VB() =>
        builderVB.AddPaths("InsecureEncryptionAlgorithm.vb").Verify();

    [TestMethod]
    public void InsecureEncryptionAlgorithm_VB_Latest() =>
        builderVB.AddPaths("InsecureEncryptionAlgorithm.Latest.vb").WithOptions(LanguageOptions.VisualBasicLatest).VerifyNoIssues();

    private static IEnumerable<MetadataReference> AdditionalReferences() =>
        MetadataReferenceFacade.SystemSecurityCryptography.Concat(NuGetMetadataReference.BouncyCastle());
}
