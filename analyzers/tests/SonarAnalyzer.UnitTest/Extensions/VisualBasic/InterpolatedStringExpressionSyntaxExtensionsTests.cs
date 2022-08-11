/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2022 SonarSource SA
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

using Microsoft.CodeAnalysis.VisualBasic;
using Microsoft.CodeAnalysis.VisualBasic.Syntax;
using SonarAnalyzer.Extensions;

namespace SonarAnalyzer.UnitTest.Extensions.VisualBasic
{
    [TestClass]
    public class InterpolatedStringExpressionSyntaxExtensionsTests
    {
        private const string CodeSnipet = @"
Public Class C
        Public Sub M(ByVal notConstant As Integer, ByVal notConstantString As String)
            {0}
        End Sub

        Private Function Foo() As String
            Return ""x""
        End Function
End Class";

        [DataTestMethod]
        [DataRow(@"Dim methodCall = $""{Foo()}""")]
        [DataRow(@"Dim nestedMethodCall = $""{$""{$""{Foo()}""}""}""")]
        [DataRow(@"Const constant As Integer = 1
                   Dim mixConstantNonConstant = $""{notConstant}{constant}""")]
        [DataRow(@"Const constant As Integer = 1
                   Dim mixConstantAndLiteral = $""TextValue {constant}""")]
        [DataRow(@"Const constant As Integer = 1
                   Dim mix = $""{constant}{$""{Foo()}""}{""{notConstant}""}""")]
        public void TryGetGetInterpolatedTextValue_UnsupportedSyntaxKinds_ReturnsFalse(string code)
        {
            var codeSnipet = string.Format(CodeSnipet, code);
            var (expression, semanticModel) = Compile(codeSnipet);
            expression.TryGetGetInterpolatedTextValue(semanticModel, out var interpolatedValue).Should().Be(false);
            interpolatedValue.Should().BeNull();
        }

        [DataTestMethod]
        [DataRow(@"
                   Dim textOnly = $""TextOnly""
                 ",
                 "TextOnly")]
        [DataRow(@"
                    Const constantString As String = ""Foo""
                    Const constantInterpolation As String = $""{constantString} with text.""
                 ",
                 "Foo with text.")]
        [DataRow(@"
                    Const constantString As String = ""Foo""
                    Const constantInterpolation As String = $""{$""Nested {constantString}""} with text.""",
                 "Nested Foo with text.")]
        [DataRow(@"
                    notConstantString = ""SomeValue""
                    Dim interpolatedString As String = $""{notConstantString}""
                 ",
                 "SomeValue")]
        public void TryGetGetInterpolatedTextValue_SupportedSyntaxKinds_ReturnsTrue(string code, string expectedTextValue)
        {
            var codeSnipet = string.Format(CodeSnipet, code);
            var (expression, semanticModel) = Compile(codeSnipet);
            expression.TryGetGetInterpolatedTextValue(semanticModel, out var interpolatedValue).Should().Be(true);
            interpolatedValue.Should().Be(expectedTextValue);
        }

        private static (InterpolatedStringExpressionSyntax InterpolatedStringExpression, SemanticModel SemanticModel) Compile(string code)
        {
            var tree = VisualBasicSyntaxTree.ParseText(code);
            var compilation = VisualBasicCompilation.Create("TempAssembly.dll")
                                               .AddSyntaxTrees(tree)
                                               .AddReferences(MetadataReferenceFacade.ProjectDefaultReferences);

            var semanticModel = compilation.GetSemanticModel(tree);

            return (tree.GetRoot().DescendantNodes().OfType<InterpolatedStringExpressionSyntax>().First(), semanticModel);
        }
    }
}
