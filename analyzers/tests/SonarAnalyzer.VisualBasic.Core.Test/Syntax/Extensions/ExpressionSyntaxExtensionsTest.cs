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

namespace SonarAnalyzer.VisualBasic.Core.Syntax.Extensions.Test;

[TestClass]
public class ExpressionSyntaxExtensionsTest
{
    [TestMethod]
    [DataRow("a", "a")]
    [DataRow("Nothing", "null")]
    [DataRow("a + b", "null")]
    [DataRow("Me.a", "a")]
    [DataRow("Me.a.b", "a")]
    [DataRow("a.b", "a")]
    [DataRow("a.b()", "a")]
    [DataRow("a.b().c", "a")]
    [DataRow("a()", "a")]
    [DataRow("a().b", "a")]
    [DataRow("(a.b).c", "a")]
    [DataRow("a.b?.c.d(e)?(f).g?.h", "a")]
    [DataRow("Integer.MaxValue", "Integer")]
    public void LeftMostInMemberAccess(string expression, string expected)
    {
        var parsed = SyntaxFactory.ParseExpression(expression);
        var result = parsed.LeftMostInMemberAccess;
        var asString = result?.ToString() ?? "null";
        asString.Should().BeEquivalentTo(expected);
    }
}
