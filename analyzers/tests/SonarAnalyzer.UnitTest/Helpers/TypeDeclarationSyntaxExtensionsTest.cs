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

using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SonarAnalyzer.Extensions;

namespace SonarAnalyzer.UnitTest.Helpers
{
    [TestClass]
    public class TypeDeclarationSyntaxExtensionsTest
    {
        [TestMethod]
        public void GetMethodDeclarations_EmptyClass_ReturnsEmpty()
        {
            var typeDeclaration = SyntaxFactory.ClassDeclaration(SyntaxFactory.Identifier("TestClass"));

            typeDeclaration.GetMethodDeclarations().Should().BeEmpty();
        }

        [TestMethod]
        public void GetMethodDeclarations_SingleMethod_ReturnsMethod()
        {
            const string code = @"
namespace Test
{
    class TestClass
    {
        private void WriteLine() {}
    }
}
";
            var snippet = new SnippetCompiler(code);
            var typeDeclaration = snippet.SyntaxTree.Single<TypeDeclarationSyntax>();
            typeDeclaration.GetMethodDeclarations().Single().Identifier.Text.Should().Be("WriteLine");
        }

        [TestMethod]
        public void GetMethodDeclarations_MultipleMethodsWithLocalFunctions_ReturnsMethodsAndFunctions()
        {
            const string code = @"
namespace Test
{
    class TestClass
    {
        private void Method1()
        {
            Function1();
            Function2();
            void Function1() {}
            void Function2() {}
        }

        private void Method2()
        {
            Function3();
            void Function3() {}
        }
    }
}
";
            var snippet = new SnippetCompiler(code);
            var typeDeclaration = snippet.SyntaxTree.Single<TypeDeclarationSyntax>();
            typeDeclaration
                .GetMethodDeclarations()
                .Select(methodDeclaration => methodDeclaration.Identifier.Text)
                .Should()
                .BeEquivalentTo(new List<string>
                {
                    "Method1",
                    "Method2",
                    "Function1",
                    "Function2",
                    "Function3"
                });
        }

        [DataTestMethod]
        [DataRow("class")]
        [DataRow("struct")]
        [DataRow("readonly struct")]
        [DataRow("record")]
        [DataRow("record class")]
        [DataRow("record struct")]
        [DataRow("readonly record struct")]
        public void PrimaryConstructorParameterList_ReturnsList(string type)
        {
            var (tree, model) = TestHelper.CompileCS($$"""
                {{type}} Test(int i)
                {
                }
                """);
            var typeDeclaration = tree.GetCompilationUnitRoot().DescendantNodesAndSelf().OfType<TypeDeclarationSyntax>().Single();
            var parameterList = typeDeclaration.PrimaryConstructorParameterList();
            parameterList.Should().NotBeNull();
            parameterList.Parameters.Should().ContainSingle();
            parameterList.Parameters.Should().ContainSingle(p => p.Type is PredefinedTypeSyntax && p.Identifier.ValueText == "i");
        }

        [DataTestMethod]
        [DataRow(LanguageVersion.CSharp1)]
        [DataRow(LanguageVersion.CSharp2)]
        [DataRow(LanguageVersion.CSharp3)]
        [DataRow(LanguageVersion.CSharp4)]
        [DataRow(LanguageVersion.CSharp5)]
        [DataRow(LanguageVersion.CSharp6)]
        [DataRow(LanguageVersion.CSharp7)]
        [DataRow(LanguageVersion.CSharp7_1)]
        [DataRow(LanguageVersion.CSharp7_2)]
        [DataRow(LanguageVersion.CSharp7_3)]
        [DataRow(LanguageVersion.CSharp8)]
        [DataRow(LanguageVersion.CSharp9)]
        [DataRow(LanguageVersion.CSharp10)]
        [DataRow(LanguageVersion.CSharp11)]
        [DataRow(LanguageVersion.CSharp12)]
        public void PrimaryConstructor_NoPrimaryConstructor(LanguageVersion languageVersion)
        {
            var tree = CSharpSyntaxTree.ParseText("""
                public class Test
                {
                }
                """, new CSharpParseOptions(languageVersion));
            var compilation = CSharpCompilation.Create(assemblyName: null, syntaxTrees: new[] { tree });
            var typeDeclaration = tree.GetCompilationUnitRoot().DescendantNodesAndSelf().OfType<TypeDeclarationSyntax>().Single();
            typeDeclaration.PrimaryConstructor(compilation.GetSemanticModel(tree)).Should().BeNull();
        }

        [DataTestMethod]
        [DataRow(LanguageVersion.CSharp9)]
        [DataRow(LanguageVersion.CSharp10)]
        [DataRow(LanguageVersion.CSharp11)]
        [DataRow(LanguageVersion.CSharp12)]
        public void PrimaryConstructor_PrimaryConstructorRecord(LanguageVersion languageVersion)
        {
            var options = new CSharpParseOptions(languageVersion);
            var tree = CSharpSyntaxTree.ParseText("""
                public record Test(int i)
                {
                }
                """, options);
            var compilation = CSharpCompilation.Create(assemblyName: null, syntaxTrees: new[] { tree });
            var typeDeclaration = tree.GetCompilationUnitRoot().DescendantNodesAndSelf().OfType<TypeDeclarationSyntax>().Single();
            var methodSymbol = typeDeclaration.PrimaryConstructor(compilation.GetSemanticModel(tree));
            methodSymbol.Should().NotBeNull();
            methodSymbol.MethodKind.Should().Be(MethodKind.Constructor);
            methodSymbol.Parameters.Should().ContainSingle();
            methodSymbol.Parameters.Should().ContainSingle(p => p.Name == "i" && p.Type.SpecialType == SpecialType.System_Int32);
        }

        [DataTestMethod]
        [DataRow("class")]
        [DataRow("struct")]
        [DataRow("readonly struct")]
#if NET
        [DataRow("record")]
        [DataRow("record class")]
#endif
        [DataRow("record struct")]
        public void PrimaryConstructor_PrimaryConstructorOnClass(string type)
        {
            var (tree, model) = TestHelper.CompileCS($$"""
                {{type}} Test(int i)
                {
                }
                """);
            var typeDeclaration = tree.GetCompilationUnitRoot().DescendantNodesAndSelf().OfType<TypeDeclarationSyntax>().Single();
            var methodSymbol = typeDeclaration.PrimaryConstructor(model);
            methodSymbol.Should().NotBeNull();
            methodSymbol.MethodKind.Should().Be(MethodKind.Constructor);
            methodSymbol.Parameters.Should().ContainSingle();
            methodSymbol.Parameters.Should().ContainSingle(p => p.Name == "i" && p.Type.SpecialType == SpecialType.System_Int32);
        }

        [TestMethod]
        public void PrimaryConstructor_EmptyPrimaryConstructor()
        {
            var (tree, model) = TestHelper.CompileCS($$"""
                public class Test()
                {
                    public Test(int i) : this() { }
                }
                """);
            var typeDeclaration = tree.GetCompilationUnitRoot().DescendantNodesAndSelf().OfType<TypeDeclarationSyntax>().Single();
            var methodSymbol = typeDeclaration.PrimaryConstructor(model);
            methodSymbol.Should().NotBeNull();
            methodSymbol.MethodKind.Should().Be(MethodKind.Constructor);
            methodSymbol.Parameters.Should().BeEmpty();
        }

        [TestMethod]
        public void PrimaryConstructor_EmptyPrimaryConstructorAndStaticConstructor()
        {
            var (tree, model) = TestHelper.CompileCS($$"""
                public class Test()
                {
                    static Test() { }
                }
                """);
            var typeDeclaration = tree.GetCompilationUnitRoot().DescendantNodesAndSelf().OfType<TypeDeclarationSyntax>().Single();
            var methodSymbol = typeDeclaration.PrimaryConstructor(model);
            methodSymbol.Should().NotBeNull();
            methodSymbol.MethodKind.Should().Be(MethodKind.Constructor);
            methodSymbol.Parameters.Should().BeEmpty();
            methodSymbol.IsStatic.Should().BeFalse();
        }
    }
}
