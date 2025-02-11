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
    public class MethodOverloadsShouldBeGroupedTest
    {
        private readonly VerifierBuilder builderCS = new VerifierBuilder<CS.MethodOverloadsShouldBeGrouped>();
        private readonly VerifierBuilder builderVB = new VerifierBuilder<VB.MethodOverloadsShouldBeGrouped>();

        [TestMethod]
        public void MethodOverloadsShouldBeGrouped_CS() =>
            builderCS.AddPaths("MethodOverloadsShouldBeGrouped.cs").Verify();

#if NET
        [TestMethod]
        public void MethodOverloadsShouldBeGrouped_CS_CSharp9() =>
            builderCS.AddPaths("MethodOverloadsShouldBeGrouped.CSharp9.cs").WithOptions(LanguageOptions.FromCSharp9).Verify();

        [TestMethod]
        public void MethodOverloadsShouldBeGrouped_CS_CSharp10() =>
            builderCS.AddPaths("MethodOverloadsShouldBeGrouped.CSharp10.cs").WithOptions(LanguageOptions.FromCSharp10).Verify();

        [TestMethod]
        public void MethodOverloadsShouldBeGrouped_CS_CSharp11() =>
            builderCS.AddPaths("MethodOverloadsShouldBeGrouped.CSharp11.cs").WithOptions(LanguageOptions.FromCSharp11).Verify();

        [TestMethod]
        public void MethodOverloadsShouldBeGrouped_CS_CSharp12() =>
            builderCS.AddPaths("MethodOverloadsShouldBeGrouped.CSharp12.cs").WithOptions(LanguageOptions.FromCSharp12).Verify();

#endif

        [TestMethod]
        public void MethodOverloadsShouldBeGrouped_VB() =>
            builderVB.AddPaths("MethodOverloadsShouldBeGrouped.vb").Verify();
    }
}
