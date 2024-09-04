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
using ExtensionsCS = SonarAnalyzer.CSharp.Core.Syntax.Extensions.InvocationExpressionSyntaxExtensions;
using ExtensionsVB = SonarAnalyzer.VisualBasic.Core.Syntax.Extensions.InvocationExpressionSyntaxExtensions;
using SyntaxCS = Microsoft.CodeAnalysis.CSharp.Syntax;
using SyntaxVB = Microsoft.CodeAnalysis.VisualBasic.Syntax;
using VB = Microsoft.CodeAnalysis.VisualBasic;

namespace SonarAnalyzer.Test.Extensions;

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
        var node = NodeBetweenMarkers(code, LanguageNames.CSharp) as SyntaxCS.InvocationExpressionSyntax;

        var result = ExtensionsCS.TryGetOperands(node, out var left, out var right);

        result.Should().BeTrue();
        left.Should().NotBeNull();
        left.ToString().Should().Be(expectedLeft);
        right.Should().NotBeNull();
        right.ToString().Should().Be(expectedRight);
    }

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
        var node = NodeBetweenMarkers(code, LanguageNames.VisualBasic) as SyntaxVB.InvocationExpressionSyntax;

        var result = ExtensionsVB.TryGetOperands(node, out var left, out var right);

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
        var node = NodeBetweenMarkers(code, LanguageNames.VisualBasic) as SyntaxVB.InvocationExpressionSyntax;

        var result = ExtensionsVB.TryGetOperands(node, out var left, out var right);

        result.Should().BeFalse();
        left.Should().BeNull();
        right.Should().BeNull();
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
        var node = NodeBetweenMarkers(code, LanguageNames.CSharp) as SyntaxCS.InvocationExpressionSyntax;

        var result = ExtensionsCS.TryGetOperands(node, out var left, out var right);

        result.Should().BeFalse();
        left.Should().BeNull();
        right.Should().BeNull();
    }

    [TestMethod]
    public void HasExactlyNArguments_Null_CS() =>
        ExtensionsCS.HasExactlyNArguments(null, 42).Should().BeFalse();

    [TestMethod]
    public void HasExactlyNArguments_Null_VB() =>
        ExtensionsVB.HasExactlyNArguments(null, 42).Should().BeFalse();

    [TestMethod]
    public void GetMethodCallIdentifier_Null_CS() =>
        ExtensionsCS.GetMethodCallIdentifier(null).Should().BeNull();

    [TestMethod]
    public void GetMethodCallIdentifier_Null_VB() =>
        ExtensionsVB.GetMethodCallIdentifier(null).Should().BeNull();

    [TestMethod]
    public void IsMemberAccessOnKnownType_NotMemberAccessExpression_ReturnsFalse()
    {
        var invocationExpression = VB.SyntaxFactory.ParseSyntaxTree(
@"Sub test()
test()
End Sub")
            .GetRoot()
            .DescendantNodes()
            .OfType<SyntaxVB.InvocationExpressionSyntax>()
            .Single();

        ExtensionsVB.IsMemberAccessOnKnownType(invocationExpression, null, KnownType.System_String, null).Should().BeFalse();
    }

    private static SyntaxNode NodeBetweenMarkers(string code, string language)
    {
        var position = code.IndexOf("$$");
        var lastPosition = code.LastIndexOf("$$");
        var length = lastPosition == position ? 0 : lastPosition - position - "$$".Length;
        code = code.Replace("$$", string.Empty);
        var (tree, _) = IsCSharp() ? TestHelper.CompileCS(code) : TestHelper.CompileVB(code);
        var node = tree.GetRoot().FindNode(new TextSpan(position, length));
        return node;

        bool IsCSharp() => language == LanguageNames.CSharp;
    }
}
