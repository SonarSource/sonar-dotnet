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

using SonarAnalyzer.TestFramework.Extensions;

namespace SonarAnalyzer.ShimLayer.Generator.Strategies.Test;

[TestClass]
public class OperationExtendStrategyTest
{
    [TestMethod]
    public void Generate_IOperation()
    {
        var sut = new OperationExtendStrategy(typeof(IOperation), []);
        var result = sut.Generate([]);
        result.Should().BeIgnoringLineEndings(
            """
            namespace SonarAnalyzer.ShimLayer; // Extend IOperation
            """);
    }
}
