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

using Microsoft.VisualStudio.TestTools.UnitTesting;
using SonarAnalyzer.UnitTest.TestFramework;
using CS = SonarAnalyzer.Rules.CSharp;
using VB = SonarAnalyzer.Rules.VisualBasic;

namespace SonarAnalyzer.UnitTest.Rules
{
    [TestClass]
    public class BeginInvokePairedWithEndInvokeTest
    {
        private readonly VerifierBuilder builderCS = new VerifierBuilder<CS.BeginInvokePairedWithEndInvoke>();
        private readonly VerifierBuilder builderVB = new VerifierBuilder<VB.BeginInvokePairedWithEndInvoke>();

        [TestMethod]
        public void BeginInvokePairedWithEndInvoke_CS() =>
            builderCS.AddPaths("BeginInvokePairedWithEndInvoke.cs").Verify();

#if NET

        [TestMethod]
        public void BeginInvokePairedWithEndInvoke_CSharp9() =>
            builderCS.AddPaths("BeginInvokePairedWithEndInvoke.CSharp9.Part1.cs", "BeginInvokePairedWithEndInvoke.CSharp9.Part2.cs").WithOptions(ParseOptionsHelper.FromCSharp9).Verify();

        [TestMethod]
        public void BeginInvokePairedWithEndInvoke_CSharp10() =>
            builderCS.AddPaths("BeginInvokePairedWithEndInvoke.CSharp10.cs").WithOptions(ParseOptionsHelper.FromCSharp10).Verify();

#endif

        [TestMethod]
        public void BeginInvokePairedWithEndInvoke_VB() =>
            builderVB.AddPaths("BeginInvokePairedWithEndInvoke.vb").Verify();
    }
}
