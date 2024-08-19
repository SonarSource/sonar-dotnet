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
    public class WeakSslTlsProtocolsTest
    {
        private readonly VerifierBuilder builderCS = WithReferences(new VerifierBuilder<CS.WeakSslTlsProtocols>());
        private readonly VerifierBuilder builderVB = WithReferences(new VerifierBuilder<VB.WeakSslTlsProtocols>());

        [TestMethod]
        public void WeakSslTlsProtocols_CSharp() =>
            builderCS.AddPaths("WeakSslTlsProtocols.cs").WithOptions(ParseOptionsHelper.FromCSharp8).Verify();

#if NET

        [TestMethod]
        public void WeakSslTlsProtocols_CSharp12() =>
            builderCS.AddPaths("WeakSslTlsProtocols.CSharp12.cs").WithOptions(ParseOptionsHelper.FromCSharp12).Verify();

#endif

        [TestMethod]
        public void WeakSslTlsProtocols_VB() =>
            builderVB.AddPaths("WeakSslTlsProtocols.vb").Verify();

        private static VerifierBuilder WithReferences(VerifierBuilder builder) =>
            builder.AddReferences(MetadataReferenceFacade.SystemNetHttp).AddReferences(MetadataReferenceFacade.SystemSecurityCryptography);
    }
}
