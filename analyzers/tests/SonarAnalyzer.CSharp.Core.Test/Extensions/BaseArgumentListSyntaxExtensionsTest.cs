/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2014-2025 SonarSource Sàrl
 * mailto:info AT sonarsource DOT com
 * This program is free software; you can redistribute it and/or
 * modify it under the terms of the Sonar Source-Available License Version 1, as published by SonarSource Sàrl.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
 * See the Sonar Source-Available License for more details.
 *
 * You should have received a copy of the Sonar Source-Available License
 * along with this program; if not, see https://sonarsource.com/license/ssal/
 */

namespace SonarAnalyzer.CSharp.Core.Test.Extensions;

[TestClass]
public class BaseArgumentListSyntaxExtensionsTest
{
    [TestMethod]
    public void GivenEmptyList_GetArgumentByName_ReturnsNull() =>
        CreateNamedArgumentList().GetArgumentByName("argument").Should().BeNull();

    [TestMethod]
    public void GivenListWithAnotherNamedArgument_GetArgumentByName_ReturnsNull() =>
        CreateNamedArgumentList("p1").GetArgumentByName("p2").Should().BeNull();

    [TestMethod]
    public void GivenListWithNamedArgument_GetArgumentByName_ReturnsArgument() =>
        CreateNamedArgumentList("p1").GetArgumentByName("p1").Should().Match(x => ((ArgumentSyntax)x).NameColon.Name.Identifier.Text == "p1");

    [TestMethod]
    public void GivenListWithMultipleNamedArguments_GetArgumentByName_ReturnsArgument() =>
        CreateNamedArgumentList("p1", "p2", "p3").GetArgumentByName("p2").Should().Match(x =>  ((ArgumentSyntax)x).NameColon.Name.Identifier.Text == "p2");

    [TestMethod]
    public void GivenListWithNotNamedArguments_GetArgumentByName_ReturnsNull() =>
        SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList(
                new[]
                {
                    CreateNotNamedArgument("p1"),
                    CreateNotNamedArgument("p2")
                }))
            .GetArgumentByName("p1")
            .Should().BeNull();

    [TestMethod]
    public void GivenListWithMixedNotNamedAndNamedArguments_GetArgumentByName_ReturnsNull() =>
        SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList(
                new[]
                {
                    CreateNamedArgument("p1"),
                    CreateNotNamedArgument("p2"),
                    CreateNotNamedArgument("p3")
                }))
            .GetArgumentByName("p1")
            .Should().Match(x =>  ((ArgumentSyntax)x).NameColon.Name.Identifier.Text == "p1");

    private static ArgumentListSyntax CreateNamedArgumentList(params string[] names) =>
        SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList(names.Select(CreateNamedArgument)));

    private static ArgumentSyntax CreateNamedArgument(string name) =>
        SyntaxFactory.Argument(SyntaxFactory.NameColon(name), SyntaxFactory.Token(SyntaxKind.None), SyntaxFactory.IdentifierName(name));

    private static ArgumentSyntax CreateNotNamedArgument(string identifierName) =>
        SyntaxFactory.Argument(SyntaxFactory.IdentifierName(identifierName));
}
