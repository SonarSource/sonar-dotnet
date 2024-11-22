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

namespace SonarAnalyzer.CSharp.Core.Test.Syntax.Extensions;

[TestClass]
public class InterpolatedStringExpressionSyntaxExtensionsTest
{
    [DataTestMethod]
    [DataRow(@"var methodCall = $""{Foo()}"";")]
    [DataRow(@"var nestedMethodCall = $""{$""{$""{Foo()}""}""}"";")]
    [DataRow(@"const int constant = 1; var mixConstantNonConstant = $""{notConstant}{constant}"";")]
    [DataRow(@"const int constant = 1; var mixConstantAndLiteral = $""TextValue {constant}"";")]
    [DataRow(@"const int constant = 1; var mix = $""{constant}{$""{Foo()}""}{""{notConstant}""}"";")]
    public void TryGetGetInterpolatedTextValue_UnsupportedSyntaxKinds_ReturnsFalse_CS(string snippet)
    {
        var (expression, semanticModel) = CompileCS(snippet);
        InterpolatedStringExpressionSyntaxExtensions.TryGetInterpolatedTextValue(expression, semanticModel, out var interpolatedValue).Should().Be(false);
        interpolatedValue.Should().BeNull();
    }

    [DataTestMethod]
    [DataRow(@"var textOnly = $""TextOnly"";", "TextOnly")]
    [DataRow(@"const string constantString = ""Foo""; const string constantInterpolation = $""{constantString} with text."";", "Foo with text.")]
    [DataRow(@"const string constantString = ""Foo""; const string constantInterpolation = $""{$""Nested {constantString}""} with text."";", "Nested Foo with text.")]
    [DataRow(@"notConstantString = ""SomeValue""; string interpolatedString = $""{notConstantString}"";", "SomeValue")]
    public void TryGetGetInterpolatedTextValue_SupportedSyntaxKinds_ReturnsTrue_CS(string snippet, string expectedTextValue)
    {
        var (expression, semanticModel) = CompileCS(snippet);
        InterpolatedStringExpressionSyntaxExtensions.TryGetInterpolatedTextValue(expression, semanticModel, out var interpolatedValue).Should().Be(true);
        interpolatedValue.Should().Be(expectedTextValue);
    }

    private static (InterpolatedStringExpressionSyntax InterpolatedStringExpression, SemanticModel SemanticModel) CompileCS(string snippet)
    {
        var code = $$"""
            public class C
            {
                public void M(int notConstant, string notConstantString)
                {
                    {{snippet}}
                }

                string Foo() => "x";
            }
            """;
        var tree = CSharpSyntaxTree.ParseText(code);
        var compilation = CSharpCompilation.Create("TempAssembly.dll").AddSyntaxTrees(tree).AddReferences(MetadataReferenceFacade.ProjectDefaultReferences);
        var model = compilation.GetSemanticModel(tree);
        return (tree.First<InterpolatedStringExpressionSyntax>(), model);
    }
}
