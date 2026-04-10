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

using SonarAnalyzer.CFG.Extensions;

namespace SonarAnalyzer.CSharp.Core.Test.Syntax.Extensions;

[TestClass]
public class PatternSyntaxWrapperExtensionsTest
{
    [TestMethod]
    public void IsNull_ForNullPattern_ReturnsTrue()
    {
        var isPattern = (IsPatternExpressionSyntaxWrapper)SyntaxFactory.ParseExpression("is null");
        isPattern.Pattern.IsNull().Should().BeTrue();
    }

    [TestMethod]
    public void IsNull_ForDifferentPattern_ReturnsFalse()
    {
        var isPattern = (IsPatternExpressionSyntaxWrapper)SyntaxFactory.ParseExpression("is not 1");
        isPattern.Pattern.IsNull().Should().BeFalse();
    }
}
