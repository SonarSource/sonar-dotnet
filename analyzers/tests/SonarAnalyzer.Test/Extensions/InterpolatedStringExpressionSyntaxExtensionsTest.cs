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

extern alias csharp;
extern alias vbnet;

using CS = Microsoft.CodeAnalysis.CSharp;
using ExtensionsCS = csharp::SonarAnalyzer.Extensions.InterpolatedStringExpressionSyntaxExtensions;
using ExtensionsVB = vbnet::SonarAnalyzer.Extensions.InterpolatedStringExpressionSyntaxExtensions;
using SyntaxCS = Microsoft.CodeAnalysis.CSharp.Syntax;
using SyntaxVB = Microsoft.CodeAnalysis.VisualBasic.Syntax;

namespace SonarAnalyzer.Test.Extensions;

[TestClass]
public class InterpolatedStringExpressionSyntaxExtensionsTest
{
    private const string CodeSnipetCS = @"
public class C
{{
    public void M(int notConstant, string notConstantString)
    {{
        {0}
    }}

    string Foo() => ""x"";
}}";

    private const string CodeSnipetVB = @"
Public Class C
        Public Sub M(ByVal notConstant As Integer, ByVal notConstantString As String)
            {0}
        End Sub

        Private Function Foo() As String
            Return ""x""
        End Function
End Class";

    [DataTestMethod]
    [DataRow(@"var methodCall = $""{Foo()}"";")]
    [DataRow(@"var nestedMethodCall = $""{$""{$""{Foo()}""}""}"";")]
    [DataRow(@"const int constant = 1; var mixConstantNonConstant = $""{notConstant}{constant}"";")]
    [DataRow(@"const int constant = 1; var mixConstantAndLiteral = $""TextValue {constant}"";")]
    [DataRow(@"const int constant = 1; var mix = $""{constant}{$""{Foo()}""}{""{notConstant}""}"";")]
    public void TryGetGetInterpolatedTextValue_UnsupportedSyntaxKinds_ReturnsFalse_CS(string code)
    {
        var codeSnipet = string.Format(CodeSnipetCS, code);
        var (expression, semanticModel) = CompileCS(codeSnipet);
        ExtensionsCS.TryGetInterpolatedTextValue(expression, semanticModel, out var interpolatedValue).Should().Be(false);
        interpolatedValue.Should().BeNull();
    }

    [DataTestMethod]
    [DataRow(@"Dim methodCall = $""{Foo()}""")]
    [DataRow(@"Dim nestedMethodCall = $""{$""{$""{Foo()}""}""}""")]
    [DataRow(@"Const constant As Integer = 1 : Dim mixConstantNonConstant = $""{notConstant}{constant}""")]
    [DataRow(@"Const constant As Integer = 1 : Dim mixConstantAndLiteral = $""TextValue {constant}""")]
    [DataRow(@"Const constant As Integer = 1 : Dim mix = $""{constant}{$""{Foo()}""}{""{notConstant}""}""")]
    public void TryGetGetInterpolatedTextValue_UnsupportedSyntaxKinds_ReturnsFalse_VB(string methodBody)
    {
        var (expression, semanticModel) = CompileVB(methodBody);
        ExtensionsVB.TryGetInterpolatedTextValue(expression, semanticModel, out var interpolatedValue).Should().Be(false);
        interpolatedValue.Should().BeNull();
    }

    [DataTestMethod]
    [DataRow(@"var textOnly = $""TextOnly"";", "TextOnly")]
    [DataRow(@"const string constantString = ""Foo""; const string constantInterpolation = $""{constantString} with text."";", "Foo with text.")]
    [DataRow(@"const string constantString = ""Foo""; const string constantInterpolation = $""{$""Nested {constantString}""} with text."";", "Nested Foo with text.")]
    [DataRow(@"notConstantString = ""SomeValue""; string interpolatedString = $""{notConstantString}"";", "SomeValue")]
    public void TryGetGetInterpolatedTextValue_SupportedSyntaxKinds_ReturnsTrue_CS(string code, string expectedTextValue)
    {
        var codeSnipet = string.Format(CodeSnipetCS, code);
        var (expression, semanticModel) = CompileCS(codeSnipet);
        ExtensionsCS.TryGetInterpolatedTextValue(expression, semanticModel, out var interpolatedValue).Should().Be(true);
        interpolatedValue.Should().Be(expectedTextValue);
    }

    [DataTestMethod]
    [DataRow(@"Dim textOnly = $""TextOnly""", "TextOnly")]
    [DataRow(@"Const constantString As String = ""Foo"" : Dim constantInterpolation As String = $""{constantString} with text.""", "Foo with text.")]
    [DataRow(@"Const constantString As String = ""Foo"" : Dim constantInterpolation As String = $""{$""Nested {constantString}""} with text.""", "Nested Foo with text.")]
    [DataRow(@"notConstantString = ""SomeValue"" : Dim interpolatedString As String = $""{notConstantString}""", "SomeValue")]
    public void TryGetGetInterpolatedTextValue_SupportedSyntaxKinds_ReturnsTrue_VB(string methodBody, string expectedTextValue)
    {
        var (expression, semanticModel) = CompileVB(methodBody);
        ExtensionsVB.TryGetInterpolatedTextValue(expression, semanticModel, out var interpolatedValue).Should().Be(true);
        interpolatedValue.Should().Be(expectedTextValue);
    }

    private static (SyntaxCS.InterpolatedStringExpressionSyntax InterpolatedStringExpression, SemanticModel SemanticModel) CompileCS(string code)
    {
        var tree = CS.CSharpSyntaxTree.ParseText(code);
        var compilation = CS.CSharpCompilation.Create("TempAssembly.dll")
                                           .AddSyntaxTrees(tree)
                                           .AddReferences(MetadataReferenceFacade.ProjectDefaultReferences);

        var semanticModel = compilation.GetSemanticModel(tree);

        return (tree.First<SyntaxCS.InterpolatedStringExpressionSyntax>(), semanticModel);
    }

    private static (SyntaxVB.InterpolatedStringExpressionSyntax InterpolatedStringExpression, SemanticModel SemanticModel) CompileVB(string methodBody)
    {
        var (tree, model) = TestHelper.CompileVB(string.Format(CodeSnipetVB, methodBody));
        return (tree.First<SyntaxVB.InterpolatedStringExpressionSyntax>(), model);
    }
}
