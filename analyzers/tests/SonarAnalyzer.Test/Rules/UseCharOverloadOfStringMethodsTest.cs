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
public class UseCharOverloadOfStringMethodsTest
{
    private readonly VerifierBuilder builderCS = new VerifierBuilder<CS.UseCharOverloadOfStringMethods>();
    private readonly VerifierBuilder builderVB = new VerifierBuilder<VB.UseCharOverloadOfStringMethods>();

#if NETFRAMEWORK

    [TestMethod]
    public void UseCharOverloadOfStringMethods_CS() =>
        builderCS.AddPaths("UseCharOverloadOfStringMethods.Framework.cs").Verify();

    [TestMethod]
    public void UseCharOverloadOfStringMethods_VB() =>
        builderVB.AddPaths("UseCharOverloadOfStringMethods.Framework.vb").VerifyNoIssues();

#else

    [TestMethod]
    public void UseCharOverloadOfStringMethods_CS() =>
        builderCS.AddPaths("UseCharOverloadOfStringMethods.cs").Verify();

    [TestMethod]
    public void UseCharOverloadOfStringMethods_CS_Latest() =>
        builderCS.AddPaths("UseCharOverloadOfStringMethods.Latest.cs")
        .WithOptions(LanguageOptions.CSharpLatest)
        .Verify();

    [TestMethod]
    public void UseCharOverloadOfStringMethods_VB() =>
        builderVB.AddPaths("UseCharOverloadOfStringMethods.vb").Verify();

    [TestMethod]
    public void UseCharOverloadOfStringMethods_CS_Fix() =>
        builderCS
        .WithCodeFix<CS.UseCharOverloadOfStringMethodsCodeFix>()
        .AddPaths("UseCharOverloadOfStringMethods.ToFix.cs")
        .WithCodeFixedPaths("UseCharOverloadOfStringMethods.Fixed.cs")
        .VerifyCodeFix();

#endif

}
