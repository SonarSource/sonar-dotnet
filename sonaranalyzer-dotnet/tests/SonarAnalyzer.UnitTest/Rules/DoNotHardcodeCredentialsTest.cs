/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2019 SonarSource SA
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

extern alias csharp;
extern alias vbnet;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using CSharp = csharp::SonarAnalyzer.Rules.CSharp;
using VisualBasic = vbnet::SonarAnalyzer.Rules.VisualBasic;

namespace SonarAnalyzer.UnitTest.Rules
{
    [TestClass]
    public class DoNotHardcodeCredentialsTest
    {
        [TestMethod]
        [TestCategory("Rule")]
        public void DoNotHardcodeCredentials_CS_DefaultValues()
        {
            Verifier.VerifyAnalyzer(@"TestCases\DoNotHardcodeCredentials_DefaultValues.cs",
                new CSharp.DoNotHardcodeCredentials());
        }

        [TestMethod]
        [TestCategory("Rule")]
        public void DoNotHardcodeCredentials_CS_CustomValues()
        {
            Verifier.VerifyAnalyzer(@"TestCases\DoNotHardcodeCredentials_CustomValues.cs",
                new CSharp.DoNotHardcodeCredentials { CredentialWords = "kode,facal-faire" });
        }

        [TestMethod]
        [TestCategory("Rule")]
        public void DoNotHardcodeCredentials_CS_CustomValues_CaseInsensitive()
        {
            Verifier.VerifyAnalyzer(@"TestCases\DoNotHardcodeCredentials_CustomValues.cs",
                new CSharp.DoNotHardcodeCredentials { CredentialWords = "KODE ,,,, FaCaL-FaIrE " });
        }

        [TestMethod]
        [TestCategory("Rule")]
        public void DoNotHardcodeCredentials_VB_DefaultValues()
        {
            Verifier.VerifyAnalyzer(@"TestCases\DoNotHardcodeCredentials_DefaultValues.vb",
                new VisualBasic.DoNotHardcodeCredentials());
        }

        [TestMethod]
        [TestCategory("Rule")]
        public void DoNotHardcodeCredentials_VB_CustomValues()
        {
            Verifier.VerifyAnalyzer(@"TestCases\DoNotHardcodeCredentials_CustomValues.vb",
                new VisualBasic.DoNotHardcodeCredentials { CredentialWords = "kode,facal-faire" });
        }

        [TestMethod]
        [TestCategory("Rule")]
        public void DoNotHardcodeCredentials_VB_CustomValues_CaseInsensitive()
        {
            Verifier.VerifyAnalyzer(@"TestCases\DoNotHardcodeCredentials_CustomValues.vb",
                new VisualBasic.DoNotHardcodeCredentials { CredentialWords = "KODE ,,,, FaCaL-FaIrE " });
        }
    }
}

