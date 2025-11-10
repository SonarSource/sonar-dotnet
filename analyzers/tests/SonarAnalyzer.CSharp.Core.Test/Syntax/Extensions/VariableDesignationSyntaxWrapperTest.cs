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

namespace SonarAnalyzer.CSharp.Core.Test.Syntax.Extensions;

[TestClass]
public class VariableDesignationSyntaxWrapperTest
{
    [TestMethod]
    [DataRow("var (a, b) = (1, 2);", "a,b")]
    [DataRow("var (a, _) = (1, 2);", "a")]
    [DataRow("var (a, (b, c), d) = (1, (2, 3), 4);", "a,b,c,d")]
    [DataRow("_ = (1, 2) is var (a, b);", "a,b")]
    [DataRow("_ = (1, 2) switch { var (a, b) => true };", "a,b")]
    public void VariableDesignationSyntaxWrapper_DifferentDesignations(string designation, string expectedVariables)
    {
        var syntaxTree = CSharpSyntaxTree.ParseText(WrapInMethod(designation));
        var variableDesignation = (VariableDesignationSyntaxWrapper)syntaxTree.GetRoot().DescendantNodesAndSelf().First(VariableDesignationSyntaxWrapper.IsInstance);
        var allVariables = variableDesignation.AllVariables();
        var allVariablesAsString = string.Join(",", allVariables.Select(x => x.SyntaxNode.ToString()));
        allVariablesAsString.Should().Be(expectedVariables);
    }

    private static string WrapInMethod(string code) =>
$@"
public class C
{{
    public void M()
    {{
        {code}
    }}
}}
";
}
