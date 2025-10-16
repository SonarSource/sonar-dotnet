/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2014-2025 SonarSource SA
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

using FluentAssertions.Extensions;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Operations;
using Microsoft.CodeAnalysis.Text;
using SonarAnalyzer.CFG.Roslyn;
using SonarAnalyzer.Core.Syntax.Extensions;
using SonarAnalyzer.CSharp.Core.Syntax.Extensions;
using SonarAnalyzer.VisualBasic.Core.Syntax.Extensions;
using StyleCop.Analyzers.Lightup;
using ExtensionsCore = SonarAnalyzer.Core.Syntax.Extensions.SyntaxNodeExtensions;
using ExtensionsShared = SonarAnalyzer.CSharp.Core.Syntax.Extensions.SyntaxNodeExtensionsShared;
using MicrosoftExtensionsCS = Microsoft.CodeAnalysis.CSharp.Extensions.SyntaxNodeExtensions;
using SyntaxCS = Microsoft.CodeAnalysis.CSharp.Syntax;
using SyntaxTokenExtensions = SonarAnalyzer.CSharp.Core.Syntax.Extensions.SyntaxTokenExtensionsShared;
using SyntaxVB = Microsoft.CodeAnalysis.VisualBasic.Syntax;

namespace SonarAnalyzer.Test.Extensions;

[TestClass]
public class SyntaxNodeExtensionsTest
{
    private const string DefaultFileName = "Test.cs";

    [TestMethod]
    public void GetPreviousStatementsCurrentBlockOfNotAStatement()
    {
        const string code = "int x = 42;";

        var tree = CSharpSyntaxTree.ParseText(code);
        var compilationUnit = tree.GetCompilationUnitRoot();

        ExtensionsShared.GetPreviousStatementsCurrentBlock(compilationUnit).Should().BeEmpty();
    }

    [TestMethod]
    public void GetPreviousStatementsCurrentBlockOfFirstStatement()
    {
        const string code = "public void M() { int x = 42; }";

        var tree = CSharpSyntaxTree.ParseText(code);
        var aToken = GetFirstTokenOfKind(tree, SyntaxKind.NumericLiteralToken);
        var parent = SyntaxTokenExtensions.GetBindableParent(aToken);
        ExtensionsShared.GetPreviousStatementsCurrentBlock(parent).Should().BeEmpty();
    }

    [TestMethod]
    public void GetPreviousStatementsCurrentBlockOfSecondStatement()
    {
        const string code = "public void M() { string s = null; int x = 42; }";

        var tree = CSharpSyntaxTree.ParseText(code);
        var aToken = GetFirstTokenOfKind(tree, SyntaxKind.NumericLiteralToken);
        var parent = SyntaxTokenExtensions.GetBindableParent(aToken);
        ExtensionsShared.GetPreviousStatementsCurrentBlock(parent).Should().HaveCount(1);
    }

    [TestMethod]
    public void GetPreviousStatementsCurrentBlockRetrievesOnlyForCurrentBlock()
    {
        const string code = "public void M(string y) { string s = null; if (y != null) { int x = 42; } }";

        var tree = CSharpSyntaxTree.ParseText(code);
        var aToken = GetFirstTokenOfKind(tree, SyntaxKind.NumericLiteralToken);
        var parent = SyntaxTokenExtensions.GetBindableParent(aToken);
        ExtensionsShared.GetPreviousStatementsCurrentBlock(parent).Should().BeEmpty();
    }

    [TestMethod]
    public void ArrowExpressionBody_WithNotExpectedNode_ReturnsNull()
    {
        const string code = "var x = 1;";
        var tree = CSharpSyntaxTree.ParseText(code);
        SyntaxNodeExtensionsCSharp.ArrowExpressionBody(tree.GetRoot()).Should().BeNull();
    }

    [TestMethod]
    public void GetDeclarationTypeName_UnknownType() =>
#if DEBUG
        Assert.Throws<UnexpectedValueException>(() => SyntaxNodeExtensionsCSharp.GetDeclarationTypeName(SyntaxFactory.Block()), "Unexpected type Block\r\nParameter name: kind");
#else
        SyntaxNodeExtensionsCSharp.GetDeclarationTypeName(SyntaxFactory.Block()).Should().Be("type");
#endif

    [TestMethod]
    public void GetDeclarationTypeName_Class() =>
        SyntaxNodeExtensionsCSharp.GetDeclarationTypeName(SyntaxFactory.ClassDeclaration("MyClass")).Should().Be("class");

    [TestMethod]
    public void GetDeclarationTypeName_Constructor() =>
        SyntaxNodeExtensionsCSharp.GetDeclarationTypeName(SyntaxFactory.ConstructorDeclaration("MyConstructor")).Should().Be("constructor");

    [TestMethod]
    public void GetDeclarationTypeName_Delegate() =>
        SyntaxNodeExtensionsCSharp.GetDeclarationTypeName(SyntaxFactory.DelegateDeclaration(SyntaxFactory.ParseTypeName("void"), "MyDelegate")).Should().Be("delegate");

    [TestMethod]
    public void GetDeclarationTypeName_Destructor() =>
        SyntaxNodeExtensionsCSharp.GetDeclarationTypeName(SyntaxFactory.DestructorDeclaration("~")).Should().Be("destructor");

    [TestMethod]
    public void GetDeclarationTypeName_Enum() =>
        SyntaxNodeExtensionsCSharp.GetDeclarationTypeName(SyntaxFactory.EnumDeclaration("MyEnum")).Should().Be("enum");

    [TestMethod]
    public void GetDeclarationTypeName_EnumMember() =>
        SyntaxNodeExtensionsCSharp.GetDeclarationTypeName(SyntaxFactory.EnumMemberDeclaration("EnumValue1")).Should().Be("enum");

    [TestMethod]
    public void GetDeclarationTypeName_Event() =>
        SyntaxNodeExtensionsCSharp.GetDeclarationTypeName(SyntaxFactory.EventDeclaration(SyntaxFactory.ParseTypeName("void"), "MyEvent")).Should().Be("event");

    [TestMethod]
    public void GetDeclarationTypeName_EventField() =>
        SyntaxNodeExtensionsCSharp.GetDeclarationTypeName(SyntaxFactory.EventFieldDeclaration(SyntaxFactory.VariableDeclaration(SyntaxFactory.ParseTypeName("MyEvent")))).Should().Be("event");

    [TestMethod]
    public void GetDeclarationTypeName_Field() =>
        SyntaxNodeExtensionsCSharp.GetDeclarationTypeName(SyntaxFactory.FieldDeclaration(SyntaxFactory.VariableDeclaration(SyntaxFactory.ParseTypeName("int")))).Should().Be("field");

    [TestMethod]
    public void GetDeclarationTypeName_Indexer() =>
        SyntaxNodeExtensionsCSharp.GetDeclarationTypeName(SyntaxFactory.IndexerDeclaration(SyntaxFactory.ParseTypeName("int"))).Should().Be("indexer");

    [TestMethod]
    public void GetDeclarationTypeName_Interface() =>
        SyntaxNodeExtensionsCSharp.GetDeclarationTypeName(SyntaxFactory.InterfaceDeclaration("MyInterface")).Should().Be("interface");

    [TestMethod]
    public void GetDeclarationTypeName_LocalFunction() =>
        SyntaxNodeExtensionsCSharp.GetDeclarationTypeName(SyntaxFactory.LocalFunctionStatement(SyntaxFactory.ParseTypeName("void"), "MyLocalFunction")).Should().Be("local function");

    [TestMethod]
    public void GetDeclarationTypeName_Method() =>
        SyntaxNodeExtensionsCSharp.GetDeclarationTypeName(SyntaxFactory.MethodDeclaration(SyntaxFactory.ParseTypeName("void"), "MyMethod")).Should().Be("method");

    [TestMethod]
    public void GetDeclarationTypeName_Property() =>
        SyntaxNodeExtensionsCSharp.GetDeclarationTypeName(SyntaxFactory.PropertyDeclaration(SyntaxFactory.ParseTypeName("void"), "MyProperty")).Should().Be("property");

    [TestMethod]
    public void GetDeclarationTypeName_Record() =>
        SyntaxNodeExtensionsCSharp.GetDeclarationTypeName(SyntaxFactory.RecordDeclaration(SyntaxFactory.Token(SyntaxKind.RecordKeyword), "MyRecord")).Should().Be("record");

    [TestMethod]
    public void GetDeclarationTypeName_RecordStruct() =>
        SyntaxNodeExtensionsCSharp.GetDeclarationTypeName(SyntaxFactory.RecordDeclaration(SyntaxKind.RecordStructDeclaration, SyntaxFactory.Token(SyntaxKind.RecordKeyword), "MyRecord"))
            .Should().Be("record struct");

    [TestMethod]
    public void GetDeclarationTypeName_Struct() =>
        SyntaxNodeExtensionsCSharp.GetDeclarationTypeName(SyntaxFactory.StructDeclaration("MyStruct")).Should().Be("struct");

    [TestMethod]
    public void CreateCfg_MethodDeclaration_ReturnsCfg_CS()
    {
        const string code = """
            public class Sample
            {
                public void Main()
                {
                    var x = 42;
                }
            }
            """;
        CreateCfgCS<SyntaxCS.MethodDeclarationSyntax>(code).Should().NotBeNull();
    }

    [TestMethod]
    public void CreateCfg_PropertyDeclaration_ReturnsCfg_CS()
    {
        const string code = """
            public class Sample
            {
                public int Property => 42;
            }
            """;
        CreateCfgCS<SyntaxCS.PropertyDeclarationSyntax>(code).Should().NotBeNull();
    }

    [TestMethod]
    public void CreateCfg_PropertyDeclarationWithoutExpressionBody_ReturnsNull_CS()
    {
        const string code = """
            public class Sample
            {
                public int Property {get; set;}
            }
            """;
        CreateCfgCS<SyntaxCS.PropertyDeclarationSyntax>(code).Should().BeNull();
    }

    [TestMethod]
    public void CreateCfg_IndexerDeclaration_ReturnsCfg_CS()
    {
        const string code = """
            public class Sample
            {
                private string field;
                public string this[int index] => field = null;
            }
            """;
        CreateCfgCS<SyntaxCS.IndexerDeclarationSyntax>(code).Should().NotBeNull();
    }

    [TestMethod]
    public void CreateCfg_FieldInitializerWithoutOperation_ReturnsCfg_CS()
    {
        const string code = """
            public class Sample
            {
                private string field = null!;   // null! itself doens't have operation, and we can still generate CFG for it from the equals clause
            }
            """;
        CreateCfgCS<SyntaxCS.EqualsValueClauseSyntax>(code).Should().NotBeNull();
    }

    [TestMethod]
    public void CreateCfg_MethodBlock_ReturnsCfg_VB()
    {
        const string code = """
            Public Class Sample
                Public Sub Main()
                    Dim X As Integer = 42
                End Sub
            End Class
            """;
        CreateCfgVB<SyntaxVB.MethodBlockSyntax>(code).Should().NotBeNull();
    }

    [TestMethod]
    public void CreateCfg_AnyNode_ReturnsCfg_CS()
    {
        const string code = """
            public class Sample
            {
                public void Main()
                {
                    Main();
                }
            }
            """;
        CreateCfgCS<SyntaxCS.InvocationExpressionSyntax>(code).Should().NotBeNull();
    }

    [TestMethod]
    public void CreateCfg_AnyNode_ReturnsCfg_VB()
    {
        const string code = """
            Public Class Sample
                Public Sub Main()
                    Main()
                End Sub
            End Class
            """;
        CreateCfgVB<SyntaxVB.InvocationExpressionSyntax>(code).Should().NotBeNull();
    }

    [TestMethod]
    public void CreateCfg_LambdaInsideQuery_CS()
    {
        const string code = """
            using System;
            using System.Linq;
            public class Sample
            {
                public void Main(int[] values)
                {
                    var result = from value in values select new Lazy<int>(() => value);
                }
            }
            """;
        CreateCfgCS<SyntaxCS.ParenthesizedLambdaExpressionSyntax>(code).Should().NotBeNull();
    }

    [TestMethod]
    public void CreateCfg_LambdaInsideQuery_VB()
    {
        const string code = """
            Public Class Sample
                Public Sub Main(Values() As Integer)
                    Dim Result As IEnumerable(Of Lazy(Of Integer)) = From Value In Values Select New Lazy(Of Integer)(Function() Value)
                End Sub
            End Class
            """;
        CreateCfgVB<SyntaxVB.SingleLineLambdaExpressionSyntax>(code).Should().NotBeNull();
    }

    [TestMethod]
    public void CreateCfg_NestingChain_CS()
    {
        const string code = """
            using System;
            using System.Linq;
            public class Sample
            {
                public void Main(int[] values)
                {
                    OuterLocalFunction();

                    void OuterLocalFunction()
                    {
                        Action<int, int> outerParenthesizedLambda = (a, b) =>
                        {
                            MiddleLocalFunction(42);

                            void MiddleLocalFunction(int c)
                            {
                                var queryExpressionInTheWay = from value in values select new Lazy<int>(() =>
                                    {
                                        return InnerLocalFunction(value);

                                        static int InnerLocalFunction(int arg)
                                        {
                                            Func<int, int> innerLambda = xxx => xxx;

                                            return innerLambda(arg);
                                        }
                                    });
                            }
                        };
                    }
                }
            }
            """;
        var (tree, semanticModel) = TestCompiler.CompileCS(code);
        var innerLambda = tree.Single<SyntaxCS.SimpleLambdaExpressionSyntax>();
        innerLambda.Parent.Parent.Should().BeOfType<SyntaxCS.VariableDeclaratorSyntax>().Subject.Identifier.ValueText.Should().Be("innerLambda");

        var cfg = SyntaxNodeExtensionsCSharp.CreateCfg(innerLambda, semanticModel, default);
        cfg.Should().NotBeNull("It's innerLambda");
        cfg.Parent.Should().NotBeNull("It's InnerLocalFunction");
        cfg.Parent.Parent.Should().NotBeNull("Lambda iniside Lazy<int> constructor");
        cfg.Parent.Parent.Parent.Should().NotBeNull("Anonymous function for query expression");
        cfg.Parent.Parent.Parent.Parent.Should().NotBeNull("It's MiddleLocalFunction");
        cfg.Parent.Parent.Parent.Parent.Parent.Should().NotBeNull("It's outerParenthesizedLambda");
        cfg.Parent.Parent.Parent.Parent.Parent.Parent.Should().NotBeNull("It's OuterLocalFunction");
        cfg.Parent.Parent.Parent.Parent.Parent.Parent.Parent.Should().NotBeNull("It's the root CFG");
        cfg.Parent.Parent.Parent.Parent.Parent.Parent.Parent.Parent.Should().BeNull("Root CFG should not have Parent");
        cfg.OriginalOperation.Should().BeAssignableTo<IAnonymousFunctionOperation>().Subject.Symbol.Parameters.Should().HaveCount(1).And.Contain(x => x.Name == "xxx");
    }

    [TestMethod]
    public void CreateCfg_NestingChain_VB()
    {
        const string code = """
            Public Class Sample
                Public Sub Main(Values() As Integer)
                    Dim OuterMultiLineSub As Action =
                        Sub()
                            Dim OuterSingleLineFunc As Func(Of Func(Of Integer, Integer)) =
                                    Function() Function(NestedMultiLineFuncArg As Integer)
                                                    Dim Lst = From Value
                                                              In Values
                                                              Select New Lazy(Of Integer)(Function()
                                                                                              Dim InnerSingleLineSub = Sub(xxx As Integer) Value.ToString()
                                                                                          End Function)
                                                End Function
                        End Sub
                End Sub
            End Class
            """;
        var (tree, semanticModel) = TestCompiler.CompileVB(code);
        var innerSub = tree.Single<SyntaxVB.InvocationExpressionSyntax>().FirstAncestorOrSelf<SyntaxVB.SingleLineLambdaExpressionSyntax>();
        innerSub.Parent.Parent.Should().BeOfType<SyntaxVB.VariableDeclaratorSyntax>().Subject.Names.Single().Identifier.ValueText.Should().Be("InnerSingleLineSub");

        var cfg = SyntaxNodeExtensionsVisualBasic.CreateCfg(innerSub, semanticModel, default);
        cfg.Should().NotBeNull("It's InnerSingleLineSub");
        cfg.Parent.Should().NotBeNull("It's multiline function inside Lazy(Of Integer)");
        cfg.Parent.Parent.Should().NotBeNull("Lambda iniside Lazy<int> constructor");
        cfg.Parent.Parent.Parent.Should().NotBeNull("Anonymous function for query expression");
        cfg.Parent.Parent.Parent.Parent.Should().NotBeNull("It's OuterSingleLineFunc");
        cfg.Parent.Parent.Parent.Parent.Parent.Should().NotBeNull("It's OuterMultiLineSub");
        cfg.Parent.Parent.Parent.Parent.Parent.Parent.Should().NotBeNull("It's the root CFG");
        cfg.Parent.Parent.Parent.Parent.Parent.Parent.Parent.Should().BeNull("Root CFG should not have Parent");
        cfg.OriginalOperation.Should().BeAssignableTo<IAnonymousFunctionOperation>().Subject.Symbol.Parameters.Should().HaveCount(1).And.Contain(x => x.Name == "xxx");
    }

    [TestMethod]
    public void CreateCfg_UndefinedSymbol_ReturnsCfg_CS()
    {
        const string code = """
            public class Sample
            {
                public void Main()
                {
                    Undefined(() => 45);
                }
            }
            """;
        var (tree, model) = TestCompiler.CompileIgnoreErrorsCS(code);
        var lambda = tree.Single<SyntaxCS.ParenthesizedLambdaExpressionSyntax>();
        SyntaxNodeExtensionsCSharp.CreateCfg(lambda, model, default).Should().NotBeNull();
    }

    [TestMethod]
    public void CreateCfg_UndefinedSymbol_ReturnsNull_VB()
    {
        const string code = """
            Public Class Sample
                Public Sub Main()
                    Undefined(Function() 42)
                End Sub
            End Class
            """;
        var (tree, model) = TestCompiler.CompileIgnoreErrorsVB(code);
        var lambda = tree.Single<SyntaxVB.SingleLineLambdaExpressionSyntax>();
        SyntaxNodeExtensionsVisualBasic.CreateCfg(lambda, model, default).Should().BeNull();
    }

    [TestMethod]
    [DataRow(@"() =>")]
    [DataRow(@"} () => }")]
    [DataRow(@"{ () => .")]
    [DataRow(@"{ () => => =>")]
    public void CreateCfg_InvalidSyntax_ReturnsCfg_CS(string code)
    {
        var (tree, model) = TestCompiler.CompileIgnoreErrorsCS(code);
        var lambda = tree.Single<SyntaxCS.ParenthesizedLambdaExpressionSyntax>();
        SyntaxNodeExtensionsCSharp.CreateCfg(lambda, model, default).Should().NotBeNull();
    }

    [TestMethod]
    public void CreateCfg_Performance_UsesCache_CS()
    {
        const string code = """
            using System;
            public class Sample
            {
                public void Main(string noiseToHaveMoreOperations)
                {
                    noiseToHaveMoreOperations.ToString();
                    noiseToHaveMoreOperations.ToString();
                    noiseToHaveMoreOperations.ToString();
                    void LocalFunction()
                    {
                        noiseToHaveMoreOperations.ToString();
                        noiseToHaveMoreOperations.ToString();
                        noiseToHaveMoreOperations.ToString();
                        Action a = () => 42.ToString();
                    }
                }
            }
            """;
        var (tree, model) = TestCompiler.CompileCS(code);
        var lambda = tree.Single<SyntaxCS.ParenthesizedLambdaExpressionSyntax>();
        Action a = () =>
            {
                for (var i = 0; i < 10000; i++)
                {
                    SyntaxNodeExtensionsCSharp.CreateCfg(lambda, model, default);
                }
            };
        a.ExecutionTime().Should().BeLessThan(1500.Milliseconds());     // Takes roughly 0.2 sec on CI
    }

    [TestMethod]
    public void CreateCfg_Performance_UsesCache_VB()
    {
        const string code = """
            Public Class Sample
                Public Sub Main(NoiseToHaveMoreOperations As String)
                    NoiseToHaveMoreOperations.ToString()
                    NoiseToHaveMoreOperations.ToString()
                    NoiseToHaveMoreOperations.ToString()
                    Dim Outer As Action = Sub()
                                              NoiseToHaveMoreOperations.ToString()
                                              NoiseToHaveMoreOperations.ToString()
                                              NoiseToHaveMoreOperations.ToString()
                                              Dim Inner As Action = Sub() NoiseToHaveMoreOperations.ToString()
                                          End Sub
                End Sub
            End Class
            """;
        var (tree, model) = TestCompiler.CompileVB(code);
        var lambda = tree.Single<SyntaxVB.SingleLineLambdaExpressionSyntax>();
        Action a = () =>
        {
            for (var i = 0; i < 10000; i++)
            {
                SyntaxNodeExtensionsCSharp.CreateCfg(lambda, model, default);
            }
        };
        a.ExecutionTime().Should().BeLessThan(1.Seconds());     // Takes roughly 0.4 sec on CI
    }

    [TestMethod]
    public void CreateCfg_SameNode_DifferentCompilation_DoesNotCache()
    {
        // https://github.com/SonarSource/sonar-dotnet/issues/5491
        const string code = """
            public class Sample
            {
                private void Method()
                { }
            }
            """;
        var compilation1 = TestCompiler.CompileCS(code).Model.Compilation;
        var compilation2 = compilation1.WithAssemblyName("Different-Compilation-Reusing-Same-Nodes");
        var method1 = compilation1.SyntaxTrees.Single().Single<SyntaxCS.MethodDeclarationSyntax>();
        var method2 = compilation2.SyntaxTrees.Single().Single<SyntaxCS.MethodDeclarationSyntax>();
        var cfg1 = SyntaxNodeExtensionsCSharp.CreateCfg(method1, compilation1.GetSemanticModel(method1.SyntaxTree), default);
        var cfg2 = SyntaxNodeExtensionsCSharp.CreateCfg(method2, compilation2.GetSemanticModel(method2.SyntaxTree), default);

        ReferenceEquals(cfg1, cfg2).Should().BeFalse("Different compilations should not reuse cache. They do not share semantic model and symbols.");
    }

    [TestMethod]
    // Tuple. From right to left.
    [DataRow("(var a, (x, var b)) = (0, ($$x++, 1));", "x")]
    [DataRow("(var a, (x, var b)) = (0, $$(1, 2));", "(x, var b)")]
    [DataRow("(var a, (var b, var c, var d), var e) = (0, (1, 2, $$3), 4);", "var d")]
    // Tuple. From left to right
    [DataRow("(var a, (x, $$var b)) = (0, (x++, 1));", "1")]
    [DataRow("(var a, (var b, var c, $$var d), var e) = (0, (1, 2, 3), 4);", "3")]
    [DataRow("(var a, $$(var b, var c)) = (0, (1, 2));", "(1, 2)")]
    // Designation. From right to left.
    [DataRow("var (a, (b, c)) = (0, (1, $$2));", "c")]
    [DataRow("var (a, (b, c)) = (0, $$(1, 2));", "(b, c)")]
    [DataRow("var (a, (b, _)) = (0, (1, $$2));", "_")]
    [DataRow("var (a, _) = (0, ($$1, 2));", null)]
    [DataRow("var (a, _) = (0, $$(1, 2));", "_")]
    [DataRow("var _ = ($$0, (1, 2));", null)]
    [DataRow("_ = ($$0, (1, 2));", null)]
    // Designation. From left to right
    [DataRow("var (a, (b, $$c)) = (0, (1, 2));", "2")]
    [DataRow("var (a, $$(b, c)) = (0, (1, 2));", "(1, 2)")]
    [DataRow("var (a, (b, $$_)) = (0, (1, 2));", "2")]
    [DataRow("var (a, $$_) = (0, (1, 2));", "(1, 2)")]
    // Unaligned tuples. From left to right.
    [DataRow("(var a, var b) = ($$0, 1, 2);", null)]
    [DataRow("(var a, var b) = (0, 1, $$2);", null)]
    [DataRow("(var a, var b) = (0, (1, $$2));", null)]
    [DataRow("(var a, (var b, var c)) = (0, $$1);", "(var b, var c)")] // Syntacticly correct
    // Unaligned tuples. From right to left.
    [DataRow("(var a, var b, $$var c) = (0, (1, 2));", null)]
    [DataRow("(var a, (var b, $$var c)) = (0, 1, 2);", null)]
    // Unaligned designation. From right to left.
    [DataRow("var (a, (b, c)) = (0, (1, $$2, 3));", null)]
    [DataRow("var (a, (b, c)) = (0, (1, ($$2, 3)));", null)]
    [DataRow("var (a, (b, c, d)) = (0, (1, $$2));", null)]
    // Unaligned designation. From left to right .
    [DataRow("var (a, (b, $$c)) = (0, (1, 2, 3));", null)]
    [DataRow("var (a, (b, ($$c, d))) = (0, (1, 2));", null)]
    [DataRow("var (a, (b, $$c, d)) = (0, (1, 2));", null)]
    // Mixed. From right to left.
    [DataRow("(var a, var (b, c)) = (1, ($$2, 3));", "b")]
    [DataRow("(var a, var (b, (c, (d, e)))) = (1, (2, (3, (4, $$5))));", "e")]
    // Mixed. From left to right.
    [DataRow("(var a, var ($$b, c) )= (1, (2, 3));", "2")]
    [DataRow("(var a, var (b, (c, (d, $$e)))) = (1, (2, (3, (4, 5))));", "5")]
    [DataRow("(var $$a, var (b, c))= (a: 1, (b: (byte)2, 3));", "1")]
    [DataRow("(var a, $$var (b, c))= (a: 1, (b: (byte)2, 3));", "(b: (byte)2, 3)")]
    [DataRow("(var a, var ($$b, c))= (a: 1, (b: (byte)2, 3));", "(byte)2")]
    [DataRow("(var a, var (b, $$c))= (a: 1, (b: (byte)2, 3));", "3")]
    // Assignment to tuple variable.
    [DataRow("(int, int) t; t = (1, $$2);", null)]
    [DataRow("(int, int) t; (var a, t) = (1, ($$2, 3));", null)]
    [DataRow("(int, int) t; (var a, t) = (1, $$(2, 3));", "t")]
    // Start node is right side of assignment
    [DataRow("var (a, b) = $$(1, 2);", "var (a, b)")]
    [DataRow("var t = $$(1, 2);", null)] // Not an assignment
    [DataRow("(int, int) t; t = $$(1, 2);", "t")]
    [DataRow("(int, int) t; t = ($$1, 2);", null)]
    [DataRow("int a; a = $$1;", "a")]
    // Start node is left side of assignment
    [DataRow("var (a, b)$$ = (1, 2);", "(1, 2)")]
    [DataRow("$$var t = (1, 2);", null)] // Not an assignment
    [DataRow("(int, int) t; $$t = (1, 2);", "(1, 2)")]
    [DataRow("int a; $$a = 1;", "1")]
    public void FindAssignmentComplement_Tests(string code, string expectedNode)
    {
        code = $@"
public class C
{{
    public void M()
    {{
        var x = 0;
        {code}
    }}
}}";
        var nodePosition = code.IndexOf("$$");
        code = code.Replace("$$", string.Empty);
        var tree = CSharpSyntaxTree.ParseText(code);
        var argument = tree.GetRoot().FindNode(new TextSpan(nodePosition, 0));
        argument = argument.AncestorsAndSelf()
            .FirstOrDefault(x => x?.Kind() is
                SyntaxKind.Argument or
                SyntaxKindEx.DiscardDesignation or
                SyntaxKindEx.SingleVariableDesignation or
                SyntaxKindEx.ParenthesizedVariableDesignation or
                SyntaxKindEx.TupleExpression)
            ?? argument;
        tree.GetDiagnostics().Should().BeEmpty();
        var target = SyntaxNodeExtensionsCSharp.FindAssignmentComplement(argument);
        if (expectedNode is null)
        {
            target.Should().BeNull();
        }
        else
        {
            target.Should().NotBeNull();
            target.ToString().Should().Be(expectedNode);
        }
    }

    [TestMethod]
    public void IsInExpressionTree_CS()
    {
        const string code = @"
using System.Linq;
public class Sample
{
    public void Main(int[] arr)
    {
            var withNormalLambda = arr.Where(xNormal => xNormal == 42).OrderBy((xNormal) => xNormal).Select(xNormal => xNormal.ToString());
            var withNormal = from xNormal in arr where xNormal == 42 orderby xNormal select xNormal.ToString();

            var withExpressionLambda = arr.AsQueryable().Where(xExpres => xExpres == 42).OrderBy((xExpres) => xExpres).Select(xExpres => xExpres.ToString());
            var withExpression = from xExpres in arr.AsQueryable() where xExpres == 42 orderby xExpres select xExpres.ToString();
    }
}";
        var (tree, model) = TestCompiler.CompileCS(code);
        var allIdentifiers = tree.GetRoot().DescendantNodes().OfType<SyntaxCS.IdentifierNameSyntax>().ToArray();
        allIdentifiers.Where(x => x.Identifier.ValueText == "xNormal").Should().HaveCount(6).And.OnlyContain(x => !SyntaxNodeExtensionsCSharp.IsInExpressionTree(x, model));
        allIdentifiers.Where(x => x.Identifier.ValueText == "xExpres").Should().HaveCount(6).And.OnlyContain(x => SyntaxNodeExtensionsCSharp.IsInExpressionTree(x, model));
    }

    [TestMethod]
    public void IsInExpressionTree_VB()
    {
        const string code = @"
Public Class Sample
    Public Sub Main(Arr() As Integer)
        Dim WithNormalLambda = Arr.Where(Function(xNormal) xNormal = 42).OrderBy(Function(xNormal) xNormal).Select(Function(xNormal) xNormal.ToString())
        Dim WithNormal = From xNormal In Arr Where xNormal = 42 Order By xNormal Select Result = xNormal.ToString()

        Dim WithExpressionLambda = Arr.AsQueryable().Where(Function(xExpres) xExpres = 42).OrderBy(Function(xExpres) xExpres).Select(Function(xExpres) xExpres.ToString())
        Dim WithExpression = From xExpres In Arr.AsQueryable() Where xExpres = 42 Order By xExpres Select Result = xExpres.ToString()
    End Sub
End Class";
        var (tree, model) = TestCompiler.CompileVB(code);
        var allIdentifiers = tree.GetRoot().DescendantNodes().OfType<SyntaxVB.IdentifierNameSyntax>().ToArray();
        allIdentifiers.Where(x => x.Identifier.ValueText == "xNormal").Should().HaveCount(6).And.OnlyContain(x => !SyntaxNodeExtensionsVisualBasic.IsInExpressionTree(x, model));
        allIdentifiers.Where(x => x.Identifier.ValueText == "xExpres").Should().HaveCount(6).And.OnlyContain(x => SyntaxNodeExtensionsVisualBasic.IsInExpressionTree(x, model));
    }

    [TestMethod]
    [DataRow("A?.$$M()", "A?.M()", "A")]
    [DataRow("A?.B?.$$M()", ".B?.M()", ".B")]
    [DataRow("A?.M()?.$$B", ".M()?.B", ".M()")]
    [DataRow("A?.$$M()?.B", "A?.M()?.B", "A")]
    [DataRow("A[0]?.M()?.$$B", ".M()?.B", ".M()")]
    [DataRow("A[0]?.M().B?.$$C", ".M().B?.C", ".M().B")]
    [DataRow("A[0]?.$$M().B?.C", "A[0]?.M().B?.C", "A[0]")]
    [DataRow("A?.$$B.C", "A?.B.C", "A")]
    [DataRow("A?.$$B?.C", "A?.B?.C", "A")]
    public void GetParentConditionalAccessExpression_CS(string expression, string parent, string parentExpression)
    {
        var code = $$"""
            public class X
            {
                public X A { get; }
                public X B { get; }
                public X C { get; }
                public X this[int i] => null;
                public X M()
                {
                    var _ = {{expression}};
                    return null;
                }
            }
            """;
        var node = TestCompiler.NodeBetweenMarkersCS(code).Node;
        var parentConditional = MicrosoftExtensionsCS.GetParentConditionalAccessExpression(node);
        parentConditional.ToString().Should().Be(parent);
        parentConditional.Expression.ToString().Should().Be(parentExpression);
    }

    [TestMethod]
    [DataRow("A?.$$M()", "A?.M()", "A")]
    [DataRow("A?.B?.$$M()", ".B?.M()", ".B")]
    [DataRow("A?.M()?.$$B", ".M()?.B", ".M()")]
    [DataRow("A?.$$M()?.B", "A?.M()?.B", "A")]
    [DataRow("A(0)?.M()?.$$B", ".M()?.B", ".M()")]
    [DataRow("A(0)?.M().B?.$$C", ".M().B?.C", ".M().B")]
    [DataRow("A(0)?.$$M().B?.C", "A(0)?.M().B?.C", "A(0)")]
    [DataRow("A?.$$B.C", "A?.B.C", "A")]
    [DataRow("A?.$$B?.C", "A?.B?.C", "A")]
    public void GetParentConditionalAccessExpression_VB(string expression, string parent, string parentExpression)
    {
        var code = $$"""
            Public Class X
                Public ReadOnly Property A As X
                Public ReadOnly Property B As X
                Public ReadOnly Property C As X

                Default Public ReadOnly Property Item(i As Integer) As X
                    Get
                        Return Nothing
                    End Get
                End Property

                Public Function M() As X
                    Dim __ = {{expression}}
                    Return Nothing
                End Function
            End Class
            """;
        var node = TestCompiler.NodeBetweenMarkersVB(code).Node;
        var parentConditional = SyntaxNodeExtensionsVisualBasic.GetParentConditionalAccessExpression(node);
        parentConditional.ToString().Should().Be(parent);
        parentConditional.Expression.ToString().Should().Be(parentExpression);
    }

    [TestMethod]
    [DataRow("A?.$$M()")]
    [DataRow("A?.B?.$$M()")]
    [DataRow("A?.M()?.$$B")]
    [DataRow("A?.$$M()?.B")]
    [DataRow("A[0]?.M()?.$$B")]
    [DataRow("A[0]?.M().B?.$$C")]
    [DataRow("A?.$$B.C")]
    [DataRow("A?.$$B?.C")]
    public void GetRootConditionalAccessExpression_CS(string expression)
    {
        var code = $$"""
            public class X
            {
                public X A { get; }
                public X B { get; }
                public X C { get; }
                public X this[int i] => null;

                public X M()
                {
                    var _ = {{expression}};
                    return null;
                }
            }
            """;
        var node = TestCompiler.NodeBetweenMarkersCS(code).Node;
        var parentConditional = MicrosoftExtensionsCS.GetRootConditionalAccessExpression(node);
        parentConditional.ToString().Should().Be(expression.Replace("$$", string.Empty));
    }

    [TestMethod]
    public void Kind_Null_ReturnsNone() =>
        ExtensionsCore.Kind<SyntaxKind>(null).Should().Be(SyntaxKind.None);

    [TestMethod]
    [DataRow("class Test { }", DisplayName = "When there is no pragma, return default file name.")]
    [DataRow("#pragma checksum \"FileName.txt\" \"{guid}\" \"checksum bytes\"", "FileName.txt", DisplayName = "When pragma is present, return file name from pragma.")]
    public void GetMappedFilePath(string code, string expectedFileName = DefaultFileName)
    {
        var tree = GetSyntaxTree(code, DefaultFileName);
        tree.GetRoot().GetMappedFilePathFromRoot().Should().Be(expectedFileName);
    }

    [TestMethod]
    [DataRow("$$M(1)$$;")]
    [DataRow("_ = $$new C(1)$$;")]
    [DataRow("C c = $$new(1)$$;")]
    public void ArgumentList_CS_InvocationObjectCreation(string statement)
    {
        var code = $$"""
            public class C(int p) {
                public void M(int p) {
                    {{statement}}
                }
            }
            """;
        var node = TestCompiler.NodeBetweenMarkersCS(code).Node;
        var argumentList = SyntaxNodeExtensionsCSharp.ArgumentList(node).Arguments;
        var argument = argumentList.Should().ContainSingle().Which;
        (argument is { Expression: SyntaxCS.LiteralExpressionSyntax { Token.ValueText: "1" } }).Should().BeTrue();
    }

    [TestMethod]
    [DataRow("base")]
    [DataRow("this")]
    public void ArgumentList_CS_ConstructorInitializer(string keyword)
    {
        var code = $$"""
            public class Base(int p);

            public class C: Base
            {
                public C(): $${{keyword}}(1)$$ { }
                public C(int  p): base(p) { }
            }

            """;
        var node = TestCompiler.NodeBetweenMarkersCS(code).Node;
        var argumentList = SyntaxNodeExtensionsCSharp.ArgumentList(node).Arguments;
        var argument = argumentList.Should().ContainSingle().Which;
        (argument is { Expression: SyntaxCS.LiteralExpressionSyntax { Token.ValueText: "1" } }).Should().BeTrue();
    }

    [TestMethod]
    public void ArgumentList_CS_PrimaryConstructorBaseType()
    {
        var code = """
            public class Base(int p);
            public class Derived(int p): $$Base(1)$$;
            """;
        var node = TestCompiler.NodeBetweenMarkersCS(code).Node;
        var argumentList = SyntaxNodeExtensionsCSharp.ArgumentList(node).Arguments;
        var argument = argumentList.Should().ContainSingle().Which;
        (argument is { Expression: SyntaxCS.LiteralExpressionSyntax { Token.ValueText: "1" } }).Should().BeTrue();
    }

    [TestMethod]
    [DataRow("_ = $$new System.Collections.Generic.List<int> { 0 }$$;")]
    public void ArgumentList_CS_NoList(string statement)
    {
        var code = $$"""
            public class C {
                public void M() {
                    {{statement}}
                }
            }
            """;
        var node = TestCompiler.NodeBetweenMarkersCS(code).Node;
        SyntaxNodeExtensionsCSharp.ArgumentList(node).Should().BeNull();
    }

    [TestMethod]
    public void ArgumentList_CS_Null() =>
        SyntaxNodeExtensionsCSharp.ArgumentList(null).Should().BeNull();

    [TestMethod]
    [DataRow("_ = $$new int[] { 1 }$$;")]
    [DataRow("_ = $$new { A = 1 }$$;")]
    public void ArgumentList_CS_UnsupportedNodeKinds(string statement)
    {
        var code = $$"""
            public class C {
                public void M() {
                    {{statement}}
                }
            }
            """;
        var node = TestCompiler.NodeBetweenMarkersCS(code).Node;
        var sut = () => SyntaxNodeExtensionsCSharp.ArgumentList(node);
        sut.Should().Throw<InvalidOperationException>();
    }

    [TestMethod]
    [DataRow("$$M(1)$$")]
    [DataRow("Call $$M(1)$$")]
    [DataRow("Dim c = $$New C(1)$$")]
    [DataRow("$$RaiseEvent SomeEvent(1)$$")]
    public void ArgumentList_VB_Invocations(string statement)
    {
        var code = $$"""
            Imports System

            Public Class C
                Public Event SomeEvent As Action(Of Integer)

                Public Sub New(p As Integer)
                End Sub

                Public Sub M(p As Integer)
                    Dim s As String = "Test"
                    {{statement}}
                End Sub
            End Class
            """;
        var node = TestCompiler.NodeBetweenMarkersVB(code, getInnermostNodeForTie: true).Node;
        var argumentList = SyntaxNodeExtensionsVisualBasic.ArgumentList(node);
        var argument = argumentList.Arguments.Should().ContainSingle().Which;
        (argument.GetExpression() is SyntaxVB.LiteralExpressionSyntax { Token.ValueText: "1" }).Should().BeTrue();
    }

    [TestMethod]
    public void ArgumentList_VB_Mid()
    {
        var code = $$"""
            Public Class C
                Public Sub M()
                    Dim s As String = "Test"
                    $$Mid(s, 1)$$ = "Test"
                End Sub
            End Class
            """;
        var node = TestCompiler.NodeBetweenMarkersVB(code, getInnermostNodeForTie: true).Node;
        var argumentList = SyntaxNodeExtensionsVisualBasic.ArgumentList(node);
        argumentList.Arguments.Should().SatisfyRespectively(
            x => (x.GetExpression() is SyntaxVB.IdentifierNameSyntax { Identifier.ValueText: "s" }).Should().BeTrue(),
            x => (x.GetExpression() is SyntaxVB.LiteralExpressionSyntax { Token.ValueText: "1" }).Should().BeTrue());
    }

    [TestMethod]
    public void ArgumentList_VB_Attribute()
    {
        var code = """
            <$$System.Obsolete("1")$$>
            Public Class C
            End Class
            """;
        var node = TestCompiler.NodeBetweenMarkersVB(code, getInnermostNodeForTie: true).Node;
        var argumentList = SyntaxNodeExtensionsVisualBasic.ArgumentList(node);
        var argument = argumentList.Arguments.Should().ContainSingle().Which;
        (argument.GetExpression() is SyntaxVB.LiteralExpressionSyntax { Token.ValueText: "1" }).Should().BeTrue();
    }

    [TestMethod]
    [DataRow("Dim $$i(1)$$ As Integer")]
    [DataRow("Dim sales()() As Double = $$New Double(1)() { }$$")]
    [DataRow("ReDim $$arr(1)$$")]
    public void ArgumentList_VB_ArrayBounds(string statement)
    {
        var code = $$"""
            Public Class C
                Public Sub M()
                    Dim arr(0) As Integer
                    {{statement}}
                End Sub
            End Class
            """;
        var node = TestCompiler.NodeBetweenMarkersVB(code, getInnermostNodeForTie: true).Node;
        var argumentList = SyntaxNodeExtensionsVisualBasic.ArgumentList(node);
        var argument = argumentList.Arguments.Should().ContainSingle().Which;
        (argument.GetExpression() is SyntaxVB.LiteralExpressionSyntax { Token.ValueText: "1" }).Should().BeTrue();
    }

    [TestMethod]
    public void ArgumentList_VB_Call()
    {
        var code = $$"""
            Public Class C
                Public Sub M()
                    Call $$M$$
                End Sub
            End Class
            """;
        var node = TestCompiler.NodeBetweenMarkersVB(code, getInnermostNodeForTie: false).Node;
        SyntaxNodeExtensionsVisualBasic.ArgumentList(node).Should().BeNull();
    }

    [TestMethod]
    public void ArgumentList_VB_Null() =>
        SyntaxNodeExtensionsVisualBasic.ArgumentList(null).Should().BeNull();

    [TestMethod]
    [DataRow("$$Dim a = 1$$")]
    public void ArgumentList_VB_UnsupportedNodeKinds(string statement)
    {
        var code = $$"""
            Public Class C
                Public Sub M()
                    {{statement}}
                End Sub
            End Class
            """;
        var node = TestCompiler.NodeBetweenMarkersVB(code, getInnermostNodeForTie: true).Node;
        var sut = () => SyntaxNodeExtensionsVisualBasic.ArgumentList(node);
        sut.Should().Throw<InvalidOperationException>();
    }

    [TestMethod]
    [DataRow("""public C(int p) { }""")]
    [DataRow("""public void M(int p) { }""")]
    [DataRow("""public static C operator + (C p) => default;""")]
    [DataRow("""public static implicit operator C (int p) => default;""")]
    [DataRow("""public delegate void M(int p);""")]
    public void ParameterList_Methods(string declarations)
    {
        var node = TestCompiler.NodeBetweenMarkersCS($$"""
            public class C
            {
                $${{declarations}}$$
            }
            """).Node;
        var actual = SyntaxNodeExtensionsCSharp.ParameterList(node);
        actual.Should().NotBeNull();
        var entry = actual.Parameters.Should().ContainSingle().Which;
        entry.Identifier.ValueText.Should().Be("p");
    }

    [TestMethod]
    [DataRow("""$$void Local(int p) { }$$""")]
    [DataRow("""$$static void Local(int p) { }$$""")]
    [DataRow("""Func<int, int> f = $$(int p) => 0$$;""")]
    [DataRow("""Func<int, int> f = $$delegate (int p) { return 0; }$$;""")]
    public void ParameterList_NestedMethods(string declarations)
    {
        var node = TestCompiler.NodeBetweenMarkersCS($$"""
            using System;

            public class C
            {
                public void M()
                {
                    {{declarations}}
                }
            }
            """).Node;
        var actual = SyntaxNodeExtensionsCSharp.ParameterList(node);
        actual.Should().NotBeNull();
        var entry = actual.Parameters.Should().ContainSingle().Which;
        entry.Identifier.ValueText.Should().Be("p");
    }

    [TestMethod]
    public void ParameterList_Destructor()
    {
        var node = TestCompiler.NodeBetweenMarkersCS("""
            public class C
            {
                $$~C() { }$$
            }
            """).Node;
        var actual = SyntaxNodeExtensionsCSharp.ParameterList(node);
        actual.Should().NotBeNull();
        actual.Parameters.Should().BeEmpty();
    }

    [TestMethod]
    [DataRow("class")]
    [DataRow("struct")]
    [DataRow("record struct")]
    [DataRow("readonly struct")]
#if NET
    [DataRow("readonly record struct")]
    [DataRow("record")]
    [DataRow("record class")]
#endif
    public void ParameterList_PrimaryConstructors(string type)
    {
        var node = TestCompiler.NodeBetweenMarkersCS($$"""
            $$public {{type}} C(int p)
            {

            }$$
            """).Node;
        var actual = SyntaxNodeExtensionsCSharp.ParameterList(node);
        actual.Should().NotBeNull();
        var entry = actual.Parameters.Should().ContainSingle().Which;
        entry.Identifier.ValueText.Should().Be("p");
    }

    [TestMethod]
    [DataRow("$$int i;$$")]
    [DataRow("$$class Nested { }$$")]
    public void ParameterList_ReturnsNull(string declaration)
    {
        var node = TestCompiler.NodeBetweenMarkersCS($$"""
            using System.Collections.Generic;
            public class C
            {
                {{declaration}}
            }
            """).Node;
        var actual = SyntaxNodeExtensionsCSharp.ParameterList(node);
        actual.Should().BeNull();
    }

    [TestMethod]
    [DataRow("""$$global::System$$.Int32 i;""", "global")]                        // AliasQualifiedNameSyntax
    [DataRow("""int i = Math.Abs($$1$$);""", null)]                               // ArgumentSyntax
    [DataRow("""int i = Math.Abs($$value: 1$$);""", "value")]                     // ArgumentSyntax
    [DataRow("""$$int[]$$ i;""", "int")]                                          // ArrayTypeSyntax
    [DataRow("""$$int[][]$$ i;""", "int")]                                        // ArrayTypeSyntax
    [DataRow("""[DebuggerDisplay($$""$$)]int i;""", null)]                        // AttributeArgumentSyntax
    [DataRow("""[DebuggerDisplay($$value: ""$$)]int i;""", "value")]              // AttributeArgumentSyntax
    [DataRow("""[DebuggerDisplay("", $$Name = ""$$)]int i;""", "Name")]           // AttributeArgumentSyntax
    [DataRow("""[$$DebuggerDisplay("")$$]int i;""", "DebuggerDisplay")]           // AttributeSyntax
    [DataRow("""class $$T$$ { }""", "T")]                                         // BaseTypeDeclarationSyntax
    [DataRow("""struct $$T$$ { }""", "T")]                                        // BaseTypeDeclarationSyntax
    [DataRow("""interface $$T$$ { }""", "T")]                                     // BaseTypeDeclarationSyntax
    [DataRow("""record $$T$$ { }""", "T")]                                        // BaseTypeDeclarationSyntax
    [DataRow("""record class $$T$$ { }""", "T")]                                  // BaseTypeDeclarationSyntax
    [DataRow("""record struct $$T$$ { }""", "T")]                                 // BaseTypeDeclarationSyntax
    [DataRow("""enum $$T$$ { }""", "T")]                                          // BaseTypeDeclarationSyntax
    [DataRow("""void M(string s) { var x = $$s?.Length$$; }""", "Length")]                                      // ConditionalAccessExpressionSyntax
    [DataRow("""void M(string s) { $$s?.ToLower()?.ToUpper()$$; }""", "ToUpper")]                               // ConditionalAccessExpressionSyntax
    [DataRow("""void M(string s) { $$s.ToLower()?.ToUpper()$$; }""", "ToUpper")]                                // ConditionalAccessExpressionSyntax + MemberAccessExpressionSyntax
    [DataRow("""void M(string s) { $$s?.ToLower().ToUpper()$$; }""", "ToUpper")]                                // ConditionalAccessExpressionSyntax + MemberAccessExpressionSyntax
    [DataRow("""void M(string s) { $$s?.ToLower().ToUpper()?.PadLeft(42).Normalize()$$; }""", "Normalize")]     // ConditionalAccessExpressionSyntax + MemberAccessExpressionSyntax
    [DataRow("""void M(string s) { $$s.ToLower().ToUpper().PadLeft(42).Normalize()$$; }""", "Normalize")]       // MemberAccessExpressionSyntax
    [DataRow("""void M(string s) { $$s.ToLower().ToUpper()$$.PadLeft(42).Normalize(); }""", "ToUpper")]         // MemberAccessExpressionSyntax
    [DataRow("""void M(string s) { $$s.ToLower().ToUpper()$$?.PadLeft(42).Normalize(); }""", "ToUpper")]        // ConditionalAccessExpressionSyntax + MemberAccessExpressionSyntax
    [DataRow("""void M(string s) { s.ToLower()?.$$ToUpper()?.PadLeft(42).Normalize()$$; }""", "Normalize")]     // MemberAccessExpressionSyntax
    [DataRow("""$$Test() { }$$""", "Test")]                                       // ConstructorDeclarationSyntax
    [DataRow("""Test() : $$this(1)$$ { }""", "this")]                             // ConstructorInitializerSyntax
    [DataRow("""Test() : $$base()$$ { }""", "base")]                              // ConstructorInitializerSyntax
    [DataRow("""$$public static implicit operator int(Test t) => 1;$$""", "int")] // ConversionOperatorDeclarationSyntax
    [DataRow("""$$delegate void D();$$""", "D")]                                  // DelegateDeclarationSyntax
    [DataRow("""$$~Test() { }$$""", "Test")]                                      // DestructorDeclarationSyntax
    [DataRow("""enum E { $$M$$ }""", "M")]                                        // EnumMemberDeclarationSyntax
    [DataRow("""enum E { $$M = 1$$, }""", "M")]                                   // EnumMemberDeclarationSyntax
    [DataRow("""$$event Action E {add { } remove { } }$$""", "E")]                // EventDeclarationSyntax
    [DataRow("""$$event Action E;$$""", null)]                                    // EventFieldDeclarationSyntax
    [DataRow("""$$event Action E1, E2;$$""", null)]                               // EventFieldDeclarationSyntax
    [DataRow("""$$Int32$$ i;""", "Int32")]                                        // IdentifierNameSyntax
    [DataRow("""$$int this[int i] { get => 1; set { } }$$""", "this")]            // IndexerDeclarationSyntax
    [DataRow("""int i = $$Math.Abs(1)$$;""", "Abs")]                              // InvocationExpressionSyntax
    [DataRow("""int i = $$Fun()()$$;""", null)]                                   // InvocationExpressionSyntax
    [DataRow("""int i = $$int.MaxValue$$;""", "MaxValue")]                        // MemberAccessExpressionSyntax
    [DataRow("""string s = (new object())?$$.ToString$$();""", "ToString")]       // MemberBindingExpressionSyntax
    [DataRow("""string s = (new object())?$$.ToString()$$;""", "ToString")]       // MemberBindingExpressionSyntax
    [DataRow("""$$void M() { }$$""", "M")]                                        // MethodDeclarationSyntax
    [DataRow("""$$int?$$ i;""", "int")]                                           // NullableTypeSyntax
    [DataRow("""object o = $$new object()$$;""", "object")]                       // ObjectCreationExpressionSyntax
    [DataRow("""$$public static Test operator +(Test t) => default;$$""", "+")]   // OperatorDeclarationSyntax
    [DataRow("""void M($$int i$$) { }""", "i")]                                   // ParameterSyntax
    [DataRow("""int i = $$(int.MaxValue)$$;""", "MaxValue")]                      // ParenthesizedExpressionSyntax
    [DataRow("""$$int P { get; }$$""", "P")]                                      // PropertyDeclarationSyntax
    [DataRow("""int i = $$-int.MaxValue$$;""", "MaxValue")]                       // PrefixUnaryExpressionSyntax
    [DataRow("""object o = $$new object()!$$;""", "object")]                      // PostfixUnaryExpressionSyntax
    [DataRow("""$$int*$$ i;""", "int")]                                           // PointerTypeSyntax
    [DataRow("""$$System.Collections.ArrayList$$ l;""", "ArrayList")]             // QualifiedNameSyntax
    [DataRow("""$$List<int>$$ l;""", "List")]                                     // SimpleNameSyntax
    [DataRow("""bool M() => (0, Name: "a") is (_, $$Name: null$$);""", "Name")]   // SubpatternSyntax
    [DataRow("""bool M() => (0, Name: "a") is ($$_$$, Name: null);""", null)]     // SubpatternSyntax
    [DataRow("""void M<T>() where $$T : class$$ { }""", "T")]                     // TypeParameterConstraintClauseSyntax
    [DataRow("""void M<$$T$$>() { }""", "T")]                                     // TypeParameterSyntax
    [DataRow("""int $$i$$;""", "i")]                                              // VariableDeclaratorSyntax
    [DataRow("""object o = $$new()$$;""", "new")]                                 // ImplicitObjectCreationExpressionSyntax
    [DataRow("""void M(int p) { $$ref int$$ i = ref p; }""", "int")]              // RefTypeSyntax
    public void GetIdentifier_Members(string member, string expected)
    {
        var node = TestCompiler.NodeBetweenMarkersCS($$"""
            using System;
            using System.Collections.Generic;
            using System.Diagnostics;
            unsafe class Test
            {
                static Func<int> Fun() => default;
                public Test(int i) { }
                {{member}}
            }
            """).Node;
        var actual = SyntaxNodeExtensionsCSharp.GetIdentifier(node);
        if (expected is null)
        {
            actual.Should().BeNull();
        }
        else
        {
            actual.Should().NotBeNull();
            actual.Value.ValueText.Should().Be(expected);
        }
    }

    [TestMethod]
    [DataRow("""$$namespace A.B.C { }$$""", "C")]           // NamespaceDeclarationSyntax
    [DataRow("""$$namespace A.B.C;$$""", "C")]              // FileScopedNamespaceDeclarationSyntax
    [DataRow("""$$using A = System.Collections;$$""", "A")] // UsingDirectiveSyntax
    [DataRow("""$$using System.Collections;$$""", null)]    // UsingDirectiveSyntax
    public void GetIdentifier_CompilationUnit(string member, string expected)
    {
        var node = TestCompiler.NodeBetweenMarkersCS($$"""
            {{member}}
            """).Node;
        var actual = SyntaxNodeExtensionsCSharp.GetIdentifier(node);
        if (expected is null)
        {
            actual.Should().BeNull();
        }
        else
        {
            actual.Should().NotBeNull();
            actual.Value.ValueText.Should().Be(expected);
        }
    }

    [TestMethod]
    [DataRow(""" : $$Base(i)$$""", "Base")]       // NamespaceDeclarationSyntax
    [DataRow(""" : $$Test.Base(i)$$""", "Base")]  // NamespaceDeclarationSyntax
    public void GetIdentifier_PrimaryConstructor(string baseType, string expected)
    {
        var node = TestCompiler.NodeBetweenMarkersCS($$"""
            namespace Test;
            public class Base(int i)
            {
            }
            public class Derived(int i) {{baseType}} { }
            """).Node;
        var actual = SyntaxNodeExtensionsCSharp.GetIdentifier(node);
        if (expected is null)
        {
            actual.Should().BeNull();
        }
        else
        {
            actual.Should().NotBeNull();
            actual.Value.ValueText.Should().Be(expected);
        }
    }

    [TestMethod]
    [DataRow("""event EventHandler SomeEvent { add { $$int x = 42;$$ } remove { int x = 42; } }""", SyntaxKind.AddAccessorDeclaration)]
    [DataRow("""int Method() { Func<int, int, int> add = delegate (int a, int b) { return $$a + b$$; }; return add(1, 2); }""", SyntaxKind.AnonymousMethodExpression)]
    [DataRow("""Derived(int arg) : base($$arg$$) { }""", SyntaxKind.BaseConstructorInitializer)]
    [DataRow("""Derived() { $$var x = 42;$$ }""", SyntaxKind.ConstructorDeclaration)]
    [DataRow("""public static implicit operator int(Derived d) => $$42$$;""", SyntaxKind.ConversionOperatorDeclaration)]
    [DataRow("""~Derived() { $$var x = 42;$$ }""", SyntaxKind.DestructorDeclaration)]
    [DataRow("""int field = $$int.Parse("42")$$;""", SyntaxKind.FieldDeclaration)]
    [DataRow("""int Property { get; set; } = $$int.Parse("42")$$;""", SyntaxKind.PropertyDeclaration)]
    [DataRow("""int Property { set { $$_ = value;$$ } }""", SyntaxKind.SetAccessorDeclaration)]
    [DataRow("""int Property { set { $$_ = value;$$ } }""", SyntaxKind.SetAccessorDeclaration)]
    [DataRow("""int Method() { return LocalFunction(); int LocalFunction() { $$return 42;$$ } }""", SyntaxKindEx.LocalFunctionStatement)]
    [DataRow("""int Method() { return LocalFunction(); int LocalFunction() => $$42$$; }""", SyntaxKindEx.LocalFunctionStatement)]
    [DataRow("""int Method() { $$return 42;$$ }""", SyntaxKind.MethodDeclaration)]
    [DataRow("""int Method() => $$42$$;""", SyntaxKind.MethodDeclaration)]
    [DataRow("""public static Derived operator +(Derived d) => $$d$$;""", SyntaxKind.OperatorDeclaration)]
    [DataRow("""int Method() { var lambda = () => $$42$$; return lambda(); }""", SyntaxKind.ParenthesizedLambdaExpression)]
    [DataRow("""int Method() { Func<int, int> lambda = x => $$x + 1$$; return lambda(42); }""", SyntaxKind.SimpleLambdaExpression)]
    [DataRow("""event EventHandler SomeEvent { add { int x = 42; } remove { $$int x = 42;$$ } }""", SyntaxKind.RemoveAccessorDeclaration)]
    [DataRow("""Derived(int arg) : this($$arg.ToString()$$) { }""", SyntaxKind.ThisConstructorInitializer)]
    [DataRow("""enum E { A = $$1$$ }""", SyntaxKind.EnumMemberDeclaration)]
    [DataRow("""void M(int i = $$1$$) { }""", SyntaxKind.Parameter)]
#if NET
    [DataRow("""int Property { init { $$_ = value;$$ } }""", SyntaxKindEx.InitAccessorDeclaration)]
    [DataRow("""record BaseRec(int I); record DerivedRec(int I): BaseRec($$I++$$);""", SyntaxKindEx.PrimaryConstructorBaseType)]
#endif
    public void EnclosingScope_Members(string member, SyntaxKind expectedSyntaxKind)
    {
        var node = TestCompiler.NodeBetweenMarkersCS($$"""
            using System;

            public class Base
            {
                public Base() { }
                public Base(int arg) { }
            }

            public class Derived: Base
            {
                Derived(string arg) { }
                {{member}}
            }
            """).Node;
        var actual = SyntaxNodeExtensionsCSharp.EnclosingScope(node)?.Kind() ?? SyntaxKind.None;
        actual.Should().Be(expectedSyntaxKind);
    }

    [TestMethod]
    public void EnclosingScope_TopLevelStatements()
    {
        var node = TestCompiler.NodeBetweenMarkersCS($$"""
            using System;

            $$Console.WriteLine("")$$;
            """, outputKind: OutputKind.ConsoleApplication).Node;
        var actual = SyntaxNodeExtensionsCSharp.EnclosingScope(node)?.Kind() ?? SyntaxKind.None;
        actual.Should().Be(SyntaxKind.CompilationUnit);
    }

    [TestMethod]
    [DataRow("from $$x$$ in qry select x", SyntaxKind.MethodDeclaration)] // Wrong. Should be FromClause
    [DataRow("from x in $$qry$$ select x", SyntaxKind.MethodDeclaration)]
    [DataRow("from x in qry from y in $$qry$$ select x", SyntaxKind.MethodDeclaration)] // Wrong. Should be FromClause
    [DataRow("from x in qry select $$x$$", SyntaxKind.SelectClause)]
    [DataRow("from x in qry orderby $$x$$ select x", SyntaxKind.OrderByClause)]
    [DataRow("from x in qry where x == $$string.Empty$$ select x", SyntaxKind.WhereClause)]
    [DataRow("from x in qry let y = $$x$$ select y", SyntaxKind.LetClause)]
    [DataRow("from x in qry join y in qry on $$x$$ equals y select x", SyntaxKind.JoinClause)]
    [DataRow("from x in qry join y in qry on x equals $$y$$ select x", SyntaxKind.JoinClause)]
    [DataRow("from x in qry join y in $$qry$$ on x equals y select x", SyntaxKind.JoinClause)] // Wrong. Should be MethodDeclaration
    [DataRow("from x in qry group x by $$x$$ into g select g", SyntaxKind.GroupClause)]
    [DataRow("from x in qry group $$x$$ by x into g select g", SyntaxKind.GroupClause)] // Wrong. Should be the FromClause
    [DataRow("from x in qry group x by x into $$g$$ select g", SyntaxKind.QueryContinuation)]
    [DataRow("from x in qry select x into $$y$$ select y", SyntaxKind.QueryContinuation)]
    public void EnclosingScope_QueryExpressionSyntax(string qry, SyntaxKind expected)
    {
        var node = TestCompiler.NodeBetweenMarkersCS($$"""
            using System;
            using System.Linq;

            class Test
            {
                public void Query(string[] qry)
                {
                    _ = {{qry}};
                }
            }
            """).Node;
        var actual = SyntaxNodeExtensionsCSharp.EnclosingScope(node)?.Kind();
        actual.Should().Be(expected);
    }

    [TestMethod]
    public void Symbol_IsKnownType()
    {
        var snippet = new SnippetCompiler("""
            using System.Collections.Generic;
            public class Sample
            {
                public void Method<T, V>(List<T> param1, List<int> param2, List<V> param3, IList<int> param4) { }
            }
            """);
        var method = (MethodDeclarationSyntax)snippet.GetMethodDeclaration("Sample.Method");
        ExtensionsCore.IsKnownType(method.ParameterList.Parameters[0].Type, KnownType.System_Collections_Generic_List_T, snippet.SemanticModel).Should().BeTrue();
        ExtensionsCore.IsKnownType(method.ParameterList.Parameters[1].Type, KnownType.System_Collections_Generic_List_T, snippet.SemanticModel).Should().BeTrue();
        ExtensionsCore.IsKnownType(method.ParameterList.Parameters[2].Type, KnownType.System_Collections_Generic_List_T, snippet.SemanticModel).Should().BeTrue();
        ExtensionsCore.IsKnownType(method.ParameterList.Parameters[3].Type, KnownType.System_Collections_Generic_List_T, snippet.SemanticModel).Should().BeFalse();
    }

    [TestMethod]
    public void ToSecondaryLocation_NullMessage()
    {
        var code = "public class $$C$$ {}";
        var node = TestCompiler.NodeBetweenMarkersCS(code).Node;
        var secondaryLocation = node.ToSecondaryLocation(null);
        secondaryLocation.Should().NotBeNull();
        secondaryLocation.Location.Should().Be(node.GetLocation());
        secondaryLocation.Message.Should().BeNull();
    }

    [TestMethod]
    [DataRow(null)]
    [DataRow([])]
    public void ToSecondaryLocation_MessageArgs(string[] messageArgs)
    {
        var code = "public class $$C$$ {}";
        var node = TestCompiler.NodeBetweenMarkersCS(code).Node;
        var secondaryLocation = node.ToSecondaryLocation("Message", messageArgs);
        secondaryLocation.Should().NotBeNull();
        secondaryLocation.Location.Should().Be(node.GetLocation());
        secondaryLocation.Message.Should().Be("Message");
    }

    [TestMethod]
    [DataRow("Message {0}", "42")]
    [DataRow("{1} Message {0} ", "42", "21")]
    public void ToSecondaryLocation_MessageFormat(string format, params string[] messageArgs)
    {
        var code = "public class $$C$$ {}";
        var node = TestCompiler.NodeBetweenMarkersCS(code).Node;
        var secondaryLocation = node.ToSecondaryLocation(format, messageArgs);
        secondaryLocation.Should().NotBeNull();
        secondaryLocation.Location.Should().Be(node.GetLocation());
        secondaryLocation.Message.Should().Be(string.Format(format, messageArgs));
    }

    [TestMethod]
    [DataRow("public void M() { _ = $$42$$; }")]
    [DataRow("public void M() { int Local() => $$42$$; }")]
    [DataRow("public void M() { Func<int> _ = () => $$42$$; }")]
    [DataRow("public string P { get { $$return P$$; } }")]
    [DataRow("public C() { _ = $$42$$; }")]
    [DataRow("private readonly int f = $$42$$;")]
    public void PerformanceSensitiveAttribute(string node)
    {
        var code = $$"""
            using System;

            [AttributeUsage(AttributeTargets.Constructor | AttributeTargets.Method | AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = true, Inherited = false)]
            public sealed class PerformanceSensitiveAttribute(): Attribute;

            public class C
            {
                [PerformanceSensitive]
                {{node}}
            }
            """;
        var nodeAndModel = TestCompiler.NodeBetweenMarkersCS(code);
        nodeAndModel.Node.PerformanceSensitiveAttribute(nodeAndModel.Model).Should().NotBeNull();
    }

    [TestMethod]
    public void PerformanceSensitiveAttribute_NullNode() =>
        SyntaxNodeExtensionsCSharp.PerformanceSensitiveAttribute(null, null).Should().BeNull();

    [TestMethod]
    [DataRow("Func<int> f = [PerformanceSensitive] () => $$42$$;")]
    [DataRow("[PerformanceSensitive] int Local() => $$42$$;")]
    [DataRow("LM([PerformanceSensitive] () => $$42$$); int LM(Func<int> f) => f();")]
    public void PerformanceSensitiveAttribute_AnonymousAndLocalMethods(string node)
    {
        var code = $$"""
            using System;

            [AttributeUsage(AttributeTargets.Constructor | AttributeTargets.Method | AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = true, Inherited = false)]
            public sealed class PerformanceSensitiveAttribute(): Attribute;

            public class C
            {
                public void M()
                {
                    {{node}}
                }
            }
            """;
        var nodeAndModel = TestCompiler.NodeBetweenMarkersCS(code);
        nodeAndModel.Node.PerformanceSensitiveAttribute(nodeAndModel.Model).Should().NotBeNull();
    }

    [TestMethod]
    [DataRow("class Inner { public void M() { _ = $$42$$; } }")]
    [DataRow("interface Inner { void $$M$$(); }")]
    [DataRow("struct Inner { public void M() { _ = $$42$$; } }")]
    [DataRow("enum Inner { Value = $$1$$ }")]
    [DataRow("public event System.Action E = $$null$$;")]
    [DataRow("public delegate void MyDelegate(int $$param$$);")]
    public void PerformanceSensitiveAttribute_InvalidAttributeTarget(string node)
    {
        var code = $$"""
            [System.AttributeUsage(System.AttributeTargets.All, AllowMultiple = true, Inherited = false)]
            public sealed class PerformanceSensitiveAttribute(): System.Attribute;

            public class C
            {
                [PerformanceSensitive]
                {{node}}
            }
            """;
        var nodeAndModel = TestCompiler.NodeBetweenMarkersCS(code);
        nodeAndModel.Node.PerformanceSensitiveAttribute(nodeAndModel.Model).Should().BeNull();
    }

    [TestMethod]
    [DataRow("Func<int, int> f = [PerformanceSensitive] x => $$x$$;")] // error CS8916: Attributes on lambda expressions require a parenthesized parameter list.
    [DataRow("Func<int, int> f = [PerformanceSensitive] delegate(int x) { return $$x$$; };")]
    public void PerformanceSensitiveAttribute_DoNotCompile(string node)
    {
        var code = $$"""
            using System;

            [AttributeUsage(AttributeTargets.All, AllowMultiple = true, Inherited = false)]
            public sealed class PerformanceSensitiveAttribute(): Attribute;

            public class C
            {
                public void M()
                {
                    {{node}}
                }
            }
            """;
        Action action = () => TestCompiler.NodeBetweenMarkersCS(code);
        action.Should().Throw<InvalidOperationException>().WithMessage("Test setup error: test code snippet did not compile. See output window for details.");
    }

    [TestMethod]
    [DataRow("public void M() { _ = $$42$$; }")]
    [DataRow("[Some] public void M() { Func<int> f = () => $$42$$; }")]
    [DataRow("public void M() { int Local() => $$42$$; }")]
    [DataRow("public void M() { LM([Some] () => $$42$$); int LM(Func<int> f) => f(); }")]
    [DataRow("public string P { get { return $$P$$; } }")]
    [DataRow("public C() { _ = $$42$$; }")]
    [DataRow("private readonly int f = $$42$$;")]
    public void PerformanceSensitiveAttribute_NoPerformanceSensitiveAttribute(string node)
    {
        var code = $$"""
            using System;

            [AttributeUsage(AttributeTargets.Constructor | AttributeTargets.Method | AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = true, Inherited = false)]
            public sealed class SomeAttribute(): Attribute;

            public class C
            {
                {{node}}
            }
            """;
        var nodeAndModel = TestCompiler.NodeBetweenMarkersCS(code);
        nodeAndModel.Node.PerformanceSensitiveAttribute(nodeAndModel.Model).Should().BeNull();
    }

    private static SyntaxToken GetFirstTokenOfKind(SyntaxTree tree, SyntaxKind kind) =>
        tree.GetRoot().DescendantTokens().First(x => x.IsKind(kind));

    private static SyntaxTree GetSyntaxTree(string content, string fileName = null) =>
        SolutionBuilder
            .Create()
            .AddProject(AnalyzerLanguage.CSharp)
            .AddSnippet(content, fileName)
            .GetCompilation()
            .SyntaxTrees
            .First();

    private static ControlFlowGraph CreateCfgCS<T>(string code) where T : CSharpSyntaxNode
    {
        var (tree, model) = TestCompiler.CompileCS(code);
        return SyntaxNodeExtensionsCSharp.CreateCfg(tree.Single<T>(), model, default);
    }

    private static ControlFlowGraph CreateCfgVB<T>(string code) where T : Microsoft.CodeAnalysis.VisualBasic.VisualBasicSyntaxNode
    {
        var (tree, model) = TestCompiler.CompileVB(code);
        return SyntaxNodeExtensionsVisualBasic.CreateCfg(tree.Single<T>(), model, default);
    }
}
