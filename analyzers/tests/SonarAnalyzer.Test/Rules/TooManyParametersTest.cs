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
public class TooManyParametersTest
{
    private readonly VerifierBuilder builderCSMax3 = new VerifierBuilder().AddAnalyzer(() => new CS.TooManyParameters { Maximum = 3 });
    private readonly VerifierBuilder builderVBMax3 = new VerifierBuilder().AddAnalyzer(() => new VB.TooManyParameters { Maximum = 3 });

    [TestMethod]
    public void TooManyParameters_CS_CustomValues() =>
        builderCSMax3.AddPaths("TooManyParameters_CustomValues.cs")
            .WithOptions(LanguageOptions.FromCSharp8)
            .Verify();

    [TestMethod]
    public void TooManyParameters_CS_CustomValues_TopLevelStatements() =>
         builderCSMax3.AddPaths("TooManyParameters_CustomValues.TopLevelStatements.cs")
            .WithTopLevelStatements()
            .Verify();

    [TestMethod]
    public void TooManyParameters_CS_CustomValues_Latest() =>
        builderCSMax3.AddPaths("TooManyParameters_CustomValues.Latest.cs")
            .WithOptions(LanguageOptions.CSharpLatest)
            .Verify();

    [TestMethod]
    public void TooManyParameters_VB_CustomValues() =>
        builderVBMax3.AddPaths("TooManyParameters_CustomValues.vb").Verify();

    [TestMethod]
    public void TooManyParameters_CS_DefaultValues() =>
        new VerifierBuilder<CS.TooManyParameters>().AddPaths("TooManyParameters_DefaultValues.cs")
            .WithOptions(LanguageOptions.FromCSharp8)
            .Verify();

    [TestMethod]
    public void TooManyParameters_VB_DefaultValues() =>
        new VerifierBuilder<VB.TooManyParameters>().AddPaths("TooManyParameters_DefaultValues.vb").Verify();
}
