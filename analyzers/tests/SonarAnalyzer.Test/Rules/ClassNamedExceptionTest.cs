/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2014-2024 SonarSource SA
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

using CS = SonarAnalyzer.Rules.CSharp;
using VB = SonarAnalyzer.Rules.VisualBasic;

namespace SonarAnalyzer.Test.Rules;

[TestClass]
public class ClassNamedExceptionTest
{
    private readonly VerifierBuilder builderCS = new VerifierBuilder<CS.ClassNamedException>();
    private readonly VerifierBuilder builderVB = new VerifierBuilder<VB.ClassNamedException>();

    [TestMethod]
    public void ClassNamedException_CS() =>
        builderCS
            .AddPaths("ClassNamedException.cs")
            .Verify();

    [TestMethod]
    public void ClassNamedException_FromCSharp9() =>
        builderCS
            .AddPaths("ClassNamedException.CSharp9.cs")
            .WithOptions(LanguageOptions.FromCSharp9)
            .VerifyNoIssues();

    [TestMethod]
    public void ClassNamedException_FromCSharp10() =>
        builderCS
            .AddPaths("ClassNamedException.CSharp10.cs")
            .WithOptions(LanguageOptions.FromCSharp10)
            .VerifyNoIssues();

    [TestMethod]
    public void ClassNamedException_VB() =>
        builderVB
            .AddPaths("ClassNamedException.vb")
            .Verify();

#if NETFRAMEWORK

    [TestMethod]
    public void ClassNamedException_Interop_CS() =>
        builderCS
            .AddPaths("ClassNamedException.Interop.cs")
            .VerifyNoIssues();

    [TestMethod]
    public void ClassNamedException_Interop_VB() =>
        builderVB
            .AddPaths("ClassNamedException.Interop.vb")
            .VerifyNoIssues();

#endif
}
