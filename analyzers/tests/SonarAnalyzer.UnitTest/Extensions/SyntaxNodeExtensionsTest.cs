/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2021 SonarSource SA
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
using System;
using System.Linq;
using FluentAssertions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using static csharp::SonarAnalyzer.Extensions.SyntaxTokenExtensions;
using SyntaxNodeExtensions = csharp::SonarAnalyzer.Extensions.SyntaxNodeExtensions;

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

            SyntaxNodeExtensions.GetPreviousStatementsCurrentBlock(compilationUnit).Should().BeEmpty();
        }

        [TestMethod]
        public void GetPreviousStatementsCurrentBlockOfFirstStatement()
        {
            const string code = "public void M() { int x = 42; }";

            var syntaxTree = CSharpSyntaxTree.ParseText(code);
            var aToken = GetFirstTokenOfKind(syntaxTree, SyntaxKind.NumericLiteralToken);

            var parent = aToken.GetBindableParent();
            SyntaxNodeExtensions.GetPreviousStatementsCurrentBlock(parent).Should().BeEmpty();
        }

        [TestMethod]
        public void GetPreviousStatementsCurrentBlockOfSecondStatement()
        {
            const string code = "public void M() { string s = null; int x = 42; }";

            var syntaxTree = CSharpSyntaxTree.ParseText(code);
            var aToken = GetFirstTokenOfKind(syntaxTree, SyntaxKind.NumericLiteralToken);

            var parent = aToken.GetBindableParent();
            SyntaxNodeExtensions.GetPreviousStatementsCurrentBlock(parent).Should().HaveCount(1);
        }

        [TestMethod]
        public void GetPreviousStatementsCurrentBlockRetrievesOnlyForCurrentBlock()
        {
            const string code = "public void M(string y) { string s = null; if (y != null) { int x = 42; } }";

            var syntaxTree = CSharpSyntaxTree.ParseText(code);
            var aToken = GetFirstTokenOfKind(syntaxTree, SyntaxKind.NumericLiteralToken);

            var parent = aToken.GetBindableParent();
            SyntaxNodeExtensions.GetPreviousStatementsCurrentBlock(parent).Should().BeEmpty();
        }

        [TestMethod]
        public void ArrowExpressionBody_WithNotExpectedNode_ReturnsNull()
        {
            const string code = "var x = 1;";
            var syntaxTree = CSharpSyntaxTree.ParseText(code);
            SyntaxNodeExtensions.ArrowExpressionBody(syntaxTree.GetRoot()).Should().BeNull();
        }

        [TestMethod]
        public void GetDeclarationTypeName_UnknownType() =>
#if DEBUG
            Assert.ThrowsException<ArgumentException>(() => SyntaxNodeExtensions.GetDeclarationTypeName(SyntaxFactory.Block()), "Unexpected type Block\r\nParameter name: kind");
#else
            SyntaxNodeExtensions.GetDeclarationTypeName(SyntaxFactory.Block()).Should().Be("type");
#endif

        [TestMethod]
        public void CreateCfg_MethodBody_ReturnsCfg()
        {
            var code = @"
using System.Linq;
public class Sample
{
    public void Main()
    {
        var x = 42;
    }
}";
            var (tree, semanticModel) = TestHelper.Compile(code);
            var node = tree.GetRoot().DescendantNodes().OfType<MethodDeclarationSyntax>().Single();

            SyntaxNodeExtensions.CreateCfg(node.Body, semanticModel).Should().NotBeNull();
        }

        [TestMethod]
        public void CreateCfg_AnyNode_ReturnsCfg()
        {
            var code = @"
using System.Linq;
public class Sample
{
    public void Main()
    {
        Main();
    }
}";
            var (tree, semanticModel) = TestHelper.Compile(code);
            var node = tree.GetRoot().DescendantNodes().OfType<InvocationExpressionSyntax>().Single();

            SyntaxNodeExtensions.CreateCfg(node, semanticModel).Should().NotBeNull();
        }

        [TestMethod]
        public void CreateCfg_LambdaInsideQuery()
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
            var (tree, semanticModel) = TestHelper.Compile(code);
            var lambda = tree.GetRoot().DescendantNodes().OfType<ParenthesizedLambdaExpressionSyntax>().Single();

            SyntaxNodeExtensions.CreateCfg(lambda.Body, semanticModel).Should().NotBeNull();
        }

        [TestMethod]
        public void CreateCfg_InvalidSyntax_ReturnsCfg()
        {
            var code = @"
public class Sample
{
    public void Main()
    {
        Undefined(() => 45);
    }
}";
            var (tree, semanticModel) = TestHelper.Compile(code);
            var lambda = tree.GetRoot().DescendantNodes().OfType<ParenthesizedLambdaExpressionSyntax>().Single();

            SyntaxNodeExtensions.CreateCfg(lambda.Body, semanticModel).Should().NotBeNull();
        }

        private static SyntaxToken GetFirstTokenOfKind(SyntaxTree syntaxTree, SyntaxKind kind) =>
            syntaxTree.GetRoot().DescendantTokens().First(token => token.IsKind(kind));
    }
}
