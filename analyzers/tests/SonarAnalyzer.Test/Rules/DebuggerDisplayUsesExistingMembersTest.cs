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

namespace SonarAnalyzer.Test.Rules;

[TestClass]
public class DebuggerDisplayUsesExistingMembersTest
{
    private readonly VerifierBuilder builderCS = new VerifierBuilder<CS.DebuggerDisplayUsesExistingMembers>();
    private readonly VerifierBuilder builderVB = new VerifierBuilder<VB.DebuggerDisplayUsesExistingMembers>();

    [TestMethod]
    public void DebuggerDisplayUsesExistingMembers_CS() =>
        builderCS.AddPaths("DebuggerDisplayUsesExistingMembers.cs").Verify();

#if NET

    [TestMethod]
    public void DebuggerDisplayUsesExistingMembers_CSharp_Latest() =>
        builderCS.AddPaths("DebuggerDisplayUsesExistingMembers.Latest.cs")
            .WithOptions(LanguageOptions.CSharpLatest)
            .Verify();

#endif

    [TestMethod]
    public void DebuggerDisplayUsesExistingMembers_VB() =>
        builderVB.AddPaths("DebuggerDisplayUsesExistingMembers.vb").WithOptions(LanguageOptions.FromVisualBasic14).Verify();
}
