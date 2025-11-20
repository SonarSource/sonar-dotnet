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
public class VariableUnusedTest
{
    private readonly VerifierBuilder builderCS = new VerifierBuilder<CS.VariableUnused>();

    [TestMethod]
    public void VariableUnused_CS() =>
        builderCS.AddPaths("VariableUnused.cs").WithOptions(LanguageOptions.FromCSharp8).Verify();

    [TestMethod]
    public void VariableUnused_CSharp9() =>
        builderCS.AddPaths("VariableUnused.CSharp9.cs").WithTopLevelStatements().Verify();

    [TestMethod]
    public void VariableUnused_CSharp10() =>
        builderCS.AddPaths("VariableUnused.CSharp10.cs").WithOptions(LanguageOptions.FromCSharp10).Verify();

    [TestMethod]
    public void VariableUnused_CSharp11() =>
        builderCS.AddPaths("VariableUnused.CSharp11.cs").WithOptions(LanguageOptions.FromCSharp11).Verify();

    [TestMethod]
    public void VariableUnused_CSharp12() =>
        builderCS.AddPaths("VariableUnused.CSharp12.cs").WithOptions(LanguageOptions.FromCSharp12).VerifyNoIssues();

    [TestMethod]
    public void VariableUnused_VB() =>
        new VerifierBuilder<VB.VariableUnused>().AddPaths("VariableUnused.vb").Verify();
}
