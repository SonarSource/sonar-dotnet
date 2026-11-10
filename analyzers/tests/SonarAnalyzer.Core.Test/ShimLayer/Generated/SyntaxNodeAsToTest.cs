/*
 * SonarAnalyzer for .NET
 * Copyright (C) SonarSource Sàrl
 * mailto:info AT sonarsource DOT com
 *
 * You can redistribute and/or modify this program under the terms of
 * the Sonar Source-Available License Version 1, as published by SonarSource Sàrl.
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

namespace SonarAnalyzer.Core.Test.ShimLayer.Generated;

[TestClass]
public class SyntaxNodeAsToTest
{
    [TestMethod]
    public void As_MatchingKind_ReturnsWrapper()
    {
        var node = SyntaxFactory.FileScopedNamespaceDeclaration(SyntaxFactory.IdentifierName("Sample"));

        SyntaxNodeShimExtensions.get_As(node).FileScopedNamespaceDeclaration.Should().BeOfType<FileScopedNamespaceDeclarationSyntaxWrapper>().Which.WrappedInstance.Should().Be(node); // node.As is ambiguous with FluentAssertions' As<T>(object)
    }

    [TestMethod]
    public void As_OtherKind_ReturnsNull()
    {
        var node = SyntaxFactory.ClassDeclaration("Sample");

        SyntaxNodeShimExtensions.get_As(node).FileScopedNamespaceDeclaration.HasValue.Should().BeFalse();
    }

    [TestMethod]
    public void To_MatchingKind_ReturnsWrapper()
    {
        var node = SyntaxFactory.FileScopedNamespaceDeclaration(SyntaxFactory.IdentifierName("Sample"));

        node.To.FileScopedNamespaceDeclaration.Should().BeOfType<FileScopedNamespaceDeclarationSyntaxWrapper>().Which.WrappedInstance.Should().Be(node);
    }

    [TestMethod]
    public void To_OtherKind_Throws()
    {
        var node = SyntaxFactory.ClassDeclaration("Sample");

        node.Invoking(x => x.To.FileScopedNamespaceDeclaration).Should().Throw<InvalidCastException>();
    }
}
