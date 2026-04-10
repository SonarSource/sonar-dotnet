/*
 * SonarAnalyzer for .NET
 * Copyright (C) SonarSource Sàrl
 * mailto:info AT sonarsource DOT com
 *
 * You can redistribute and/or modify this program under the terms of
 * the Sonar Source-Available License Version 1, as published by SonarSource Sàrl.
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
public class UnnecessaryMathematicalComparisonTest
{
    private readonly VerifierBuilder builderCS = new VerifierBuilder<CS.UnnecessaryMathematicalComparison>()
        .WithWarningsAsErrors("CS0652");

    [TestMethod]
    public void UnnecessaryMathematicalComparison_CS() =>
        builderCS.AddPaths("UnnecessaryMathematicalComparison.cs").Verify();

    [TestMethod]
    public void UnnecessaryMathematicalComparison_CSharpLatest() =>
        builderCS.AddPaths("UnnecessaryMathematicalComparison.Latest.cs").WithOptions(LanguageOptions.CSharpLatest).Verify();
}
