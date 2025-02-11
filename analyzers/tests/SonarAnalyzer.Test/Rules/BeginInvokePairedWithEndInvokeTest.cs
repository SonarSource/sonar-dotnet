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
            builderCS.AddPaths("BeginInvokePairedWithEndInvoke.CSharp9.Part1.cs", "BeginInvokePairedWithEndInvoke.CSharp9.Part2.cs")
                .WithOptions(LanguageOptions.FromCSharp9)
                .Verify();

        [TestMethod]
        public void BeginInvokePairedWithEndInvoke_CSharp10() =>
            builderCS.AddPaths("BeginInvokePairedWithEndInvoke.CSharp10.cs")
                .WithOptions(LanguageOptions.FromCSharp10)
                .Verify();

        [TestMethod]
        public void BeginInvokePairedWithEndInvoke_CSharp11() =>
            builderCS.AddPaths("BeginInvokePairedWithEndInvoke.CSharp11.cs")
                .WithOptions(LanguageOptions.FromCSharp11)
                .Verify();

#endif

        [TestMethod]
        public void BeginInvokePairedWithEndInvoke_VB() =>
            builderVB.AddPaths("BeginInvokePairedWithEndInvoke.vb").WithOptions(LanguageOptions.FromVisualBasic14).Verify();
    }
}
