/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2021 SonarSource SA
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

using Microsoft.VisualStudio.TestTools.UnitTesting;
using SonarAnalyzer.UnitTest.MetadataReferences;
using SonarAnalyzer.UnitTest.TestFramework;
using CS = SonarAnalyzer.Rules.CSharp;
using VB = SonarAnalyzer.Rules.VisualBasic;

namespace SonarAnalyzer.UnitTest.Rules
{
    [TestClass]
    public class JwtSignedTest
    {
        [TestMethod]
        public void JwtSigned_CS() =>
            Verifier.VerifyAnalyzer(@"TestCases\JwtSigned.cs", new CS.JwtSigned(), NuGetMetadataReference.JWT("6.1.0"));

        [TestMethod]
        public void JwtSigned_JWTDecoderExtensions_CS() =>
            Verifier.VerifyAnalyzer(@"TestCases\JwtSigned.Extensions.cs", new CS.JwtSigned(), NuGetMetadataReference.JWT("7.3.1"));

#if NET
        [TestMethod]
        public void JwtSigned_CS_FromCSharp9() =>
            Verifier.VerifyAnalyzerFromCSharp9Console(@"TestCases\JwtSigned.CSharp9.cs", new CS.JwtSigned(), NuGetMetadataReference.JWT("6.1.0"));
#endif

        [TestMethod]
        public void JwtSigned_VB() =>
            Verifier.VerifyAnalyzer(@"TestCases\JwtSigned.vb", new VB.JwtSigned(), NuGetMetadataReference.JWT("6.1.0"));

        [TestMethod]
        public void JwtSigned_JWTDecoderExtensions_VB() =>
            Verifier.VerifyAnalyzer(@"TestCases\JwtSigned.Extensions.vb", new VB.JwtSigned(), NuGetMetadataReference.JWT("7.3.1"));
    }
}
