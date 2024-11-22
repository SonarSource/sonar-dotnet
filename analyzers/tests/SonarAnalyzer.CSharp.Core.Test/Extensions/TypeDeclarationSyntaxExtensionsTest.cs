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

namespace SonarAnalyzer.CSharp.Core.Test.Extensions;

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
        const string code = """
            namespace Test
            {
                class TestClass
                {
                    private void WriteLine() {}
                }
            }
            """;
        var snippet = new SnippetCompiler(code);
        var typeDeclaration = snippet.SyntaxTree.Single<TypeDeclarationSyntax>();
        typeDeclaration.GetMethodDeclarations().Single().Identifier.Text.Should().Be("WriteLine");
    }

    [TestMethod]
    public void GetMethodDeclarations_MultipleMethodsWithLocalFunctions_ReturnsMethodsAndFunctions()
    {
        const string code = """
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
            """;
        var snippet = new SnippetCompiler(code);
        var typeDeclaration = snippet.SyntaxTree.Single<TypeDeclarationSyntax>();
        typeDeclaration
            .GetMethodDeclarations()
            .Select(x => x.Identifier.Text)
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
    [DataRow("record struct")]
#if NET
    [DataRow("record")]
    [DataRow("record class")]
    [DataRow("readonly record struct")]
#endif
    public void ParameterList_ReturnsList(string type)
    {
        var tree = TestHelper.CompileCS($$"""{{type}} Test(int i) { }""").Tree;
        var typeDeclaration = tree.GetCompilationUnitRoot().DescendantNodesAndSelf().OfType<TypeDeclarationSyntax>().Single();
        var parameterList = typeDeclaration.ParameterList();
        parameterList.Should().NotBeNull();
        var entry = parameterList.Parameters.Should().ContainSingle().Which;
        entry.Type.Should().BeOfType<PredefinedTypeSyntax>();
        entry.Identifier.ValueText.Should().Be("i");
    }

    [TestMethod]
    public void ParameterList_Interface()
    {
        var tree = TestHelper.CompileCS("interface Test { }").Tree;
        var typeDeclaration = tree.GetCompilationUnitRoot().DescendantNodesAndSelf().OfType<TypeDeclarationSyntax>().Single();
        var parameterList = typeDeclaration.ParameterList();
        parameterList.Should().BeNull();
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
        var tree = CSharpSyntaxTree.ParseText("public class Test { }", new CSharpParseOptions(languageVersion));
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
        var tree = CSharpSyntaxTree.ParseText("public record Test(int i) { }", new CSharpParseOptions(languageVersion));
        var compilation = CSharpCompilation.Create(assemblyName: null, syntaxTrees: new[] { tree });
        var typeDeclaration = tree.GetCompilationUnitRoot().DescendantNodesAndSelf().OfType<TypeDeclarationSyntax>().Single();
        var methodSymbol = typeDeclaration.PrimaryConstructor(compilation.GetSemanticModel(tree));
        methodSymbol.Should().NotBeNull();
        methodSymbol.MethodKind.Should().Be(MethodKind.Constructor);
        var entry = methodSymbol.Parameters.Should().ContainSingle().Which;
        entry.Name.Should().Be("i");
        entry.Type.SpecialType.Should().Be(SpecialType.System_Int32);
    }

    [DataTestMethod]
    [DataRow("class")]
    [DataRow("struct")]
    [DataRow("readonly struct")]
    [DataRow("record struct")]
#if NET
    [DataRow("record")]
    [DataRow("record class")]
#endif
    public void PrimaryConstructor_PrimaryConstructorOnClass(string type)
    {
        var (tree, model) = TestHelper.CompileCS($$"""{{type}} Test(int i) { }""");
        var typeDeclaration = tree.GetCompilationUnitRoot().DescendantNodesAndSelf().OfType<TypeDeclarationSyntax>().Single();
        var methodSymbol = typeDeclaration.PrimaryConstructor(model);
        methodSymbol.Should().NotBeNull();
        methodSymbol.MethodKind.Should().Be(MethodKind.Constructor);
        var entry = methodSymbol.Parameters.Should().ContainSingle().Which;
        entry.Name.Should().Be("i");
        entry.Type.SpecialType.Should().Be(SpecialType.System_Int32);
    }

    [TestMethod]
    public void PrimaryConstructor_EmptyPrimaryConstructor()
    {
        var (tree, model) = TestHelper.CompileCS("public class Test() { }");
        var typeDeclaration = tree.GetCompilationUnitRoot().DescendantNodesAndSelf().OfType<TypeDeclarationSyntax>().Single();
        var methodSymbol = typeDeclaration.PrimaryConstructor(model);
        methodSymbol.Should().NotBeNull();
        methodSymbol.MethodKind.Should().Be(MethodKind.Constructor);
        methodSymbol.Parameters.Should().BeEmpty();
    }

    [TestMethod]
    public void PrimaryConstructor_EmptyPrimaryConstructor_SecondConstructor()
    {
        var (tree, model) = TestHelper.CompileCS("""
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
        var (tree, model) = TestHelper.CompileCS("""
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

    [DataTestMethod]
    [DataRow("__arglist", 0)]
    [DataRow("int i, __arglist", 1)]
    [DataRow("int i, int j, __arglist", 2)]
    public void PrimaryConstructor_ArglistPrimaryConstructor(string parameterList, int expectedNumberOfParameters)
    {
        var (tree, model) = TestHelper.CompileCS($$"""public class Test({{parameterList}}) { }""");
        var typeDeclaration = tree.GetCompilationUnitRoot().DescendantNodesAndSelf().OfType<TypeDeclarationSyntax>().Single();
        var methodSymbol = typeDeclaration.PrimaryConstructor(model);
        methodSymbol.Should().NotBeNull();
        methodSymbol.MethodKind.Should().Be(MethodKind.Constructor);
        methodSymbol.Parameters.Should().HaveCount(expectedNumberOfParameters);
    }
}
