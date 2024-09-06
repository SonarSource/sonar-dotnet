/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2024 SonarSource SA
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

using Microsoft.CodeAnalysis.Text;

namespace SonarAnalyzer.CSharp.Core.Test.Syntax.Extensions;

[TestClass]
public class InvocationExpressionSyntaxExtensionsTest
{
    [DataTestMethod]
    [DataRow("System.Array.$$Empty<int>()$$", "System.Array", "Empty<int>")]
    [DataRow("this.$$M()$$", "this", "M")]
    [DataRow("A?.$$M()$$", "A", "M")]
    public void TryGetOperands_InvocationNode_ShouldReturnsTrue_CS(string expression, string expectedLeft, string expectedRight)
    {
        var code = $$"""
            public class X
            {
                public X A { get; }
                public int M()
                {
                    var _ = {{expression}};
                    return 42;
                }
            }
            """;
        var node = NodeBetweenMarkers(code, LanguageNames.CSharp) as InvocationExpressionSyntax;
        var result = InvocationExpressionSyntaxExtensions.TryGetOperands(node, out var left, out var right);

        result.Should().BeTrue();
        left.Should().NotBeNull();
        left.ToString().Should().Be(expectedLeft);
        right.Should().NotBeNull();
        right.ToString().Should().Be(expectedRight);
    }

    [DataTestMethod]
    [DataRow("$$M()$$")]
    public void TryGetOperands_InvocationNodeDoesNotContainMemberAccess_ShouldReturnsFalse_CS(string expression)
    {
        var code = $$"""
            public class X
            {
                public X A { get; }
                public int M()
                {
                    var _ = {{expression}};
                    return 42;
                }
            }
            """;
        var node = NodeBetweenMarkers(code, LanguageNames.CSharp) as InvocationExpressionSyntax;

        var result = InvocationExpressionSyntaxExtensions.TryGetOperands(node, out var left, out var right);

        result.Should().BeFalse();
        left.Should().BeNull();
        right.Should().BeNull();
    }

    [TestMethod]
    public void HasExactlyNArguments_Null_CS() =>
        InvocationExpressionSyntaxExtensions.HasExactlyNArguments(null, 42).Should().BeFalse();

    [TestMethod]
    public void GetMethodCallIdentifier_Null_CS() =>
        InvocationExpressionSyntaxExtensions.GetMethodCallIdentifier(null).Should().BeNull();

    private static SyntaxNode NodeBetweenMarkers(string code, string language)
    {
        var position = code.IndexOf("$$");
        var lastPosition = code.LastIndexOf("$$");
        var length = lastPosition == position ? 0 : lastPosition - position - "$$".Length;
        code = code.Replace("$$", string.Empty);
        return TestHelper.CompileCS(code).Tree.GetRoot().FindNode(new TextSpan(position, length));
    }
}
