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
    public void PrecedingStatement()
    {
        var ifMethodStatements = DescendantNodes<MethodDeclarationSyntax>()
            .First(x => x.Identifier.ValueText == "IfMethod")
            .Body.Statements.ToList();

        ifMethodStatements[2].PrecedingStatement().Should().BeEquivalentTo(ifMethodStatements[1]);
        ifMethodStatements[1].PrecedingStatement().Should().BeEquivalentTo(ifMethodStatements[0]);
        ifMethodStatements[0].PrecedingStatement().Should().Be(null);
    }

    [TestMethod]
    public void FollowingStatement()
    {
        var ifMethodStatements = DescendantNodes<MethodDeclarationSyntax>()
            .First(x => x.Identifier.ValueText == "IfMethod")
            .Body.Statements.ToList();

        ifMethodStatements[0].FollowingStatement().Should().BeEquivalentTo(ifMethodStatements[1]);
        ifMethodStatements[1].FollowingStatement().Should().BeEquivalentTo(ifMethodStatements[2]);
        ifMethodStatements[2].FollowingStatement().Should().Be(null);
    }

    [TestMethod]
    public void PrecedingStatementTopLevelStatements()
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
        aDeclaration.PrecedingStatement().Should().Be(null);
        bDeclaration.PrecedingStatement().Should().BeEquivalentTo(aDeclaration);
    }

    [TestMethod]
    public void FirstNonBlockStatement_NoBlock()
    {
        var ifStatements = DescendantNodes<IfStatementSyntax>()
            .Select(x => x.Statement)
            .ToArray();
        var expressionStatements = DescendantNodes<ExpressionStatementSyntax>().ToArray();
        ifStatements[0].FirstNonBlockStatement().Span.Should().Be(expressionStatements[0].Span);
        ifStatements[1].FirstNonBlockStatement().Span.Should().Be(expressionStatements[1].Span);
        ifStatements[2].FirstNonBlockStatement().Span.Should().Be(expressionStatements[2].Span);
    }

    private static IEnumerable<T> DescendantNodes<T>()
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
        return DescendantNodes<T>(source);
    }

    private static IEnumerable<T> DescendantNodes<T>(string source) =>
        CSharpSyntaxTree.ParseText(source)
            .GetRoot()
            .DescendantNodes()
            .OfType<T>();
}
