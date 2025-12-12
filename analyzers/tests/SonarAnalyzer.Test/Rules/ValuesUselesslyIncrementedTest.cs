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
public class ValuesUselesslyIncrementedTest
{
    private readonly VerifierBuilder builder = new VerifierBuilder<ValuesUselesslyIncremented>();

    [TestMethod]
    public void ValuesUselesslyIncremented() =>
        builder.AddPaths("ValuesUselesslyIncremented.cs").Verify();

    [TestMethod]
    public void ValuesUselesslyIncremented_TopLevelStatements() =>
        builder.AddPaths("ValuesUselesslyIncremented.TopLevelStatements.cs").WithTopLevelStatements().Verify();

    [TestMethod]
    public void ValuesUselesslyIncremented_CS_Latest() =>
        builder.AddPaths("ValuesUselesslyIncremented.Latest.cs").WithOptions(LanguageOptions.CSharpLatest).Verify();
}
