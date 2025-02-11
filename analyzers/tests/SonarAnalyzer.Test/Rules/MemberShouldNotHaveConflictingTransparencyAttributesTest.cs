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

using SonarAnalyzer.CSharp.Rules;

namespace SonarAnalyzer.Test.Rules
{
    [TestClass]
    public class MemberShouldNotHaveConflictingTransparencyAttributesTest
    {
        private readonly VerifierBuilder builder = new VerifierBuilder<MemberShouldNotHaveConflictingTransparencyAttributes>();

        [TestMethod]
        public void MemberShouldNotHaveConflictingTransparencyAttributes() =>
            builder.AddPaths("MemberShouldNotHaveConflictingTransparencyAttributes.cs").Verify();

        [TestMethod]
        public void MemberShouldNotHaveConflictingTransparencyAttributes_AssemblyLevel() =>
            builder.AddPaths("MemberShouldNotHaveConflictingTransparencyAttributes_AssemblyLevel.cs").WithConcurrentAnalysis(false).Verify();

#if NET

        [TestMethod]
        public void MemberShouldNotHaveConflictingTransparencyAttributes_CSharp10() =>
            builder.AddPaths("MemberShouldNotHaveConflictingTransparencyAttributes.CSharp10.cs").WithConcurrentAnalysis(false).WithOptions(LanguageOptions.FromCSharp10).Verify();

        [TestMethod]
        public void MemberShouldNotHaveConflictingTransparencyAttributes_CSharp11() =>
            builder.AddPaths("MemberShouldNotHaveConflictingTransparencyAttributes.CSharp11.cs").WithConcurrentAnalysis(false).WithOptions(LanguageOptions.FromCSharp11).Verify();

#endif

    }
}
