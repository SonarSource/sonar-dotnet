/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2014-2025 SonarSource SA
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

namespace SonarAnalyzer.CSharp.Core.Test.Syntax.Extensions;

[TestClass]
public class TupleExpressionSyntaxExtensionsTest
{
    [DataTestMethod]
    [DataRow("(1, 2)", "1,2")]
    [DataRow("(1, (2, 3))", "1,2,3")]
    [DataRow("(1, (2, 3), 4)", "1,2,3,4")]
    [DataRow("(1, (2, 3), 4, M())", "1,2,3,4,M()")]
    [DataRow("(1, (2, 3, (4, 5, 6), 7), 8, M())", "1,2,3,4,5,6,7,8,M()")]
    public void TupleExpressionSyntaxExtensions_FlatteningTests(string tuple, string expectedArguments)
    {
        var syntaxTree = CSharpSyntaxTree.ParseText(WrapInMethod(tuple));
        var tupleExpression = (TupleExpressionSyntaxWrapper)syntaxTree.GetRoot().DescendantNodesAndSelf().First(x => TupleExpressionSyntaxWrapper.IsInstance(x));
        var allArguments = tupleExpression.AllArguments();
        var allArgumentsAsString = string.Join(",", allArguments.Select(x => x.ToString()));
        allArgumentsAsString.Should().Be(expectedArguments);
    }

    private static string WrapInMethod(string code) =>
$@"
public class C
{{
    public int M()
    {{
        var t = {code};
        return 0;
    }}
}}
";
}
