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

namespace SonarAnalyzer.VisualBasic.Core.Test.Syntax.Extensions;

[TestClass]
public class InterpolatedStringExpressionSyntaxExtensionsTest
{
    [DataTestMethod]
    [DataRow(@"Dim methodCall = $""{Foo()}""")]
    [DataRow(@"Dim nestedMethodCall = $""{$""{$""{Foo()}""}""}""")]
    [DataRow(@"Const constant As Integer = 1 : Dim mixConstantNonConstant = $""{notConstant}{constant}""")]
    [DataRow(@"Const constant As Integer = 1 : Dim mixConstantAndLiteral = $""TextValue {constant}""")]
    [DataRow(@"Const constant As Integer = 1 : Dim mix = $""{constant}{$""{Foo()}""}{""{notConstant}""}""")]
    public void TryGetGetInterpolatedTextValue_UnsupportedSyntaxKinds_ReturnsFalse_VB(string methodBody)
    {
        var (expression, model) = CompileVB(methodBody);
        InterpolatedStringExpressionSyntaxExtensions.InterpolatedTextValue(expression, model).Should().BeNull();
    }

    [DataTestMethod]
    [DataRow(@"Dim textOnly = $""TextOnly""", "TextOnly")]
    [DataRow(@"Const constantString As String = ""Foo"" : Dim constantInterpolation As String = $""{constantString} with text.""", "Foo with text.")]
    [DataRow(@"Const constantString As String = ""Foo"" : Dim constantInterpolation As String = $""{$""Nested {constantString}""} with text.""", "Nested Foo with text.")]
    [DataRow(@"notConstantString = ""SomeValue"" : Dim interpolatedString As String = $""{notConstantString}""", "SomeValue")]
    public void TryGetGetInterpolatedTextValue_SupportedSyntaxKinds_ReturnsTrue_VB(string methodBody, string expectedTextValue)
    {
        var (expression, model) = CompileVB(methodBody);
        InterpolatedStringExpressionSyntaxExtensions.InterpolatedTextValue(expression, model).Should().Be(expectedTextValue);
    }

    private static (InterpolatedStringExpressionSyntax InterpolatedStringExpression, SemanticModel SemanticModel) CompileVB(string methodBody)
    {
        var code = $"""
            Public Class C
                    Public Sub M(ByVal notConstant As Integer, ByVal notConstantString As String)
                        {methodBody}
                    End Sub

                    Private Function Foo() As String
                        Return "x"
                    End Function
            End Class
            """;
        var (tree, model) = TestHelper.CompileVB(code);
        return (tree.First<InterpolatedStringExpressionSyntax>(), model);
    }
}
