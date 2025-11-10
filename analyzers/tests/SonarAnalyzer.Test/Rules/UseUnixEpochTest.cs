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

namespace SonarAnalyzer.Test.Rules;

[TestClass]
public class UseUnixEpochTest
{
    private readonly VerifierBuilder builderCS = new VerifierBuilder<CS.UseUnixEpoch>();
    private readonly VerifierBuilder builderVB = new VerifierBuilder<VB.UseUnixEpoch>();

#if NETFRAMEWORK

    [TestMethod]
    public void UseUnixEpoch_Framework_CS() =>
        builderCS.AddPaths("UseUnixEpoch.Framework.cs").VerifyNoIssues();

    [TestMethod]
    public void UseUnixEpoch_Framework_VB() =>
        builderVB.AddPaths("UseUnixEpoch.Framework.vb").VerifyNoIssues();

#else

    [TestMethod]
    public void UseUnixEpoch_CS() =>
        builderCS.AddPaths("UseUnixEpoch.cs").Verify();

    [TestMethod]
    public void UseUnixEpoch_CSharp9() =>
        builderCS
            .AddPaths("UseUnixEpoch.CSharp9.cs")
            .WithTopLevelStatements()
            .Verify();

    [TestMethod]
    public void UseUnixEpoch_VB() =>
        builderVB.AddPaths("UseUnixEpoch.vb").Verify();

    [TestMethod]
    public void UseUnixEpoch_CodeFix_CS() =>
        builderCS
            .AddPaths("UseUnixEpoch.cs")
            .WithCodeFix<CS.UseUnixEpochCodeFix>()
            .WithCodeFixedPaths("UseUnixEpoch.Fixed.cs")
            .VerifyCodeFix();

    [TestMethod]
    public void UseUnixEpoch_CodeFix_CSharp9() =>
        builderCS
            .AddPaths("UseUnixEpoch.CSharp9.cs")
            .WithCodeFix<CS.UseUnixEpochCodeFix>()
            .WithCodeFixedPaths("UseUnixEpoch.CSharp9.Fixed.cs")
            .WithTopLevelStatements()
            .VerifyCodeFix();

    [TestMethod]
    public void UseUnixEpoch_CodeFix_VB() =>
        builderVB
            .AddPaths("UseUnixEpoch.vb")
            .WithCodeFix<VB.UseUnixEpochCodeFix>()
            .WithCodeFixedPaths("UseUnixEpoch.Fixed.vb")
            .VerifyCodeFix();

#endif

}
