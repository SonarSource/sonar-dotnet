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
        var source = @"
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
}";
        var ifMethodStatements = CSharpSyntaxTree.ParseText(source)
                                         .GetRoot()
                                         .DescendantNodes()
                                         .OfType<MethodDeclarationSyntax>()
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
        var sourceTopLevelStatement = @"
var a = 1;
var b = 2;
if (a == b)
{
    DoSomething();
}
void DoSomething() { }";
        var variableDeclarators = CSharpSyntaxTree.ParseText(sourceTopLevelStatement)
                                                  .GetRoot()
                                                  .DescendantNodes()
                                                  .OfType<LocalDeclarationStatementSyntax>()
                                                  .ToArray();
        var aDeclaration = variableDeclarators[0];
        var bDeclaration = variableDeclarators[1];
        aDeclaration.GetPrecedingStatement().Should().Be(null);
        bDeclaration.GetPrecedingStatement().Should().BeEquivalentTo(aDeclaration);
    }
}
