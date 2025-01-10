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
public class SwitchSectionSyntaxExtensionsTest
{
    private const string Source = """
        class TestClass
        {
            public void DoSomething(){}

            public void SwitchMethod()
            {
                var i = 5;
                switch(i)
                {
                    case 3:
                        DoSomething();
                        break;
                    case 5:
                        DoSomething();
                        break;
                    default:
                        DoSomething();
                        break;
                }
            }
        }
        """;

    private MethodDeclarationSyntax switchMethod;

    [TestInitialize]
    public void TestSetup() =>
        switchMethod = CSharpSyntaxTree.ParseText(Source).GetRoot().DescendantNodes().OfType<MethodDeclarationSyntax>().First(x => x.Identifier.ValueText == "SwitchMethod");

    [TestMethod]
    public void GetPrecedingSections()
    {
        var sections = switchMethod.DescendantNodes().OfType<SwitchSectionSyntax>().ToList();

        sections.Last().PrecedingSections().Should().HaveCount(2);
        sections.First().PrecedingSections().Should().BeEmpty();
        sections.Last().PrecedingSections().First().Should().BeEquivalentTo(sections.First());
    }
}
