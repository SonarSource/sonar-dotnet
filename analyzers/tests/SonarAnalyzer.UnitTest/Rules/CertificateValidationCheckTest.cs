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

using System;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SonarAnalyzer.UnitTest.MetadataReferences;
using SonarAnalyzer.UnitTest.TestFramework;
using CS = SonarAnalyzer.Rules.CSharp;
using VB = SonarAnalyzer.Rules.VisualBasic;

namespace SonarAnalyzer.UnitTest.Rules
{
    [TestClass]
    public class CertificateValidationCheckTest
    {
        private readonly VerifierBuilder builderCS = new VerifierBuilder<CS.CertificateValidationCheck>()
            .AddReferences(MetadataReferenceFacade.SystemNetHttp)
            .AddReferences(MetadataReferenceFacade.SystemSecurityCryptography)
            .AddReferences(NetStandardMetadataReference.Netstandard);
        private readonly VerifierBuilder builderVB = new VerifierBuilder<VB.CertificateValidationCheck>()
            .AddReferences(MetadataReferenceFacade.SystemNetHttp)
            .AddReferences(MetadataReferenceFacade.SystemSecurityCryptography)
            .AddReferences(NetStandardMetadataReference.Netstandard);

        [TestMethod]
        public void CertificateValidationCheck_CS() =>
            builderCS.AddPaths("CertificateValidationCheck.cs").Verify();

#if NET

        [TestMethod]
        public void CertificateValidationCheck_CSharp8() =>
            builderCS.AddPaths("CertificateValidationCheck.CSharp8.cs").WithOptions(ParseOptionsHelper.FromCSharp8).Verify();

        [TestMethod]
        public void CertificateValidationCheck_CS_CSharp9() =>
            builderCS.AddPaths("CertificateValidationCheck.CSharp9.cs", "CertificateValidationCheck.CSharp9.Partial.cs").WithOptions(ParseOptionsHelper.FromCSharp9).Verify();

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
