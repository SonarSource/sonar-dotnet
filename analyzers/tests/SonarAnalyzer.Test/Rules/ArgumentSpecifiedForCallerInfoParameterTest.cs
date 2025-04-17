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

namespace SonarAnalyzer.Test.Rules;

[TestClass]
public class ArgumentSpecifiedForCallerInfoParameterTest
{
    private readonly VerifierBuilder builder = new VerifierBuilder<ArgumentSpecifiedForCallerInfoParameter>();

    [TestMethod]
    public void ArgumentSpecifiedForCallerInfoParameter() =>
        builder.AddPaths("ArgumentSpecifiedForCallerInfoParameter.cs").Verify();

#if NET

    [TestMethod]
    public void ArgumentSpecifiedForCallerInfoParameter_CSharp10() =>
        builder.AddPaths("ArgumentSpecifiedForCallerInfoParameter.CSharp10.cs")
            .WithOptions(LanguageOptions.FromCSharp10)
            .Verify();

    [TestMethod]
    public void ArgumentSpecifiedForCallerInfoParameter_CSharp11() =>
        builder.AddPaths("ArgumentSpecifiedForCallerInfoParameter.CSharp11.cs")
            .WithOptions(LanguageOptions.FromCSharp11)
            .Verify();

#endif

}
