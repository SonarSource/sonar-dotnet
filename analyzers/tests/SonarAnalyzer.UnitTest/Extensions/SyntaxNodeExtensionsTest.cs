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

using FluentAssertions.Extensions;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Operations;
using Microsoft.CodeAnalysis.Text;
using StyleCop.Analyzers.Lightup;
using static csharp::SonarAnalyzer.Extensions.SyntaxTokenExtensions;
using ExtensionsCS = csharp::SonarAnalyzer.Extensions.SyntaxNodeExtensions;
using ExtensionsVB = vbnet::SonarAnalyzer.Extensions.SyntaxNodeExtensions;
using SyntaxCS = Microsoft.CodeAnalysis.CSharp.Syntax;
using SyntaxVB = Microsoft.CodeAnalysis.VisualBasic.Syntax;

namespace SonarAnalyzer.UnitTest.Extensions
{
    [TestClass]
    public class SyntaxNodeExtensionsTest
    {
        private const string DefaultFileName = "Test.cs";

        [TestMethod]
        public void GetPreviousStatementsCurrentBlockOfNotAStatement()
        {
            const string code = "int x = 42;";

            var syntaxTree = CSharpSyntaxTree.ParseText(code);
            var compilationUnit = syntaxTree.GetCompilationUnitRoot();

            ExtensionsCS.GetPreviousStatementsCurrentBlock(compilationUnit).Should().BeEmpty();
        }

        [TestMethod]
        public void GetPreviousStatementsCurrentBlockOfFirstStatement()
        {
            const string code = "public void M() { int x = 42; }";

            var syntaxTree = CSharpSyntaxTree.ParseText(code);
            var aToken = GetFirstTokenOfKind(syntaxTree, SyntaxKind.NumericLiteralToken);

            var parent = aToken.GetBindableParent();
            ExtensionsCS.GetPreviousStatementsCurrentBlock(parent).Should().BeEmpty();
        }

        [TestMethod]
        public void GetPreviousStatementsCurrentBlockOfSecondStatement()
        {
            const string code = "public void M() { string s = null; int x = 42; }";

            var syntaxTree = CSharpSyntaxTree.ParseText(code);
            var aToken = GetFirstTokenOfKind(syntaxTree, SyntaxKind.NumericLiteralToken);

            var parent = aToken.GetBindableParent();
            ExtensionsCS.GetPreviousStatementsCurrentBlock(parent).Should().HaveCount(1);
        }

        [TestMethod]
        public void GetPreviousStatementsCurrentBlockRetrievesOnlyForCurrentBlock()
        {
            const string code = "public void M(string y) { string s = null; if (y != null) { int x = 42; } }";

            var syntaxTree = CSharpSyntaxTree.ParseText(code);
            var aToken = GetFirstTokenOfKind(syntaxTree, SyntaxKind.NumericLiteralToken);

            var parent = aToken.GetBindableParent();
            ExtensionsCS.GetPreviousStatementsCurrentBlock(parent).Should().BeEmpty();
        }

        [TestMethod]
        public void ArrowExpressionBody_WithNotExpectedNode_ReturnsNull()
        {
            const string code = "var x = 1;";
            var syntaxTree = CSharpSyntaxTree.ParseText(code);
            ExtensionsCS.ArrowExpressionBody(syntaxTree.GetRoot()).Should().BeNull();
        }

        [TestMethod]
        public void GetDeclarationTypeName_UnknownType() =>
#if DEBUG
            Assert.ThrowsException<System.ArgumentException>(() => ExtensionsCS.GetDeclarationTypeName(SyntaxFactory.Block()), "Unexpected type Block\r\nParameter name: kind");
#else
            ExtensionsCS.GetDeclarationTypeName(SyntaxFactory.Block()).Should().Be("type");
#endif

        [TestMethod]
        public void GetDeclarationTypeName_Class() =>
            ExtensionsCS.GetDeclarationTypeName(SyntaxFactory.ClassDeclaration("MyClass")).Should().Be("class");

        [TestMethod]
        public void GetDeclarationTypeName_Constructor() =>
            ExtensionsCS.GetDeclarationTypeName(SyntaxFactory.ConstructorDeclaration("MyConstructor")).Should().Be("constructor");

        [TestMethod]
        public void GetDeclarationTypeName_Delegate() =>
            ExtensionsCS.GetDeclarationTypeName(SyntaxFactory.DelegateDeclaration(SyntaxFactory.ParseTypeName("void"), "MyDelegate")).Should().Be("delegate");

        [TestMethod]
        public void GetDeclarationTypeName_Destructor() =>
            ExtensionsCS.GetDeclarationTypeName(SyntaxFactory.DestructorDeclaration("~")).Should().Be("destructor");

        [TestMethod]
        public void GetDeclarationTypeName_Enum() =>
            ExtensionsCS.GetDeclarationTypeName(SyntaxFactory.EnumDeclaration("MyEnum")).Should().Be("enum");

        [TestMethod]
        public void GetDeclarationTypeName_EnumMember() =>
            ExtensionsCS.GetDeclarationTypeName(SyntaxFactory.EnumMemberDeclaration("EnumValue1")).Should().Be("enum");

        [TestMethod]
        public void GetDeclarationTypeName_Event() =>
            ExtensionsCS.GetDeclarationTypeName(SyntaxFactory.EventDeclaration(SyntaxFactory.ParseTypeName("void"), "MyEvent")).Should().Be("event");

        [TestMethod]
        public void GetDeclarationTypeName_EventField() =>
            ExtensionsCS.GetDeclarationTypeName(SyntaxFactory.EventFieldDeclaration(SyntaxFactory.VariableDeclaration(SyntaxFactory.ParseTypeName("MyEvent")))).Should().Be("event");

        [TestMethod]
        public void GetDeclarationTypeName_Field() =>
            ExtensionsCS.GetDeclarationTypeName(SyntaxFactory.FieldDeclaration(SyntaxFactory.VariableDeclaration(SyntaxFactory.ParseTypeName("int")))).Should().Be("field");

        [TestMethod]
        public void GetDeclarationTypeName_Indexer() =>
            ExtensionsCS.GetDeclarationTypeName(SyntaxFactory.IndexerDeclaration(SyntaxFactory.ParseTypeName("int"))).Should().Be("indexer");

        [TestMethod]
        public void GetDeclarationTypeName_Interface() =>
            ExtensionsCS.GetDeclarationTypeName(SyntaxFactory.InterfaceDeclaration("MyInterface")).Should().Be("interface");

        [TestMethod]
        public void GetDeclarationTypeName_LocalFunction() =>
            ExtensionsCS.GetDeclarationTypeName(SyntaxFactory.LocalFunctionStatement(SyntaxFactory.ParseTypeName("void"), "MyLocalFunction")).Should().Be("local function");

        [TestMethod]
        public void GetDeclarationTypeName_Method() =>
            ExtensionsCS.GetDeclarationTypeName(SyntaxFactory.MethodDeclaration(SyntaxFactory.ParseTypeName("void"), "MyMethod")).Should().Be("method");

        [TestMethod]
        public void GetDeclarationTypeName_Property() =>
            ExtensionsCS.GetDeclarationTypeName(SyntaxFactory.PropertyDeclaration(SyntaxFactory.ParseTypeName("void"), "MyProperty")).Should().Be("property");

        [TestMethod]
        public void GetDeclarationTypeName_Record() =>
            ExtensionsCS.GetDeclarationTypeName(SyntaxFactory.RecordDeclaration(SyntaxFactory.Token(SyntaxKind.RecordKeyword), "MyRecord")).Should().Be("record");

        [TestMethod]
        public void GetDeclarationTypeName_RecordStruct() =>
            ExtensionsCS.GetDeclarationTypeName(SyntaxFactory.RecordDeclaration(SyntaxKind.RecordStructDeclaration, SyntaxFactory.Token(SyntaxKind.RecordKeyword), "MyRecord"))
                .Should().Be("record struct");

        [TestMethod]
        public void GetDeclarationTypeName_Struct() =>
            ExtensionsCS.GetDeclarationTypeName(SyntaxFactory.StructDeclaration("MyStruct")).Should().Be("struct");

        [TestMethod]
        public void CreateCfg_MethodBody_ReturnsCfg_CS()
        {
            const string code = @"
public class Sample
{
    public void Main()
    {
        var x = 42;
    }
}";
            var (tree, semanticModel) = TestHelper.CompileCS(code);
            var node = tree.Single<SyntaxCS.MethodDeclarationSyntax>();

            ExtensionsCS.CreateCfg(node.Body, semanticModel, default).Should().NotBeNull();
        }

        [TestMethod]
        public void CreateCfg_MethodBlock_ReturnsCfg_VB()
        {
            const string code = @"
Public Class Sample
    Public Sub Main()
        Dim X As Integer = 42
    End Sub
End Class";
            var (tree, semanticModel) = TestHelper.CompileVB(code);
            var node = tree.Single<SyntaxVB.MethodBlockSyntax>();

            ExtensionsVB.CreateCfg(node, semanticModel, default).Should().NotBeNull();
        }

        [TestMethod]
        public void CreateCfg_AnyNode_ReturnsCfg_CS()
        {
            const string code = @"
public class Sample
{
    public void Main()
    {
        Main();
    }
}";
            var (tree, semanticModel) = TestHelper.CompileCS(code);
            var node = tree.Single<SyntaxCS.InvocationExpressionSyntax>();

            ExtensionsCS.CreateCfg(node, semanticModel, default).Should().NotBeNull();
        }

        [TestMethod]
        public void CreateCfg_AnyNode_ReturnsCfg_VB()
        {
            const string code = @"
Public Class Sample
    Public Sub Main()
        Main()
    End Sub
End Class";
            var (tree, semanticModel) = TestHelper.CompileVB(code);
            var node = tree.Single<SyntaxVB.InvocationExpressionSyntax>();

            ExtensionsVB.CreateCfg(node, semanticModel, default).Should().NotBeNull();
        }

        [TestMethod]
        public void CreateCfg_LambdaInsideQuery_CS()
        {
            const string code = @"
using System;
using System.Linq;
public class Sample
{
    public void Main(int[] values)
    {
        var result = from value in values select new Lazy<int>(() => value);
    }
}";
            var (tree, semanticModel) = TestHelper.CompileCS(code);
            var lambda = tree.Single<SyntaxCS.ParenthesizedLambdaExpressionSyntax>();

            ExtensionsCS.CreateCfg(lambda.Body, semanticModel, default).Should().NotBeNull();
        }

        [TestMethod]
        public void CreateCfg_LambdaInsideQuery_VB()
        {
            const string code = @"
Public Class Sample
    Public Sub Main(Values() As Integer)
        Dim Result As IEnumerable(Of Lazy(Of Integer)) = From Value In Values Select New Lazy(Of Integer)(Function() Value)
    End Sub
End Class
";
            var (tree, semanticModel) = TestHelper.CompileVB(code);
            var lambda = tree.Single<SyntaxVB.SingleLineLambdaExpressionSyntax>();

            ExtensionsVB.CreateCfg(lambda.Body, semanticModel, default).Should().NotBeNull();
        }

        [TestMethod]
        public void CreateCfg_NestingChain_CS()
        {
            const string code = @"
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
}";
            var (tree, semanticModel) = TestHelper.CompileCS(code);
            var innerLambda = tree.Single<SyntaxCS.SimpleLambdaExpressionSyntax>();
            innerLambda.Parent.Parent.Should().BeOfType<SyntaxCS.VariableDeclaratorSyntax>().Subject.Identifier.ValueText.Should().Be("innerLambda");

            var cfg = ExtensionsCS.CreateCfg(innerLambda.Body, semanticModel, default);
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
            const string code = @"
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
End Class";
            var (tree, semanticModel) = TestHelper.CompileVB(code);
            var innerSub = tree.Single<SyntaxVB.InvocationExpressionSyntax>().FirstAncestorOrSelf<SyntaxVB.SingleLineLambdaExpressionSyntax>();
            innerSub.Parent.Parent.Should().BeOfType<SyntaxVB.VariableDeclaratorSyntax>().Subject.Names.Single().Identifier.ValueText.Should().Be("InnerSingleLineSub");

            var cfg = ExtensionsVB.CreateCfg(innerSub, semanticModel, default);
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
            const string code = @"
public class Sample
{
    public void Main()
    {
        Undefined(() => 45);
    }
}";
            var (tree, model) = TestHelper.CompileIgnoreErrorsCS(code);
            var lambda = tree.Single<SyntaxCS.ParenthesizedLambdaExpressionSyntax>();
            ExtensionsCS.CreateCfg(lambda.Body, model, default).Should().NotBeNull();
        }

        [TestMethod]
        public void CreateCfg_UndefinedSymbol_ReturnsNull_VB()
        {
            const string code = @"
Public Class Sample
    Public Sub Main()
        Undefined(Function() 42)
    End Sub
End Class";
            var (tree, model) = TestHelper.CompileIgnoreErrorsVB(code);
            var lambda = tree.Single<SyntaxVB.SingleLineLambdaExpressionSyntax>();
            ExtensionsVB.CreateCfg(lambda, model, default).Should().BeNull();
        }

        [DataTestMethod]
        [DataRow(@"() =>")]
        [DataRow(@"} () => }")]
        [DataRow(@"{ () => .")]
        [DataRow(@"{ () => => =>")]
        public void CreateCfg_InvalidSyntax_ReturnsCfg_CS(string code)
        {
            var (tree, model) = TestHelper.CompileIgnoreErrorsCS(code);
            var lambda = tree.Single<SyntaxCS.ParenthesizedLambdaExpressionSyntax>();

            ExtensionsCS.CreateCfg(lambda.Body, model, default).Should().NotBeNull();
        }

        [TestMethod]
        public void CreateCfg_Performance_UsesCache_CS()
        {
            const string code = @"
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
}";
            var (tree, model) = TestHelper.CompileCS(code);
            var lambda = tree.Single<SyntaxCS.ParenthesizedLambdaExpressionSyntax>();
            Action a = () =>
                {
                    for (var i = 0; i < 10000; i++)
                    {
                        ExtensionsCS.CreateCfg(lambda, model, default);
                    }
                };
            a.ExecutionTime().Should().BeLessThan(1.Seconds());     // Takes roughly 0.2 sec on CI
        }

        [TestMethod]
        public void CreateCfg_Performance_UsesCache_VB()
        {
            const string code = @"
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
End Class";
            var (tree, model) = TestHelper.CompileVB(code);
            var lambda = tree.Single<SyntaxVB.SingleLineLambdaExpressionSyntax>();
            Action a = () =>
            {
                for (var i = 0; i < 10000; i++)
                {
                    ExtensionsCS.CreateCfg(lambda, model, default);
                }
            };
            a.ExecutionTime().Should().BeLessThan(1.Seconds());     // Takes roughly 0.4 sec on CI
        }

        [TestMethod]
        public void CreateCfg_SameNode_DifferentCompilation_DoesNotCache()
        {
            // https://github.com/SonarSource/sonar-dotnet/issues/5491
            const string code = @"
public class Sample
{
    private void Method()
    { }
}";
            var compilation1 = TestHelper.CompileCS(code).Model.Compilation;
            var compilation2 = compilation1.WithAssemblyName("Different-Compilation-Reusing-Same-Nodes");
            var method1 = compilation1.SyntaxTrees.Single().Single<SyntaxCS.MethodDeclarationSyntax>();
            var method2 = compilation2.SyntaxTrees.Single().Single<SyntaxCS.MethodDeclarationSyntax>();
            var cfg1 = ExtensionsCS.CreateCfg(method1.Body, compilation1.GetSemanticModel(method1.SyntaxTree), default);
            var cfg2 = ExtensionsCS.CreateCfg(method2.Body, compilation2.GetSemanticModel(method2.SyntaxTree), default);

            ReferenceEquals(cfg1, cfg2).Should().BeFalse("Different compilations should not reuse cache. They do not share semantic model and symbols.");
        }

        [DataTestMethod]
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
            var syntaxTree = CSharpSyntaxTree.ParseText(code);
            var argument = syntaxTree.GetRoot().FindNode(new TextSpan(nodePosition, 0));
            argument = argument.AncestorsAndSelf()
                .FirstOrDefault(x => x.IsAnyKind(SyntaxKind.Argument,
                                                 SyntaxKindEx.DiscardDesignation,
                                                 SyntaxKindEx.SingleVariableDesignation,
                                                 SyntaxKindEx.ParenthesizedVariableDesignation,
                                                 SyntaxKindEx.TupleExpression)) ?? argument;
            syntaxTree.GetDiagnostics().Should().BeEmpty();
            var target = ExtensionsCS.FindAssignmentComplement(argument);
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
            var (tree, model) = TestHelper.CompileCS(code);
            var allIdentifiers = tree.GetRoot().DescendantNodes().OfType<SyntaxCS.IdentifierNameSyntax>().ToArray();
            allIdentifiers.Where(x => x.Identifier.ValueText == "xNormal").Should().HaveCount(6).And.OnlyContain(x => !ExtensionsCS.IsInExpressionTree(x, model));
            allIdentifiers.Where(x => x.Identifier.ValueText == "xExpres").Should().HaveCount(6).And.OnlyContain(x => ExtensionsCS.IsInExpressionTree(x, model));
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
            var (tree, model) = TestHelper.CompileVB(code);
            var allIdentifiers = tree.GetRoot().DescendantNodes().OfType<SyntaxVB.IdentifierNameSyntax>().ToArray();
            allIdentifiers.Where(x => x.Identifier.ValueText == "xNormal").Should().HaveCount(6).And.OnlyContain(x => !ExtensionsVB.IsInExpressionTree(x, model));
            allIdentifiers.Where(x => x.Identifier.ValueText == "xExpres").Should().HaveCount(6).And.OnlyContain(x => ExtensionsVB.IsInExpressionTree(x, model));
        }

        [DataTestMethod]
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
            var node = NodeBetweenMarkers(code, LanguageNames.CSharp);
            var parentConditional = ExtensionsCS.GetParentConditionalAccessExpression(node);
            parentConditional.ToString().Should().Be(parent);
            parentConditional.Expression.ToString().Should().Be(parentExpression);
        }

        [DataTestMethod]
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
            var node = NodeBetweenMarkers(code, LanguageNames.VisualBasic);
            var parentConditional = ExtensionsVB.GetParentConditionalAccessExpression(node);
            parentConditional.ToString().Should().Be(parent);
            parentConditional.Expression.ToString().Should().Be(parentExpression);
        }

        [DataTestMethod]
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
            var code = @$"
public class X
{{
    public X A {{ get; }}
    public X B {{ get; }}
    public X C {{ get; }}
    public X this[int i] => null;

    public X M()
    {{
        var _ = {expression};
        return null;
    }}
}}
";
            var node = NodeBetweenMarkers(code, LanguageNames.CSharp);
            var parentConditional = ExtensionsCS.GetRootConditionalAccessExpression(node);
            parentConditional.ToString().Should().Be(expression.Replace("$$", string.Empty));
        }

        [TestMethod]
        public void Kind_Null_ReturnsNone() =>
            SonarAnalyzer.Helpers.SyntaxNodeExtensions.Kind<SyntaxKind>(null).Should().Be(SyntaxKind.None);

        [DataTestMethod]
        [DataRow("class Test { }", DisplayName = "When there is no pragma, return default file name.")]
        [DataRow("#pragma checksum \"FileName.txt\" \"{guid}\" \"checksum bytes\"", "FileName.txt", DisplayName = "When pragma is present, return file name from pragma.")]
        public void GetMappedFilePath(string code, string expectedFileName = DefaultFileName)
        {
            var syntaxTree = GetSyntaxTree(code, DefaultFileName);
            syntaxTree.GetRoot().GetMappedFilePathFromRoot().Should().Be(expectedFileName);
        }

        private static SyntaxNode NodeBetweenMarkers(string code, string language)
        {
            var position = code.IndexOf("$$");
            var lastPosition = code.LastIndexOf("$$");
            var length = lastPosition == position ? 0 : lastPosition - position - "$$".Length;
            code = code.Replace("$$", string.Empty);
            var (tree, _) = IsCSharp() ? TestHelper.CompileCS(code) : TestHelper.CompileVB(code);
            var node = tree.GetRoot().FindNode(new TextSpan(position, length));
            return node;

            bool IsCSharp() => language == LanguageNames.CSharp;
        }

        private static SyntaxToken GetFirstTokenOfKind(SyntaxTree syntaxTree, SyntaxKind kind) =>
            syntaxTree.GetRoot().DescendantTokens().First(token => token.IsKind(kind));

        private static SyntaxTree GetSyntaxTree(string content, string fileName = null) =>
            SolutionBuilder
                .Create()
                .AddProject(AnalyzerLanguage.CSharp, createExtraEmptyFile: false)
                .AddSnippet(content, fileName)
                .GetCompilation()
                .SyntaxTrees
                .First();
    }
}
