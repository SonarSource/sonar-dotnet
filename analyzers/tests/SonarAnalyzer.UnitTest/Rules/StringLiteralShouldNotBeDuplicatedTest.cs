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
using SonarAnalyzer.UnitTest.TestFramework;
using CS = SonarAnalyzer.Rules.CSharp;
using VB = SonarAnalyzer.Rules.VisualBasic;

namespace SonarAnalyzer.UnitTest.Rules
{
    [TestClass]
    public class StringLiteralShouldNotBeDuplicatedTest
    {
        [TestMethod]
        [TestCategory("Rule")]
        public void StringLiteralShouldNotBeDuplicated_CS() =>
            Verifier.VerifyAnalyzer(@"TestCases\StringLiteralShouldNotBeDuplicated.cs", new CS.StringLiteralShouldNotBeDuplicated());

#if NET
        [TestMethod]
        [TestCategory("Rule")]
        public void StringLiteralShouldNotBeDuplicated_CSharp9() =>
            Verifier.VerifyAnalyzerFromCSharp9Console(@"TestCases\StringLiteralShouldNotBeDuplicated.CSharp9.cs", new CS.StringLiteralShouldNotBeDuplicated());
#endif

        [TestMethod]
        [TestCategory("Rule")]
        public void StringLiteralShouldNotBeDuplicated_Attributes_CS() =>
            Verifier.VerifyNonConcurrentAnalyzer(@"TestCases\StringLiteralShouldNotBeDuplicated_Attributes.cs", new CS.StringLiteralShouldNotBeDuplicated { Threshold = 2 });

        [TestMethod]
        [TestCategory("Rule")]
        public void StringLiteralShouldNotBeDuplicated_VB() =>
            Verifier.VerifyAnalyzer(@"TestCases\StringLiteralShouldNotBeDuplicated.vb", new VB.StringLiteralShouldNotBeDuplicated());

        [TestMethod]
        [TestCategory("Rule")]
        public void StringLiteralShouldNotBeDuplicated_Attributes_VB() =>
           Verifier.VerifyNonConcurrentAnalyzer(@"TestCases\StringLiteralShouldNotBeDuplicated_Attributes.vb", new VB.StringLiteralShouldNotBeDuplicated { Threshold = 2 });
    }
}
