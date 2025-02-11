/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2014-2025 SonarSource SA
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

namespace SonarAnalyzer.Test.Rules
{
    [TestClass]
    public class JwtSignedTest
    {
        private readonly VerifierBuilder builderCS = new VerifierBuilder<CS.JwtSigned>();
        private readonly VerifierBuilder builderVB = new VerifierBuilder<VB.JwtSigned>();

        [TestMethod]
        public void JwtSigned_CS() =>
            builderCS.AddPaths("JwtSigned.cs")
                .AddReferences(NuGetMetadataReference.JWT("6.1.0"))
                .Verify();

        [TestMethod]
        public void JwtSigned_JWTDecoderExtensions_CS() =>
            builderCS.AddPaths("JwtSigned.Extensions.cs")
                .AddReferences(NuGetMetadataReference.JWT("7.3.1"))
                .Verify();

#if NET

        [TestMethod]
        public void JwtSigned_CS_FromCSharp9() =>
            builderCS.AddPaths("JwtSigned.CSharp9.cs")
                .WithTopLevelStatements()
                .AddReferences(NuGetMetadataReference.JWT("6.1.0"))
                .Verify();

#endif

        [TestMethod]
        public void JwtSigned_VB() =>
            builderVB.AddPaths("JwtSigned.vb")
                .AddReferences(NuGetMetadataReference.JWT("6.1.0"))
                .Verify();

        [TestMethod]
        public void JwtSigned_JWTDecoderExtensions_VB() =>
            builderVB.AddPaths("JwtSigned.Extensions.vb")
                .AddReferences(NuGetMetadataReference.JWT("7.3.1"))
                .Verify();
    }
}
