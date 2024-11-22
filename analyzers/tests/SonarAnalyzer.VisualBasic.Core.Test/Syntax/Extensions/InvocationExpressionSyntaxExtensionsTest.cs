/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2014-2024 SonarSource SA
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
using SonarAnalyzer.Helpers;

namespace SonarAnalyzer.VisualBasic.Core.Test.Syntax.Extensions;

[TestClass]
public class InvocationExpressionSyntaxExtensionsTest
{
    [DataTestMethod]
    [DataRow("System.Array.$$Empty(Of Integer)()$$", "System.Array", "Empty(Of Integer)")]
    [DataRow("Me.$$M()$$", "Me", "M")]
    [DataRow("A?.$$M()$$", "A", "M")]
    public void TryGetOperands_InvocationNode_ShouldReturnsTrue_VB(string expression, string expectedLeft, string expectedRight)
    {
        var code = $$"""
            Public Class X
                Public Property A As X
                Public Function M() As Integer
                    Dim unused = {{expression}}
                    Return 42
                End Function
            End Class
            """;
        var node = NodeBetweenMarkers(code, LanguageNames.VisualBasic) as InvocationExpressionSyntax;

        var result = InvocationExpressionSyntaxExtensions.TryGetOperands(node, out var left, out var right);

        result.Should().BeTrue();
        left.Should().NotBeNull();
        left.ToString().Should().Be(expectedLeft);
        right.Should().NotBeNull();
        right.ToString().Should().Be(expectedRight);
    }

    [DataTestMethod]
    [DataRow("$$M()$$")]
    public void TryGetOperands_InvocationNodeDoesNotContainMemberAccess_ShouldReturnsFalse_VB(string expression)
    {
        var code = $$"""
            Public Class X
                Public Property A As X
                Public Function M() As Integer
                    Dim unused = {{expression}}
                    Return 42
                End Function
            End Class
            """;
        var node = NodeBetweenMarkers(code, LanguageNames.VisualBasic) as InvocationExpressionSyntax;

        var result = InvocationExpressionSyntaxExtensions.TryGetOperands(node, out var left, out var right);

        result.Should().BeFalse();
        left.Should().BeNull();
        right.Should().BeNull();
    }

    [TestMethod]
    public void HasExactlyNArguments_Null_VB() =>
        InvocationExpressionSyntaxExtensions.HasExactlyNArguments(null, 42).Should().BeFalse();

    [TestMethod]
    public void GetMethodCallIdentifier_Null_VB() =>
        InvocationExpressionSyntaxExtensions.GetMethodCallIdentifier(null).Should().BeNull();

    [TestMethod]
    public void IsMemberAccessOnKnownType_NotMemberAccessExpression_ReturnsFalse()
    {
        var invocationExpression = SyntaxFactory.ParseSyntaxTree("""
            Sub test()
                test()
            End Sub
            """)
            .GetRoot()
            .DescendantNodes()
            .OfType<InvocationExpressionSyntax>()
            .Single();

        InvocationExpressionSyntaxExtensions.IsMemberAccessOnKnownType(invocationExpression, null, KnownType.System_String, null).Should().BeFalse();
    }

    private static SyntaxNode NodeBetweenMarkers(string code, string language)
    {
        var position = code.IndexOf("$$");
        var lastPosition = code.LastIndexOf("$$");
        var length = lastPosition == position ? 0 : lastPosition - position - "$$".Length;
        code = code.Replace("$$", string.Empty);
        return TestHelper.CompileVB(code).Tree.GetRoot().FindNode(new TextSpan(position, length));
    }
}
