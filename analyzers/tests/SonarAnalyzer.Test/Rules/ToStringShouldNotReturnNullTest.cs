/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2014-2025 SonarSource Sàrl
 * mailto:info AT sonarsource DOT com
 * This program is free software; you can redistribute it and/or
 * modify it under the terms of the Sonar Source-Available License Version 1, as published by SonarSource Sàrl.
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
public class ToStringShouldNotReturnNullTest
{
    private readonly VerifierBuilder builderCS = new VerifierBuilder<CS.ToStringShouldNotReturnNull>();
    private readonly VerifierBuilder builderVB = new VerifierBuilder<VB.ToStringShouldNotReturnNull>();

    [TestMethod]
    public void ToStringShouldNotReturnNull_CS() =>
        builderCS.AddPaths("ToStringShouldNotReturnNull.cs").Verify();

#if NET

    [TestMethod]
    public void ToStringShouldNotReturnNull_CSharp9() =>
        builderCS
            .WithOptions(LanguageOptions.FromCSharp9)
            .WithTopLevelStatements()
            .AddPaths("ToStringShouldNotReturnNull.CSharp9.cs")
            .Verify();

    [TestMethod]
    public void ToStringShouldNotReturnNull_CSharp11() =>
        builderCS
            .WithOptions(LanguageOptions.FromCSharp11)
            .AddPaths("ToStringShouldNotReturnNull.CSharp11.cs")
            .VerifyNoIssues();

#endif

    [TestMethod]
    public void ToStringShouldNotReturnNull_VB() =>
        builderVB.AddPaths("ToStringShouldNotReturnNull.vb").Verify();
}
