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
public class IfStatementExtensionsTest
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
                    DoSomething();
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
        var ifStatement1 = ifMethod.DescendantNodes().OfType<IfStatementSyntax>().First();
        ifStatement1.PrecedingIfsInConditionChain().Should().BeEmpty();

        var ifStatement2 = ifMethod.DescendantNodes().OfType<IfStatementSyntax>().Last();
        var preceding = ifStatement2.PrecedingIfsInConditionChain();
        preceding.Should().ContainSingle();

        ifStatement1.Should().BeEquivalentTo(preceding[0]);
    }

    [TestMethod]
    public void GetPrecedingStatementsInConditionChain()
    {
        var ifStatement1 = ifMethod.DescendantNodes().OfType<IfStatementSyntax>().First();
        ifStatement1.PrecedingStatementsInConditionChain().Should().BeEmpty();

        var ifStatement2 = ifMethod.DescendantNodes().OfType<IfStatementSyntax>().Last();
        var preceding = ifStatement2.PrecedingStatementsInConditionChain().ToList();
        preceding.Should().ContainSingle();

        ifStatement1.Statement.Should().BeEquivalentTo(preceding[0]);
    }

    [TestMethod]
    public void GetPrecedingConditionsInConditionChain()
    {
        var ifStatement1 = ifMethod.DescendantNodes().OfType<IfStatementSyntax>().First();
        ifStatement1.PrecedingConditionsInConditionChain().Should().BeEmpty();

        var ifStatement2 = ifMethod.DescendantNodes().OfType<IfStatementSyntax>().Last();
        var preceding = ifStatement2.PrecedingConditionsInConditionChain().ToList();
        preceding.Should().ContainSingle();

        ifStatement1.Condition.Should().BeEquivalentTo(preceding[0]);
    }

    [TestMethod]
    public void GetPrecedingSections_Empty()
    {
        var sections = ifMethod.DescendantNodes().OfType<SwitchSectionSyntax>().ToList();

        sections.FirstOrDefault().PrecedingSections().Should().BeEmpty();
    }
}
