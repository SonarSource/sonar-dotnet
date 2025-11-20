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
public class GenericTypeParameterUnusedTest
{
    private readonly VerifierBuilder builder = new VerifierBuilder<GenericTypeParameterUnused>();

    [TestMethod]
    public void GenericTypeParameterUnused() =>
        builder.AddPaths("GenericTypeParameterUnused.cs", "GenericTypeParameterUnused.Partial.cs").WithOptions(LanguageOptions.FromCSharp8).Verify();

    [TestMethod]
    public void GenericTypeParameterUnused_CSharp9() =>
        builder.AddPaths("GenericTypeParameterUnused.CSharp9.cs").WithTopLevelStatements().Verify();

    [TestMethod]
    public void GenericTypeParameterUnused_CSharp10() =>
        builder.AddPaths("GenericTypeParameterUnused.CSharp10.cs").WithOptions(LanguageOptions.FromCSharp10).Verify();

    [TestMethod]
    public void GenericTypeParameterUnused_CSharp11() =>
        builder.AddPaths("GenericTypeParameterUnused.CSharp11.cs").WithOptions(LanguageOptions.FromCSharp11).Verify();

    [TestMethod]
    public void GenericTypeParameterUnused_CSharp12() =>
        builder.AddPaths("GenericTypeParameterUnused.CSharp12.cs").WithOptions(LanguageOptions.FromCSharp12).VerifyNoIssues();
}
