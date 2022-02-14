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

extern alias csharp;
extern alias vbnet;
using System.Linq;
using FluentAssertions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Operations;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using static csharp::SonarAnalyzer.Extensions.SyntaxTokenExtensions;
using CS = Microsoft.CodeAnalysis.CSharp.Syntax;
using SyntaxNodeExtensionsCS = csharp::SonarAnalyzer.Extensions.SyntaxNodeExtensions;
using SyntaxNodeExtensionsVB = vbnet::SonarAnalyzer.Extensions.SyntaxNodeExtensions;
using VB = Microsoft.CodeAnalysis.VisualBasic.Syntax;

namespace SonarAnalyzer.UnitTest.Extensions
{
    [TestClass]
    public class SyntaxNodeExtensionsTest
    {
        [TestMethod]
        public void GetPreviousStatementsCurrentBlockOfNotAStatement()
        {
            const string code = "int x = 42;";

            var syntaxTree = CSharpSyntaxTree.ParseText(code);
            var compilationUnit = syntaxTree.GetCompilationUnitRoot();

            SyntaxNodeExtensionsCS.GetPreviousStatementsCurrentBlock(compilationUnit).Should().BeEmpty();
        }

        [TestMethod]
        public void GetPreviousStatementsCurrentBlockOfFirstStatement()
        {
            const string code = "public void M() { int x = 42; }";

            var syntaxTree = CSharpSyntaxTree.ParseText(code);
            var aToken = GetFirstTokenOfKind(syntaxTree, SyntaxKind.NumericLiteralToken);

            var parent = aToken.GetBindableParent();
            SyntaxNodeExtensionsCS.GetPreviousStatementsCurrentBlock(parent).Should().BeEmpty();
        }

        [TestMethod]
        public void GetPreviousStatementsCurrentBlockOfSecondStatement()
        {
            const string code = "public void M() { string s = null; int x = 42; }";

            var syntaxTree = CSharpSyntaxTree.ParseText(code);
            var aToken = GetFirstTokenOfKind(syntaxTree, SyntaxKind.NumericLiteralToken);

            var parent = aToken.GetBindableParent();
            SyntaxNodeExtensionsCS.GetPreviousStatementsCurrentBlock(parent).Should().HaveCount(1);
        }

        [TestMethod]
        public void GetPreviousStatementsCurrentBlockRetrievesOnlyForCurrentBlock()
        {
            const string code = "public void M(string y) { string s = null; if (y != null) { int x = 42; } }";

            var syntaxTree = CSharpSyntaxTree.ParseText(code);
            var aToken = GetFirstTokenOfKind(syntaxTree, SyntaxKind.NumericLiteralToken);

            var parent = aToken.GetBindableParent();
            SyntaxNodeExtensionsCS.GetPreviousStatementsCurrentBlock(parent).Should().BeEmpty();
        }

        [TestMethod]
        public void ArrowExpressionBody_WithNotExpectedNode_ReturnsNull()
        {
            const string code = "var x = 1;";
            var syntaxTree = CSharpSyntaxTree.ParseText(code);
            SyntaxNodeExtensionsCS.ArrowExpressionBody(syntaxTree.GetRoot()).Should().BeNull();
        }

        [TestMethod]
        public void GetDeclarationTypeName_UnknownType() =>
#if DEBUG
            Assert.ThrowsException<ArgumentException>(() => SyntaxNodeExtensionsCS.GetDeclarationTypeName(SyntaxFactory.Block()), "Unexpected type Block\r\nParameter name: kind");
#else
            SyntaxNodeExtensionsCS.GetDeclarationTypeName(SyntaxFactory.Block()).Should().Be("type");
#endif

        [TestMethod]
        public void CreateCfg_MethodBody_ReturnsCfg_CS()
        {
            var code = @"
public class Sample
{
    public void Main()
    {
        var x = 42;
    }
}";
            var (tree, semanticModel) = TestHelper.CompileCS(code);
            var node = tree.GetRoot().DescendantNodes().OfType<CS.MethodDeclarationSyntax>().Single();

            SyntaxNodeExtensionsCS.CreateCfg(node.Body, semanticModel).Should().NotBeNull();
        }

        [TestMethod]
        public void CreateCfg_MethodBlock_ReturnsCfg_VB()
        {
            var code = @"
Public Class Sample
    Public Sub Main()
        Dim X As Integer = 42
    End Sub
End Class";
            var (tree, semanticModel) = TestHelper.CompileVB(code);
            var node = tree.GetRoot().DescendantNodes().OfType<VB.MethodBlockSyntax>().Single();

            SyntaxNodeExtensionsVB.CreateCfg(node, semanticModel).Should().NotBeNull();
        }

        [TestMethod]
        public void CreateCfg_AnyNode_ReturnsCfg_CS()
        {
            var code = @"
public class Sample
{
    public void Main()
    {
        Main();
    }
}";
            var (tree, semanticModel) = TestHelper.CompileCS(code);
            var node = tree.GetRoot().DescendantNodes().OfType<CS.InvocationExpressionSyntax>().Single();

            SyntaxNodeExtensionsCS.CreateCfg(node, semanticModel).Should().NotBeNull();
        }

        [TestMethod]
        public void CreateCfg_AnyNode_ReturnsCfg_VB()
        {
            var code = @"
Public Class Sample
    Public Sub Main()
        Main()
    End Sub
End Class";
            var (tree, semanticModel) = TestHelper.CompileVB(code);
            var node = tree.GetRoot().DescendantNodes().OfType<VB.InvocationExpressionSyntax>().Single();

            SyntaxNodeExtensionsVB.CreateCfg(node, semanticModel).Should().NotBeNull();
        }

        [TestMethod]
        public void CreateCfg_LambdaInsideQuery_CS()
        {
            var code = @"
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
            var lambda = tree.GetRoot().DescendantNodes().OfType<CS.ParenthesizedLambdaExpressionSyntax>().Single();

            SyntaxNodeExtensionsCS.CreateCfg(lambda.Body, semanticModel).Should().NotBeNull();
        }

        [TestMethod]
        public void CreateCfg_LambdaInsideQuery_VB()
        {
            var code = @"
Public Class Sample
    Public Sub Main(Values() As Integer)
        Dim Result As IEnumerable(Of Lazy(Of Integer)) = From Value In Values Select New Lazy(Of Integer)(Function() Value)
    End Sub
End Class
";
            var (tree, semanticModel) = TestHelper.CompileVB(code);
            var lambda = tree.GetRoot().DescendantNodes().OfType<VB.SingleLineLambdaExpressionSyntax>().Single();

            SyntaxNodeExtensionsVB.CreateCfg(lambda.Body, semanticModel).Should().NotBeNull();
        }

        [TestMethod]
        public void CreateCfg_NestingChain_CS()
        {
            var code = @"
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
            var innerLambda = tree.GetRoot().DescendantNodes().OfType<CS.SimpleLambdaExpressionSyntax>().Single();
            innerLambda.Parent.Parent.Should().BeOfType<CS.VariableDeclaratorSyntax>().Subject.Identifier.ValueText.Should().Be("innerLambda");

            var cfg = SyntaxNodeExtensionsCS.CreateCfg(innerLambda.Body, semanticModel);
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
            var code = @"
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
            var innerSub = tree.GetRoot().DescendantNodes().OfType<VB.InvocationExpressionSyntax>().Single().FirstAncestorOrSelf<VB.SingleLineLambdaExpressionSyntax>();
            innerSub.Parent.Parent.Should().BeOfType<VB.VariableDeclaratorSyntax>().Subject.Names.Single().Identifier.ValueText.Should().Be("InnerSingleLineSub");

            var cfg = SyntaxNodeExtensionsVB.CreateCfg(innerSub, semanticModel);
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
            var code = @"
public class Sample
{
    public void Main()
    {
        Undefined(() => 45);
    }
}";
            var (tree, model) = TestHelper.CompileIgnoreErrorsCS(code);
            var lambda = tree.GetRoot().DescendantNodes().OfType<CS.ParenthesizedLambdaExpressionSyntax>().Single();
            SyntaxNodeExtensionsCS.CreateCfg(lambda.Body, model).Should().NotBeNull();
        }

        [TestMethod]
        public void CreateCfg_UndefinedSymbol_ReturnsNull_VB()
        {
            var code = @"
Public Class Sample
    Public Sub Main()
        Undefined(Function() 42)
    End Sub
End Class";
            var (tree, model) = TestHelper.CompileIgnoreErrorsVB(code);
            var lambda = tree.GetRoot().DescendantNodes().OfType<VB.SingleLineLambdaExpressionSyntax>().Single();
            SyntaxNodeExtensionsVB.CreateCfg(lambda, model).Should().BeNull();
        }

        [DataTestMethod]
        [DataRow(@"() =>")]
        [DataRow(@"} () => }")]
        [DataRow(@"{ () => .")]
        [DataRow(@"{ () => => =>")]
        public void CreateCfg_InvalidSyntax_ReturnsCfg_CS(string code)
        {
            var (tree, model) = TestHelper.CompileIgnoreErrorsCS(code);
            var lambda = tree.GetRoot().DescendantNodes().OfType<CS.ParenthesizedLambdaExpressionSyntax>().Single();

            SyntaxNodeExtensionsCS.CreateCfg(lambda.Body, model).Should().NotBeNull();
        }

        private static SyntaxToken GetFirstTokenOfKind(SyntaxTree syntaxTree, SyntaxKind kind) =>
            syntaxTree.GetRoot().DescendantTokens().First(token => token.IsKind(kind));
    }
}
