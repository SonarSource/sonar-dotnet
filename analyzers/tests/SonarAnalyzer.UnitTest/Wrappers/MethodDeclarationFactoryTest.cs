extern alias csharp;
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

using System;
using System.Linq;
using FluentAssertions;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using csharp::SonarAnalyzer.Wrappers;
using SonarAnalyzer.UnitTest.TestFramework;

namespace SonarAnalyzer.UnitTest.Wrappers
{
    [TestClass]
    public class MethodDeclarationFactoryTest
    {
        [TestMethod]
        public void MethodDeclarationFactory_WithMethodDeclaration()
        {
            const string code = @"
                public class Foo
                {
                    public void Bar(int y) { }
                }";
            var snippet = new SnippetCompiler(code);
            var method = snippet.SyntaxTree.GetRoot().DescendantNodes().OfType<MethodDeclarationSyntax>().First();
            var wrapper = MethodDeclarationFactory.Create(method);
            wrapper.Body.Should().BeEquivalentTo(method.Body);
            wrapper.ExpressionBody.Should().BeEquivalentTo(method.ExpressionBody);
            wrapper.Identifier.Should().BeEquivalentTo(method.Identifier);
            wrapper.ParameterList.Should().BeEquivalentTo(method.ParameterList);
            wrapper.HasImplementation.Should().BeTrue();
            wrapper.IsLocal.Should().BeFalse();
        }

        [TestMethod]
        public void MethodDeclarationFactory_WithLocalFunctionDeclaration()
        {
            const string code = @"
                public class Foo
                {
                    public void Bar(int a)
                    {
                        LocalFunction();
                        int LocalFunction() => 1;
                    }
                }";
            var snippet = new SnippetCompiler(code);
            var method = snippet.SyntaxTree.GetRoot().DescendantNodes().OfType<LocalFunctionStatementSyntax>().First();
            var wrapper = MethodDeclarationFactory.Create(method);
            wrapper.Body.Should().BeEquivalentTo(method.Body);
            wrapper.ExpressionBody.Should().BeEquivalentTo(method.ExpressionBody);
            wrapper.Identifier.Should().BeEquivalentTo(method.Identifier);
            wrapper.ParameterList.Should().BeEquivalentTo(method.ParameterList);
            wrapper.HasImplementation.Should().BeTrue();
            wrapper.IsLocal.Should().BeTrue();
        }

        [TestMethod]
        public void MethodDeclarationFactory_WithMethodDeclaration_NoImplementation()
        {
            const string code = @"
                partial class Foo
                {
                    partial void Bar(int a);
                }";
            var snippet = new SnippetCompiler(code);
            var method = snippet.SyntaxTree.GetRoot().DescendantNodes().OfType<MethodDeclarationSyntax>().First();
            var wrapper = MethodDeclarationFactory.Create(method);
            wrapper.HasImplementation.Should().BeFalse();
        }

        [TestMethod]
        public void MethodDeclarationFactory_Throws_WhenNull()
        {
            Action a = () => MethodDeclarationFactory.Create(null);
            a.Should().Throw<ArgumentNullException>().WithMessage("*node*");
        }

        [TestMethod]
        public void MethodDeclarationFactory_Throws_WhenNotMethodOrLocalFunction()
        {
            const string code = @"
                public partial class Foo
                {
                }";
            var snippet = new SnippetCompiler(code);
            var method = snippet.SyntaxTree.GetRoot().DescendantNodes().OfType<ClassDeclarationSyntax>().First();
            Action a = () => MethodDeclarationFactory.Create(method);
            a.Should().Throw<InvalidOperationException>().WithMessage("Unexpected type: ClassDeclarationSyntax");
        }
    }
}
