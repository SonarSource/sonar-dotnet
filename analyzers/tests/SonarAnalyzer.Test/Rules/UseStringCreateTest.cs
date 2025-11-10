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

namespace SonarAnalyzer.Test.Rules;

[TestClass]
public class UseStringCreateTest
{
    private readonly VerifierBuilder builderCS = new VerifierBuilder<CS.UseStringCreate>();

#if NET6_0_OR_GREATER

    [TestMethod]
    public void UseStringCreate_CSharp10() =>
        builderCS.AddPaths("UseStringCreate.CSharp10.cs")
            .WithOptions(LanguageOptions.FromCSharp10)
            .Verify();

#else

    [TestMethod]
    public void UseStringCreate() =>
        builderCS.AddPaths("UseStringCreate.cs").Verify();

#endif

}
