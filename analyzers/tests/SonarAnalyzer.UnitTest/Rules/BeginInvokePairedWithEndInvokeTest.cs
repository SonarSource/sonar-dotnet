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
    public class BeginInvokePairedWithEndInvokeTest
    {
        [TestMethod]
        public void BeginInvokePairedWithEndInvoke_CS() =>
            Verifier.VerifyAnalyzer(@"TestCases\BeginInvokePairedWithEndInvoke.cs", new CS.BeginInvokePairedWithEndInvoke());

#if NET
        [TestMethod]
        public void BeginInvokePairedWithEndInvoke_CSharp9() =>
            Verifier.VerifyAnalyzerFromCSharp9Library(new[] { @"TestCases\BeginInvokePairedWithEndInvoke.CSharp9.Part1.cs", @"TestCases\BeginInvokePairedWithEndInvoke.CSharp9.Part2.cs" },
                new CS.BeginInvokePairedWithEndInvoke());

        [TestMethod]
        public void BeginInvokePairedWithEndInvoke_CSharp10() =>
            Verifier.VerifyAnalyzerFromCSharp10Library(new[] { @"TestCases\BeginInvokePairedWithEndInvoke.CSharp10.cs" },
                new CS.BeginInvokePairedWithEndInvoke());
#endif

        [TestMethod]
        public void BeginInvokePairedWithEndInvoke_VB() =>
            Verifier.VerifyAnalyzer(@"TestCases\BeginInvokePairedWithEndInvoke.vb", new VB.BeginInvokePairedWithEndInvoke());
    }
}
