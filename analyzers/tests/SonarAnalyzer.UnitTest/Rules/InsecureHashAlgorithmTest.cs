﻿/*
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
#if NET
using SonarAnalyzer.UnitTest.MetadataReferences;
#endif
using SonarAnalyzer.UnitTest.TestFramework;
using CS = SonarAnalyzer.Rules.CSharp;

namespace SonarAnalyzer.UnitTest.Rules
{
    [TestClass]
    public class InsecureHashAlgorithmTest
    {
#if NETFRAMEWORK // SHA1Managed is sealed in .Net Core and HMACRIPEMD160 is not available

        [TestMethod]
        [TestCategory("Rule")]
        public void InsecureHashAlgorithm_NetFx() =>
            Verifier.VerifyAnalyzer(@"TestCases\InsecureHashAlgorithm.NetFx.cs", new CS.InsecureHashAlgorithm());

#else

        [TestMethod]
        [TestCategory("Rule")]
        public void InsecureHashAlgorithm_CSharp9() =>
            Verifier.VerifyAnalyzerFromCSharp9Console(@"TestCases\InsecureHashAlgorithm.CSharp9.cs", new CS.InsecureHashAlgorithm(), MetadataReferenceFacade.SystemSecurityCryptography);

        [TestMethod]
        [TestCategory("Rule")]
        public void InsecureHashAlgorithm() =>
            Verifier.VerifyAnalyzerFromCSharp9Library(@"TestCases\InsecureHashAlgorithm.cs", new CS.InsecureHashAlgorithm(), MetadataReferenceFacade.SystemSecurityCryptography);
#endif
    }
}
