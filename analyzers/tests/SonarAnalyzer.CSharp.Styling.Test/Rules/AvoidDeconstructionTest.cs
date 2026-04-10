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

namespace SonarAnalyzer.CSharp.Styling.Rules.Test;

[TestClass]
public class AvoidDeconstructionTest
{
    [TestMethod]
    public void AvoidDeconstruction() =>
        StylingVerifierBuilder.Verify<AvoidDeconstruction>();

    [TestMethod]
    public void AvoidDeconstruction_TopLevelStatement() =>
        StylingVerifierBuilder.Create<AvoidDeconstruction>()
            .AddPaths("AvoidDeconstruction.TopLevelStatements.cs")
            .WithTopLevelStatements()
            .Verify();
}
