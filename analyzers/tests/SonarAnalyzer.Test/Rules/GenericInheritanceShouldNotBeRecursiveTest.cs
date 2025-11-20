/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2014-2025 SonarSource Sàrl
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
    public class GenericInheritanceShouldNotBeRecursiveTest
    {
        private readonly VerifierBuilder builderCS = new VerifierBuilder<CS.GenericInheritanceShouldNotBeRecursive>();
        private readonly VerifierBuilder builderVB = new VerifierBuilder<VB.GenericInheritanceShouldNotBeRecursive>();

        [TestMethod]
        public void GenericInheritanceShouldNotBeRecursive_CS() =>
            builderCS.AddPaths("GenericInheritanceShouldNotBeRecursive.cs").Verify();

        [TestMethod]
        public void GenericInheritanceShouldNotBeRecursive_CSharp9() =>
            builderCS.AddPaths("GenericInheritanceShouldNotBeRecursive.CSharp9.cs").WithOptions(LanguageOptions.FromCSharp9).Verify();

        [TestMethod]
        public void GenericInheritanceShouldNotBeRecursive_VB() =>
            builderVB.AddPaths("GenericInheritanceShouldNotBeRecursive.vb").Verify();
    }
}
