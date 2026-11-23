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

using SonarAnalyzer.CSharp.Rules;

namespace SonarAnalyzer.Test.Rules;

[TestClass]
public class IssueSuppressionTest
{
    private readonly VerifierBuilder builder = new VerifierBuilder<IssueSuppression>();

    [TestMethod]
    public void IssueSuppression() =>
        builder.AddPaths("IssueSuppression.cs", "IssueSuppression.Concurrent.cs").WithAutogenerateConcurrentFiles(false).Verify();

    [TestMethod]
    public void IssueSuppression_TopLevelStatements() =>
        builder.AddPaths("IssueSuppression.TopLevelStatements.cs").WithTopLevelStatements().Verify();

    [TestMethod]
    public void IssueSuppression_CS_Latest() =>
        builder.AddPaths("IssueSuppression.Latest.cs").WithOptions(LanguageOptions.CSharpLatest).Verify();
}
