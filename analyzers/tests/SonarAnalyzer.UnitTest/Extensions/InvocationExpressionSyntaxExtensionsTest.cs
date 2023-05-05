/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2023 SonarSource SA
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

extern alias csharp;
extern alias vbnet;

using Microsoft.CodeAnalysis.Text;
using CS = Microsoft.CodeAnalysis.CSharp.Syntax;
using SyntaxNodeExtensionsCS = csharp::SonarAnalyzer.Extensions.InvocationExpressionSyntaxExtensions;
using SyntaxNodeExtensionsVB = vbnet::SonarAnalyzer.Extensions.InvocationExpressionSyntaxExtensions;
using VB = Microsoft.CodeAnalysis.VisualBasic.Syntax;

namespace SonarAnalyzer.UnitTest.Extensions
{
    [TestClass]
    public class InvocationExpressionSyntaxExtensionsTest
    {
        [DataTestMethod]
        [DataRow("System.Array.$$Empty<int>()$$", "System.Array", "Empty<int>")]
        [DataRow("this.$$M()$$", "this", "M")]
        [DataRow("A?.$$M()$$", "A", "M")]
        public void TryGetOperands_CS(string expression, string expectedLeft, string expectedRight)
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
            var node = NodeBetweenMarkers_CS(code) as CS.InvocationExpressionSyntax;

            var result = SyntaxNodeExtensionsCS.TryGetOperands(node, out var left, out var right);

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
        public void TryGetOperands_VB(string expression, string expectedLeft, string expectedRight)
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
            var node = NodeBetweenMarkers_VB(code) as VB.InvocationExpressionSyntax;

            var result = SyntaxNodeExtensionsVB.TryGetOperands(node, out var left, out var right);

            result.Should().BeTrue();
            left.Should().NotBeNull();
            left.ToString().Should().Be(expectedLeft);
            right.Should().NotBeNull();
            right.ToString().Should().Be(expectedRight);
        }

        private static SyntaxNode NodeBetweenMarkers_CS(string code)
        {
            var position = code.IndexOf("$$");
            var length = code.LastIndexOf("$$") - position - 2;
            code = code.Replace("$$", string.Empty);
            var (tree, _) = TestHelper.CompileCS(code);
            var node = tree.GetRoot().FindNode(new TextSpan(position, length));
            return node;
        }

        private static SyntaxNode NodeBetweenMarkers_VB(string code)
        {
            var position = code.IndexOf("$$");
            var length = code.LastIndexOf("$$") - position - 2;
            code = code.Replace("$$", string.Empty);
            var (tree, _) = TestHelper.CompileVB(code);
            var node = tree.GetRoot().FindNode(new TextSpan(position, length));
            return node;
        }
    }
}
