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
public class DateTimeFormatShouldNotBeHardcodedTest
{
    private readonly VerifierBuilder<CS.DateTimeFormatShouldNotBeHardcoded> builderCS = new();
    private readonly VerifierBuilder<VB.DateTimeFormatShouldNotBeHardcoded> builderVB = new();

    [TestMethod]
    public void DateTimeFormatShouldNotBeHardcoded_CS() =>
        builderCS.AddPaths("DateTimeFormatShouldNotBeHardcoded.cs").Verify();

    [TestMethod]
    public void DateTimeFormatShouldNotBeHardcoded_VB() =>
        builderVB.AddPaths("DateTimeFormatShouldNotBeHardcoded.vb").WithOptions(LanguageOptions.FromVisualBasic14).Verify();

#if NET

    [TestMethod]
    public void DateTimeFormatShouldNotBeHardcoded_CS_Latest() =>
        builderCS
            .AddPaths("DateTimeFormatShouldNotBeHardcoded.Latest.cs")
            .WithOptions(LanguageOptions.CSharpLatest)
            .Verify();

    [TestMethod]
    public void DateTimeFormatShouldNotBeHardcoded_NET_VB() =>
        builderVB.AddPaths("DateTimeFormatShouldNotBeHardcoded.Net.vb").Verify();

#endif

}
