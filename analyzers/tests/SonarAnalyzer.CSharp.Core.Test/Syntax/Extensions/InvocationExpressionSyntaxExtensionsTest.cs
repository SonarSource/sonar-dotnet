/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2014-2025 SonarSource Sàrl
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

using Microsoft.CodeAnalysis.Text;

namespace SonarAnalyzer.CSharp.Core.Test.Syntax.Extensions;

[TestClass]
public class InvocationExpressionSyntaxExtensionsTest
{
    [TestMethod]
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
        var (left, right) = InvocationExpressionSyntaxExtensions.Operands(node);

        left.Should().NotBeNull();
        left.ToString().Should().Be(expectedLeft);
        right.Should().NotBeNull();
        right.ToString().Should().Be(expectedRight);
    }

    [TestMethod]
    [DataRow("$$M()$$")]
    [DataRow("new System.Func<int>(() => 1)$$()$$")]
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

        var (left, right) = InvocationExpressionSyntaxExtensions.Operands(node);

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
        return TestCompiler.CompileCS(code).Tree.GetRoot().FindNode(new TextSpan(position, length));
    }
}
