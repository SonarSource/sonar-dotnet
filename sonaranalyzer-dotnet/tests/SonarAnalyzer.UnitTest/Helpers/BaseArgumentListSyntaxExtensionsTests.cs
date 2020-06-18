/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2020 SonarSource SA
 * mailto: contact AT sonarsource DOT com
 *
 * This program is free software; you can redistribute it and/or
 * modify it under the terms of the GNU Lesser General Public
 * License as published by the Free Software Foundation; either
 * version 3 of the License, or (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
 * Lesser General Public License for more details.
 *
 * You should have received a copy of the GNU Lesser General Public License
 * along with this program; if not, write to the Free Software Foundation,
 * Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02110-1301, USA.
 */

using System.Linq;
using FluentAssertions;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SonarAnalyzer.Helpers;

namespace SonarAnalyzer.UnitTest.Helpers
{
    [TestClass]
    public class BaseArgumentListSyntaxExtensionsTests
    {
        [TestMethod]
        public void GivenEmptyList_GetArgumentByName_ReturnsNull() =>
            CreateNamedArgumentList().GetArgumentByName("argument").Should().BeNull();

        [TestMethod]
        public void GivenListWithAnotherNamedArgument_GetArgumentByName_ReturnsNull() =>
            CreateNamedArgumentList("p1").GetArgumentByName("p2").Should().BeNull();

        [TestMethod]
        public void GivenListWithNamedArgument_GetArgumentByName_ReturnsArgument() =>
            CreateNamedArgumentList("p1").GetArgumentByName("p1").Should().Match(p => ((ArgumentSyntax)p).NameColon.Name.Identifier.Text == "p1");

        [TestMethod]
        public void GivenListWithMultipleNamedArguments_GetArgumentByName_ReturnsArgument() =>
            CreateNamedArgumentList("p1", "p2", "p3").GetArgumentByName("p2").Should().Match(p =>  ((ArgumentSyntax)p).NameColon.Name.Identifier.Text == "p2");

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
                .Should().Match(p =>  ((ArgumentSyntax)p).NameColon.Name.Identifier.Text == "p1");

        private static ArgumentListSyntax CreateNamedArgumentList(params string[] names) =>
            SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList(names.Select(CreateNamedArgument)));

        private static ArgumentSyntax CreateNamedArgument(string name) =>
            SyntaxFactory.Argument(SyntaxFactory.NameColon(name), SyntaxFactory.Token(SyntaxKind.None), SyntaxFactory.IdentifierName(name));

        private static ArgumentSyntax CreateNotNamedArgument(string identifierName) =>
            SyntaxFactory.Argument(SyntaxFactory.IdentifierName(identifierName));
    }
}
