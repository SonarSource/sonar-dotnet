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

using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SonarAnalyzer.CSharp.Syntax.Extensions;

namespace SonarAnalyzer.Test.Syntax.Extensions;

[TestClass]
public class IfStatementSyntaxExtensions
{
    private const string Source = """
        class TestClass
        {
            public void DoSomething(){}

            public void IfMethod()
            {
                if (true)
                    DoSomething();
                else if (true)
                    DoSomething();
                else
                {
                    if(true)
                        DoSomething();
                    else
                        DoSomething();
                }
            }

            public void NonChainedIfMethod()
            {
                if (true)
                    DoSomething();
                else
                {
                    DoSomething();
                    if(true)
                        DoSomething();
                }
            }

            public void NonChainedIfDifferentIfFirstStatement()
            {
                if (true)
                    DoSomething();
                else
                {
                    if (false)
                        DoSomething();
                    DoSomething();
                    if(true)
                        DoSomething();
                }
            }
        }
        """;
    private MethodDeclarationSyntax ifMethod;

    [TestInitialize]
    public void TestSetup() =>
        ifMethod = CSharpSyntaxTree.ParseText(Source).GetRoot().DescendantNodes().OfType<MethodDeclarationSyntax>().First(x => x.Identifier.ValueText == "IfMethod");

    [TestMethod]
    public void GetPrecedingIfsInConditionChain()
    {
        var ifStatements = ifMethod.DescendantNodes().OfType<IfStatementSyntax>().ToList();
        var precedingIfStatements = new List<IfStatementSyntax>();
        foreach (var ifStatement in ifStatements)
        {
            var sut = ifStatement.PrecedingIfsInConditionChain();
            sut.Should().BeEquivalentTo(precedingIfStatements);
            precedingIfStatements.Add(ifStatement);
        }
    }

    [TestMethod]
    public void GetPrecedingStatementsInConditionChain()
    {
        var ifStatements = ifMethod.DescendantNodes().OfType<IfStatementSyntax>().ToList();
        var precedingStatements = new List<StatementSyntax>();
        foreach (var ifStatement in ifStatements)
        {
            var sut = ifStatement.PrecedingStatementsInConditionChain();
            sut.Should().BeEquivalentTo(precedingStatements);
            precedingStatements.Add(ifStatement.Statement);
        }
    }

    [TestMethod]
    public void GetPrecedingConditionsInConditionChain()
    {
        var ifStatements = ifMethod.DescendantNodes().OfType<IfStatementSyntax>().ToList();
        var precedingConditions = new List<ExpressionSyntax>();
        foreach (var ifStatement in ifStatements)
        {
            var sut = ifStatement.PrecedingConditionsInConditionChain();
            sut.Should().BeEquivalentTo(precedingConditions);
            precedingConditions.Add(ifStatement.Condition);
        }
    }

    [TestMethod]
    public void GetPrecedingSections_Empty()
    {
        var sections = ifMethod.DescendantNodes().OfType<SwitchSectionSyntax>().ToList();

        sections.FirstOrDefault().PrecedingSections().Should().BeEmpty();
    }

    [TestMethod]
    public void GetPrecedingIfsNonChainedIsEmpty()
    {
        var nonChained = CSharpSyntaxTree.ParseText(Source).GetRoot().DescendantNodes().OfType<MethodDeclarationSyntax>().First(x => x.Identifier.ValueText == "NonChainedIfMethod");
        var ifStatements = nonChained.DescendantNodes().OfType<IfStatementSyntax>().ToList();
        foreach (var ifStatement in ifStatements)
        {
            var sut = ifStatement.PrecedingIfsInConditionChain();
            sut.Should().BeEmpty();
        }
    }

    [TestMethod]
    public void GetPrecedingIfsNonChainedDifferentIfIsEmpty()
    {
        var nonChained = CSharpSyntaxTree.ParseText(Source).GetRoot().DescendantNodes().OfType<MethodDeclarationSyntax>().First(x => x.Identifier.ValueText == "NonChainedIfDifferentIfFirstStatement");
        var lastIf = nonChained.DescendantNodes().OfType<IfStatementSyntax>().Last();

        var sut = lastIf.PrecedingIfsInConditionChain();
        sut.Should().BeEmpty();
    }
}
