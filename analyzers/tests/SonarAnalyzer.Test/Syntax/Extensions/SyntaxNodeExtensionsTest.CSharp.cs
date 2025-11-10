/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2014-2025 SonarSource Sàrl
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

using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SonarAnalyzer.CSharp.Core.Syntax.Extensions;
using SonarAnalyzer.CSharp.Syntax.Extensions;

namespace SonarAnalyzer.Test.Syntax.Extensions;

[TestClass]
public class SyntaxNodeExtensionsTest
{
    [TestMethod]
    public void NoDirectives()
    {
        var source = """
            class Sample
            {
                void Main(){}
            }
            """;
        var node = GetMainNode(source);
        var activeSections = node.ActiveConditionalCompilationSections();

        activeSections.Should().NotBeNull();
        activeSections.Should().BeEmpty();
    }

    [TestMethod]
    public void ActiveBlocks_NonNestedIfs()
    {
        var source = """
            #define BLOCK1
            #define BLOCK2
            #define BLOCK3

            namespace Test
            {
            #if BLOCK1
            #endif

            #if BLOCK2
            #endif

            #if true // literal block
            #endif

            #if BLOCK3

                class Sample
                {
                    void Main() { }
                }

            #endif

            #if BLOCK1
            #endif

            }
            """;
        var node = GetMainNode(source);
        var activeSections = node.ActiveConditionalCompilationSections();

        activeSections.Should().NotBeNull();
        activeSections.Should().ContainSingle();
        activeSections.Should().BeEquivalentTo("BLOCK3");
    }

    [TestMethod]
    public void ActiveBlocks_NestedIfs()
    {
        // Arrange
        var source = """
            #define BLOCK1
            #define BLOCK2
            #define BLOCK3
            #define BLOCK4

            namespace Test
            {
            #if BLOCK1
            #if BLOCK2
            #if BLOCK3
            #if BLOCK4 // opened and closed, so should not appear
            #endif

                class Sample
                {
                    void Main() { }
                }

            #endif
            #endif
            #endif

            #if BLOCK1
            #endif

            }
            """;
        var node = GetMainNode(source);
        var activeSections = node.ActiveConditionalCompilationSections();

        activeSections.Should().NotBeNull();
        activeSections.Should().HaveCount(3);
        activeSections.Should().BeEquivalentTo("BLOCK1", "BLOCK2", "BLOCK3");
    }

    [TestMethod]
    public void ActiveBlocks_DirectivesInLeadingTrivia()
    {
        var source = """
            #define BLOCK1
            #define BLOCK2
            #define BLOCK3

            namespace Test
            {
                public class Sample
                {
            // trivia
            #if BLOCK2
            // more trivia
            #if true // literal block
            // more trivia
            #endif
            #if BLOCK3
            // more trivia
                    void Main() { }
                }

            #endif
            #endif

            #if BLOCK1
            #endif

            }
            """;
        var node = GetMainNode(source);
        var activeSections = node.ActiveConditionalCompilationSections();

        activeSections.Should().NotBeNull();
        activeSections.Should().BeEquivalentTo("BLOCK2", "BLOCK3");
    }

    [TestMethod]
    public void ActiveBlocks_ElseInPrecedingCode()
    {
        var source = """
            #define BLOCK2

            #if BLOCK1
            #else

            #if BLOCK2
            #else
            #elseif BLOCK3
            #endif

            #endif

            #if BLOCK2
            namespace Test
            {
                class Sample
                {
                    void Main() { }
                }
            }
            #endif
            """;
        var node = GetMainNode(source);
        var activeSections = node.ActiveConditionalCompilationSections();

        activeSections.Should().NotBeNull();
        activeSections.Should().BeEquivalentTo("BLOCK2");
    }

    [TestMethod]
    public void ActiveBlocks_NegativeConditions_InIf()
    {
        var source = """
            namespace Test
            {
            #if !BLOCK1
                class Sample
                {
                    void Main() { }
                }
            #else
            #endif
            }
            """;
        var node = GetMainNode(source);
        var activeSections = node.ActiveConditionalCompilationSections();

        activeSections.Should().NotBeNull();
        activeSections.Should().BeEmpty();
    }

    [TestMethod]
    public void ActiveBlocks_NegativeConditions_InElse()
    {
        var source = """
            #define BLOCK1

            namespace Test
            {
            #if !BLOCK1
            #else
                class Sample
                {
                    void Main() { }
                }
            #endif
            }
            """;
        var node = GetMainNode(source);
        var activeSections = node.ActiveConditionalCompilationSections();

        activeSections.Should().NotBeNull();
        activeSections.Should().BeEmpty();
    }

    [TestMethod]
    public void ActiveBlocks_Else_FirstBranchIsActive()
    {
        var source = """
            #define BLOCK1

            namespace Test
            {
            #if BLOCK1
                class Sample
                {
                    void Main() { }
            #else
                class Sample
                {
                    void Main() {}
            #endif
            """;
        var node = GetMainNode(source);
        var activeSections = node.ActiveConditionalCompilationSections();

        activeSections.Should().NotBeNull();
        activeSections.Should().BeEquivalentTo("BLOCK1");
    }

    [TestMethod]
    public void ActiveBlocks_Else_SecondBranchIsActive()
    {
        var source = """
            #define BLOCK2

            namespace Test
            {
            #if BLOCK1
                class Sample
                {
                    void Main() { }
            #else
                class Sample
                {
                    void Main() {}
            #endif
            """;
        var node = GetMainNode(source);
        var activeSections = node.ActiveConditionalCompilationSections();

        activeSections.Should().NotBeNull();
        activeSections.Should().BeEmpty();
    }

    [TestMethod]
    public void ActiveBlocks_Elif_FirstBranchIsActive()
    {
        var source = """
            #define BLOCK1
            #define BLOCK2

            namespace Test
            {
            #if BLOCK1
                class Sample
                {
                    void Main() { }
            #elif BLOCK2
                class Sample
                {
                    void Main() {}
            #endif
                }
            }
            """;
        var node = GetMainNode(source);
        var activeSections = node.ActiveConditionalCompilationSections();

        activeSections.Should().NotBeNull();
        activeSections.Should().BeEquivalentTo("BLOCK1");
    }

    [TestMethod]
    public void ActiveBlocks_Elif_SecondBranchIsActive()
    {
        var source = """
            #define BLOCK2

            namespace Test
            {
            #if BLOCK1
                class Sample
                {
                    void Main() { }
            #elif BLOCK2
                class Sample
                {
                    void Main() {}
            #endif
                }
            }
            """;
        var node = GetMainNode(source);
        var activeSections = node.ActiveConditionalCompilationSections();

        activeSections.Should().NotBeNull();
        activeSections.Should().BeEquivalentTo("BLOCK2");
    }

    [TestMethod]
    public void ActiveBlocks_Elif_FirstBranchIsActive_InLeadingTrivia()
    {
        var source = """
            #define BLOCK1
            #define BLOCK2

            namespace Test
            {
                class Sample
                {
            #if BLOCK1
                    void Main() { }
            #elif BLOCK2
                    void Main() {}
            #endif
                }
            }
            """;
        var node = GetMainNode(source);
        var activeSections = node.ActiveConditionalCompilationSections();

        activeSections.Should().NotBeNull();
        activeSections.Should().BeEquivalentTo("BLOCK1");
    }

    [TestMethod]
    public void ActiveBlocks_Elif_SecondBranchIsActive_InLeadingTrivia()
    {
        var source = """
            #define BLOCK2

            namespace Test
            {
                class Sample
                {
            #if BLOCK1
                    void Main() { }
            #elif BLOCK2
                    void Main() {}
            #endif
                }
            }
            """;
        var node = GetMainNode(source);
        var activeSections = node.ActiveConditionalCompilationSections();

        activeSections.Should().NotBeNull();
        activeSections.Should().BeEquivalentTo("BLOCK2");
    }

    [TestMethod]
    public void InactiveDirectives_ShouldBeIgnored()
    {
        var source = """
            #define BLOCK1
            #define BLOCK2
            #define BLOCK3
            #define BLOCK4

            namespace Test
            {
            #if INACTIVE1
            #if BLOCK1  // inside inactive block -> ignored
            #endif
            #endif

            #if BLOCK3
                class Sample
                {
            #if BLOCK4
            #if INACTIVE2
            #if BLOCK2  // inside inactive block -> ignored
            #endif
            #endif
                    void Main() { }
                }

            #endif

            #if BLOCK1
            #endif

            }
            """;
        var node = GetMainNode(source);
        var activeSections = node.ActiveConditionalCompilationSections();

        activeSections.Should().NotBeNull();
        activeSections.Should().BeEquivalentTo("BLOCK3", "BLOCK4");
    }

    [TestMethod]
    public void BadDirectives_ShouldBeIgnored()
    {
        var source = """
            #define BLOCK2

            #if BLOCK1
            #endif
            #else // bad directive

            #endif // bad directive

            #if BLOCK2
            #FOO // bad directive
            namespace Test
            {
                class Sample
                {
            #BAR // bad directive
                    void Main() { }
                }
            }
            """;
        var node = GetMainNode(source);
        var activeSections = node.ActiveConditionalCompilationSections();

        activeSections.Should().NotBeNull();
        activeSections.Should().BeEquivalentTo("BLOCK2");
    }

    [TestMethod]
    public void GetName_CS()
    {
        const string code = """
            using System;
            using C = System.Collections;
            namespace MyNamespace.MyNamespaceNested
            {
                class Example
                {
                    delegate void MyDelegate();
                    enum MyEnum { MyEnumValue };

                    Example() { }
                    ~Example() { }
                    int MyProp { get; }

                    unsafe ref byte Method<T>(byte[] input) where T: new()
                    {
                        int? i = null;
                        int* iPtr;
                        System.Exception qualified;
                        global::System.Exception global;
                        input.ToString()?.ToString();
                        Func<Action> fun = () => () => {};
                        fun()();
                        ref byte result = ref input[0];
                        return ref result;
                    }
                    public static explicit operator int(Example e) => 0;
                    public static int operator +(Example e) => 0;
                    ref struct MyRefStruct { }
                }
            }
            """;

        var nodes = CSharpSyntaxTree.ParseText(code).GetRoot().DescendantNodes().ToArray();
        Assert<AliasQualifiedNameSyntax>("global");
        Assert<ArrayTypeSyntax>("byte");
        Assert<BaseTypeDeclarationSyntax>("Example", "MyEnum", "MyRefStruct");
        Assert<ConstructorDeclarationSyntax>("Example");
        Assert<ConversionOperatorDeclarationSyntax>("int");
        Assert<DelegateDeclarationSyntax>("MyDelegate");
        Assert<DestructorDeclarationSyntax>("Example");
        Assert<EnumMemberDeclarationSyntax>("MyEnumValue");
        Assert<IdentifierNameSyntax>("System", "C", "System", "Collections", "MyNamespace", "MyNamespaceNested", "T", "System", "Exception", "global", "System", "Exception", "input", "ToString", "ToString", "Action", "fun", "input", "result", "Example", "Example");
        Assert<InvocationExpressionSyntax>("ToString", "ToString", string.Empty, "fun");
        Assert<MethodDeclarationSyntax>("Method");
        Assert<MemberAccessExpressionSyntax>("ToString");
        Assert<MemberBindingExpressionSyntax>("ToString");
        Assert<NamespaceDeclarationSyntax>("MyNamespaceNested");
        Assert<NullableTypeSyntax>("int");
        Assert<OperatorDeclarationSyntax>("+");
        Assert<ParameterSyntax>("input", "e", "e");
        Assert<PropertyDeclarationSyntax>("MyProp");
        Assert<PointerTypeSyntax>("int");
        Assert<PredefinedTypeSyntax>("void", "int", "int", "int", "int", "int", "byte", "byte", "byte");
        Assert<QualifiedNameSyntax>("Collections", "MyNamespaceNested", "Exception", "Exception");
        Assert<SimpleNameSyntax>("System", "C", "System", "Collections", "MyNamespace", "MyNamespaceNested", "T", "System", "Exception", "global", "System", "Exception", "input", "ToString", "ToString", "Func", "Action", "fun", "input", "result", "Example", "Example");
        Assert<TypeParameterConstraintClauseSyntax>("T");
        Assert<TypeParameterSyntax>("T");
        Assert<UsingDirectiveSyntax>(string.Empty, "C");
        Assert<VariableDeclaratorSyntax>("i", "iPtr", "qualified", "global", "fun", "result");
        Assert<RefTypeSyntax>("byte", "byte");
        Assert<ReturnStatementSyntax>(string.Empty);

        void Assert<T>(params string[] expectedNames) where T : SyntaxNode =>
            nodes.OfType<T>().Select(x => SyntaxNodeExtensionsCSharp.GetName(x)).Should().BeEquivalentTo(expectedNames, because: "GetName for {0} should return the identifier", typeof(T));
    }

    [TestMethod]
    public void IsNullLiteral_Null_CS() =>
        SyntaxNodeExtensionsCSharp.IsNullLiteral(null).Should().BeFalse();

    private static MethodDeclarationSyntax GetMainNode(string source) =>
        CSharpSyntaxTree.ParseText(source).GetRoot()
            .DescendantNodes()
            .OfType<MethodDeclarationSyntax>()
            .First(x => x.Identifier.ValueText == "Main");
}
