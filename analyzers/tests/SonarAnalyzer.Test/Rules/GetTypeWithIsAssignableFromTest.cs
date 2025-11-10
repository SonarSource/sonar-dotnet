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

using SonarAnalyzer.CSharp.Rules;

namespace SonarAnalyzer.Test.Rules;

[TestClass]
public class GetTypeWithIsAssignableFromTest
{
    private readonly VerifierBuilder builder = new VerifierBuilder<GetTypeWithIsAssignableFrom>();

    [TestMethod]
    public void GetTypeWithIsAssignableFrom() =>
        builder.AddPaths("GetTypeWithIsAssignableFrom.cs").Verify();

#if NET

    [TestMethod]
    public void GetTypeWithIsAssignableFrom_CSharp9() =>
        builder.AddPaths("GetTypeWithIsAssignableFrom.CSharp9.cs")
            .WithTopLevelStatements()
            .Verify();

    [TestMethod]
    public void GetTypeWithIsAssignableFrom_CSharp9_CodeFix() =>
        builder.AddPaths("GetTypeWithIsAssignableFrom.CSharp9.cs")
            .WithCodeFix<GetTypeWithIsAssignableFromCodeFix>()
            .WithCodeFixedPaths("GetTypeWithIsAssignableFrom.CSharp9.Fixed.cs")
            .WithTopLevelStatements()
            .VerifyCodeFix();

    [TestMethod]
    public void GetTypeWithIsAssignableFrom_CSharp10() =>
        builder.AddPaths("GetTypeWithIsAssignableFrom.CSharp10.cs")
            .WithTopLevelStatements()
            .WithOptions(LanguageOptions.FromCSharp10)
            .Verify();

    [TestMethod]
    public void GetTypeWithIsAssignableFrom_CSharp10_CodeFix() =>
        builder.AddPaths("GetTypeWithIsAssignableFrom.CSharp10.cs")
            .WithCodeFix<GetTypeWithIsAssignableFromCodeFix>()
            .WithCodeFixedPaths("GetTypeWithIsAssignableFrom.CSharp10.Fixed.cs")
            .WithTopLevelStatements()
            .WithOptions(LanguageOptions.FromCSharp10)
            .VerifyCodeFix();

    [TestMethod]
    public void GetTypeWithIsAssignableFrom_CSharp11() =>
        builder.AddPaths("GetTypeWithIsAssignableFrom.CSharp11.cs")
            .WithOptions(LanguageOptions.FromCSharp11)
            .VerifyNoIssues();

#endif

    [TestMethod]
    public void GetTypeWithIsAssignableFrom_CodeFix() =>
        builder.AddPaths("GetTypeWithIsAssignableFrom.cs")
            .WithCodeFix<GetTypeWithIsAssignableFromCodeFix>()
            .WithCodeFixedPaths("GetTypeWithIsAssignableFrom.Fixed.cs", "GetTypeWithIsAssignableFrom.Fixed.Batch.cs")
            .VerifyCodeFix();
}
