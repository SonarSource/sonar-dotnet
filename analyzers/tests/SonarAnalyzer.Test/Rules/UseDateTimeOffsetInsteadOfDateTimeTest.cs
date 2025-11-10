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
public class UseDateTimeOffsetInsteadOfDateTimeTest
{
    private readonly VerifierBuilder builderCS = new VerifierBuilder<CS.UseDateTimeOffsetInsteadOfDateTime>();
    private readonly VerifierBuilder builderVB = new VerifierBuilder<VB.UseDateTimeOffsetInsteadOfDateTime>();

    [TestMethod]
    public void UseDateTimeInsteadOfDateTimeOffset_CS() =>
        builderCS.AddPaths("UseDateTimeOffsetInsteadOfDateTime.cs").Verify();

#if NET

    [TestMethod]
    public void UseDateTimeInsteadOfDateTimeOffset_CSharp9() =>
        builderCS.AddPaths("UseDateTimeOffsetInsteadOfDateTime.CSharp9.cs").WithTopLevelStatements().Verify();

    [TestMethod]
    public void UseDateTimeInsteadOfDateTimeOffset_VB_Net() =>
        builderVB.AddPaths("UseDateTimeOffsetInsteadOfDateTime.Net.vb").Verify();

#endif

    [TestMethod]
    public void UseDateTimeInsteadOfDateTimeOffset_VB() =>
        builderVB.AddPaths("UseDateTimeOffsetInsteadOfDateTime.vb").Verify();
}
