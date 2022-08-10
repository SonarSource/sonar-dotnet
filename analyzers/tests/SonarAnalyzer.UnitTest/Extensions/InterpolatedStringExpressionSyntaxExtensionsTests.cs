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

using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SonarAnalyzer.Extensions;

namespace SonarAnalyzer.UnitTest.Extensions
{
    [TestClass]
    public class InterpolatedStringExpressionSyntaxExtensionsTests
    {
        private const string CodeSnipet = @"
public class C
{{
    public void M(int notConstant, string notConstantString)
    {{
        {0}
    }}

    string Foo() => ""x"";
}}";

        [DataTestMethod]
        [DataRow(@"var methodCall = $""{Foo()}"";")]
        [DataRow(@"var nestedMethodCall = $""{$""{$""{Foo()}""}""}"";")]
        [DataRow(@"const int constant = 1;
                 var mixConstantNonConstant = $""{notConstant}{constant}"";")]
        [DataRow(@"const int constant = 1;
                 var mixConstantAndLiteral = $""TextValue {constant}"";")]
        [DataRow(@"const int constant = 1;
                 var mix = $""{constant}{$""{Foo()}""}{""{notConstant}""}"";")]
        public void TryGetGetInterpolatedTextValue_Returns_False(string code)
        {
            var codeSnipet = string.Format(CodeSnipet, code);
            var (expression, semanticModel) = Compile(codeSnipet);
            expression.TryGetGetInterpolatedTextValue(semanticModel, out var interpolatedValue).Should().Be(false);
            interpolatedValue.Should().BeNull();
        }

        [DataTestMethod]
        [DataRow(@"
                   var textOnly = $""TextOnly"";
                 ",
                 "TextOnly")]
        [DataRow(@"
                    const string constantString = ""Foo"";
                    const string constantInterpolation = $""{constantString} with text."";
                 ",
                 "Foo with text.")]
        [DataRow(@"
                    const string constantString = ""Foo"";
                    const string constantInterpolation = $""{$""Nested {constantString}""} with text."";
                 ",
                 "Nested Foo with text.")]
        [DataRow(@"
                    notConstantString = ""SomeValue"";
                 string interpolatedString = $""{notConstantString}"";
                 ",
                 "SomeValue")]
        public void TryGetGetInterpolatedTextValue_Returns_False(string code, string expectedTextValue)
        {
            var codeSnipet = string.Format(CodeSnipet, code);
            var (expression, semanticModel) = Compile(codeSnipet);
            expression.TryGetGetInterpolatedTextValue(semanticModel, out var interpolatedValue).Should().Be(true);
            interpolatedValue.Should().Be(expectedTextValue);
        }

        private static (InterpolatedStringExpressionSyntax InterpolatedStringExpression, SemanticModel SemanticModel) Compile(string code)
        {
            var tree = CSharpSyntaxTree.ParseText(code);
            var compilation = CSharpCompilation.Create("TempAssembly.dll")
                                               .AddSyntaxTrees(tree)
                                               .AddReferences(MetadataReferenceFacade.ProjectDefaultReferences);

            var semanticModel = compilation.GetSemanticModel(tree);

            return (tree.GetRoot().DescendantNodes().OfType<InterpolatedStringExpressionSyntax>().First(), semanticModel);
        }
    }
}
