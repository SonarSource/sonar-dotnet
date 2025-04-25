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
public class StatementSyntaxExtensionsTest
{
    [TestMethod]
    public void GetPrecedingStatement()
    {
        var source = """
            namespace Test
            {
                class TestClass
                {
                    int a = 1;
                    public void DoSomething(){}
                    public void IfMethod()
                    {
                        if (a>1)
                            DoSomething();
                        if (a<0)
                            DoSomething();
                    }
                }
            }
            """;
        var ifMethodStatements = DescendantNodes<MethodDeclarationSyntax>(source)
            .First(m => m.Identifier.ValueText == "IfMethod")
            .Body.Statements.ToList();

        var ifStatementA = ifMethodStatements[0];
        var ifStatementB = ifMethodStatements[1];
        ifStatementB.GetPrecedingStatement().Should().BeEquivalentTo(ifStatementA);
        ifStatementA.GetPrecedingStatement().Should().Be(null);
    }

    [TestMethod]
    public void GetPrecedingStatementTopLevelStatements()
    {
        var sourceTopLevelStatement = """
            var a = 1;
            var b = 2;
            if (a == b)
            {
                DoSomething();
            }
            void DoSomething() { }
            """;
        var variableDeclarators = DescendantNodes<LocalDeclarationStatementSyntax>(sourceTopLevelStatement).ToArray();
        var aDeclaration = variableDeclarators[0];
        var bDeclaration = variableDeclarators[1];
        aDeclaration.GetPrecedingStatement().Should().Be(null);
        bDeclaration.GetPrecedingStatement().Should().BeEquivalentTo(aDeclaration);
    }

    [TestMethod]
    public void FirstNonBlockStatement_NoBlock()
    {
        var source = """
            namespace Test
            {
                class TestClass
                {
                    public void IfMethod()
                    {
                        if (a > 1)
                            DoSomething();
                        if (a < 0)
                        {
                            DoSomething();
                        }
                        if (a == 42)
                        {
                            {
                                DoSomething();
                                DoSomething();
                            }
                        }
                    }
                }
            }
            """;
        var ifStatements = DescendantNodes<IfStatementSyntax>(source)
            .Select(x => x.Statement)
            .ToArray();
        var expressionStatements = DescendantNodes<ExpressionStatementSyntax>(source).ToArray();
        ifStatements[0].FirstNonBlockStatement().Span.Should().Be(expressionStatements[0].Span);
        ifStatements[1].FirstNonBlockStatement().Span.Should().Be(expressionStatements[1].Span);
        ifStatements[2].FirstNonBlockStatement().Span.Should().Be(expressionStatements[2].Span);
    }

    private static IEnumerable<T> DescendantNodes<T>(string source) =>
        CSharpSyntaxTree.ParseText(source)
            .GetRoot()
            .DescendantNodes()
            .OfType<T>();
}
