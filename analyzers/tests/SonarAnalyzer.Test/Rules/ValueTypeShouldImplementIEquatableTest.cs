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
public class ValueTypeShouldImplementIEquatableTest
{
    private readonly VerifierBuilder builderCS = new VerifierBuilder<CS.ValueTypeShouldImplementIEquatable>();
    private readonly VerifierBuilder builderVB = new VerifierBuilder<VB.ValueTypeShouldImplementIEquatable>();

    [TestMethod]
    public void ValueTypeShouldImplementIEquatable_CS() =>
        builderCS.AddPaths("ValueTypeShouldImplementIEquatable.cs").WithOptions(LanguageOptions.FromCSharp8).Verify();

#if NET

    [TestMethod]
    public void ValueTypeShouldImplementIEquatable_CSharp10() =>
        builderCS.AddPaths("ValueTypeShouldImplementIEquatable.CSharp10.cs").WithOptions(LanguageOptions.FromCSharp10).VerifyNoIssues();

#endif

    [TestMethod]
    public void ValueTypeShouldImplementIEquatable_VB() =>
        builderVB.AddPaths("ValueTypeShouldImplementIEquatable.vb").Verify();
}
