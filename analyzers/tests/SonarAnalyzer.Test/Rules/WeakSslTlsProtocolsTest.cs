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
    public class WeakSslTlsProtocolsTest
    {
        private readonly VerifierBuilder builderCS = WithReferences(new VerifierBuilder<CS.WeakSslTlsProtocols>());
        private readonly VerifierBuilder builderVB = WithReferences(new VerifierBuilder<VB.WeakSslTlsProtocols>());

        [TestMethod]
        public void WeakSslTlsProtocols_CSharp() =>
            builderCS.AddPaths("WeakSslTlsProtocols.cs").WithOptions(LanguageOptions.FromCSharp8).Verify();

#if NET

        [TestMethod]
        public void WeakSslTlsProtocols_CSharp12() =>
            builderCS.AddPaths("WeakSslTlsProtocols.CSharp12.cs").WithOptions(LanguageOptions.FromCSharp12).Verify();

#endif

        [TestMethod]
        public void WeakSslTlsProtocols_VB() =>
            builderVB.AddPaths("WeakSslTlsProtocols.vb").Verify();

        private static VerifierBuilder WithReferences(VerifierBuilder builder) =>
            builder.AddReferences(MetadataReferenceFacade.SystemNetHttp).AddReferences(MetadataReferenceFacade.SystemSecurityCryptography);
    }
}
