/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2014-2024 SonarSource SA
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

using CS = SonarAnalyzer.Rules.CSharp;
using VB = SonarAnalyzer.Rules.VisualBasic;

namespace SonarAnalyzer.Test.Rules
{
    [TestClass]
    public class CertificateValidationCheckTest
    {
        private static readonly VerifierBuilder WithReferences = new VerifierBuilder()
            .AddReferences(MetadataReferenceFacade.SystemNetHttp)
            .AddReferences(MetadataReferenceFacade.SystemSecurityCryptography)
            .AddReferences(MetadataReferenceFacade.NetStandard);
        private readonly VerifierBuilder builderCS = WithReferences.AddAnalyzer(() => new CS.CertificateValidationCheck());
        private readonly VerifierBuilder builderVB = WithReferences.AddAnalyzer(() => new VB.CertificateValidationCheck());

        [TestMethod]
        public void CertificateValidationCheck_CS() =>
            builderCS.AddPaths("CertificateValidationCheck.cs").Verify();

#if NET

        [TestMethod]
        public void CertificateValidationCheck_CSharp8() =>
            builderCS.AddPaths("CertificateValidationCheck.CSharp8.cs").WithOptions(LanguageOptions.FromCSharp8).Verify();

        [TestMethod]
        public void CertificateValidationCheck_CS_CSharp9() =>
            builderCS.AddPaths("CertificateValidationCheck.CSharp9.cs", "CertificateValidationCheck.CSharp9.Partial.cs").WithOptions(LanguageOptions.FromCSharp9).Verify();

        [TestMethod]
        public void CertificateValidationCheck_CS_TopLevelStatements() =>
            builderCS.AddPaths("CertificateValidationCheck.TopLevelStatements.cs").WithTopLevelStatements().Verify();

#endif

        [TestMethod]
        public void CertificateValidationCheck_VB() =>
            builderVB.AddPaths("CertificateValidationCheck.vb").Verify();

        [TestMethod]
        public void CreateParameterLookup_CS_ThrowsException()
        {
            var analyzer = new CS.CertificateValidationCheck();
            Action a = () => analyzer.CreateParameterLookup(null, null);
            a.Should().Throw<ArgumentException>();
        }

        [TestMethod]
        public void CreateParameterLookup_VB_ThrowsException()
        {
            var analyzer = new VB.CertificateValidationCheck();
            Action a = () => analyzer.CreateParameterLookup(null, null);
            a.Should().Throw<ArgumentException>();
        }
    }
}
