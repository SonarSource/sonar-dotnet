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
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SonarAnalyzer.Common;
using SonarAnalyzer.UnitTest.TestFramework;
using SonarAnalyzer.Wrappers;
using StyleCop.Analyzers.Lightup;

namespace SonarAnalyzer.UnitTest.Wrappers
{
    [TestClass]
    public class ObjectCreationFactoryTest
    {
        [TestMethod]
        public void ObjectCreationSyntax()
        {
            const string code = @"
                public class A
                {
                    public int X;
                    public A(int y) { }
                }
                public class B
                {
                    void Foo()
                    {
                        var bar = new A(1) { X = 2 };
                    }
                }";
            var snippet = new SnippetCompiler(code);
            var objectCreation = snippet.SyntaxTree.GetRoot().DescendantNodes().OfType<ObjectCreationExpressionSyntax>().First();
            var wrapper = ObjectCreationFactory.Create(objectCreation);
            wrapper.Expression.Should().BeEquivalentTo(objectCreation);
            wrapper.Initializer.Should().BeEquivalentTo(objectCreation.Initializer);
            wrapper.ArgumentList.Should().BeEquivalentTo(objectCreation.ArgumentList);
            wrapper.TypeAsString(snippet.SemanticModel).Should().Be("A");
            wrapper.TypeSymbol(snippet.SemanticModel).Name.Should().Be("A");
        }

        [TestMethod]
        public void ObjectCreationEmptyInitializerSyntax()
        {
            const string code = @"
                public class A
                {
                    public int X;
                    public A(int y) { }
                }
                public class B
                {
                    void Foo()
                    {
                        var bar = new A(1);
                    }
                }";
            var snippet = new SnippetCompiler(code);
            var objectCreation = snippet.SyntaxTree.GetRoot().DescendantNodes().OfType<ObjectCreationExpressionSyntax>().First();
            var wrapper = ObjectCreationFactory.Create(objectCreation);
            wrapper.Initializer.Should().BeNull();
        }

        [TestMethod]
        public void ImplicitObjectCreationSyntax()
        {
            const string code = @"
                public class A
                {
                    public int X;
                    public A(int y) { }
                }
                public class B
                {
                    void Foo()
                    {
                        A bar =new(1) { X = 2 };
                    }
                }";
            var snippet = new SnippetCompiler(code);
            var syntaxTree = snippet.SyntaxTree;
            var objectCreation = (ImplicitObjectCreationExpressionSyntaxWrapper)syntaxTree.GetRoot().DescendantNodes().First((node => node.IsKind(SyntaxKindEx.ImplicitObjectCreationExpression)));
            var wrapper = ObjectCreationFactory.Create(objectCreation);
            wrapper.Expression.Should().BeEquivalentTo(objectCreation.SyntaxNode);
            wrapper.Initializer.Should().BeEquivalentTo(objectCreation.Initializer);
            wrapper.ArgumentList.Should().BeEquivalentTo(objectCreation.ArgumentList);
            wrapper.TypeAsString(snippet.SemanticModel).Should().Be("A");
            wrapper.TypeSymbol(snippet.SemanticModel).Name.Should().Be("A");
        }

        [TestMethod]
        public void ImplicitObjectCreationEmptyInitializerSyntax()
        {
            const string code = @"
                public class A
                {
                    public int X;
                    public A(int y) { }
                }
                public class B
                {
                    void Foo()
                    {
                        A bar = new (1);
                    }
                }";
            var snippet = new SnippetCompiler(code);
            var objectCreation = snippet.SyntaxTree.GetRoot().DescendantNodes().OfType<ImplicitObjectCreationExpressionSyntax>().First();
            var wrapper = ObjectCreationFactory.Create(objectCreation);
            wrapper.Initializer.Should().BeNull();
        }

        [TestMethod]
        public void GivenImplicitObjectCreationSyntaxWithMissingType_HasEmptyType()
        {
            const string code = @"
                public class B
                {
                    void Foo()
                    {
                        var bar = new();
                    }
                }";
            var snippet = new SnippetCompiler(code, true, AnalyzerLanguage.CSharp);
            var syntaxTree = snippet.SyntaxTree;
            var objectCreation = (ImplicitObjectCreationExpressionSyntaxWrapper)syntaxTree.GetRoot().DescendantNodes().First((node => node.IsKind(SyntaxKindEx.ImplicitObjectCreationExpression)));
            var wrapper = ObjectCreationFactory.Create(objectCreation);
            wrapper.TypeAsString(snippet.SemanticModel).Should().BeEmpty();
        }

        [TestMethod]
        public void GivenNull_ThrowsException()
        {
            Action action = () => { ObjectCreationFactory.Create(null); };
            action.Should().Throw<ArgumentNullException>();
        }

        [TestMethod]
        public void GivenNonConstructor_ThrowsException()
        {
            var snippet = new SnippetCompiler("public class A{}");
            var classDeclaration = snippet.SyntaxTree.GetRoot().DescendantNodes().OfType<ClassDeclarationSyntax>().First();
            Action action = () => { ObjectCreationFactory.Create(classDeclaration); };
            action.Should().Throw<InvalidOperationException>().WithMessage("Unexpected type: ClassDeclarationSyntax");
        }
    }
}
