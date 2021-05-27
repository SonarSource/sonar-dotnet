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
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SonarAnalyzer.UnitTest.MetadataReferences;
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
            var compilation = CreateCompilation("public class A{public int X;public A(int y){}} public class B{void Foo(){var bar =new A(1){X = 2};}}");
            var syntaxTree = compilation.SyntaxTrees.First();
            var objectCreation = syntaxTree.GetRoot().DescendantNodes().OfType<ObjectCreationExpressionSyntax>().First();
            var wrapper = ObjectCreationFactory.Create(objectCreation);
            wrapper.Expression.Should().BeEquivalentTo(objectCreation);
            wrapper.Initializer.Should().BeEquivalentTo(objectCreation.Initializer);
            wrapper.ArgumentList.Should().BeEquivalentTo(objectCreation.ArgumentList);
            wrapper.TypeAsString(compilation.GetSemanticModel(syntaxTree)).Should().Be("A");
        }

        [TestMethod]
        public void ImplicitObjectCreationSyntax()
        {
            var compilation = CreateCompilation("public class A{public int X;public A(int y){}} public class B{void Foo(){A bar =new(1){X = 2};}}");
            var syntaxTree = compilation.SyntaxTrees.First();
            var objectCreation = (ImplicitObjectCreationExpressionSyntaxWrapper)syntaxTree.GetRoot().DescendantNodes().First((node => node.IsKind(SyntaxKindEx.ImplicitObjectCreationExpression)));
            var wrapper = ObjectCreationFactory.Create(objectCreation);
            wrapper.Expression.Should().BeEquivalentTo(objectCreation.SyntaxNode);
            wrapper.Initializer.Should().BeEquivalentTo(objectCreation.Initializer);
            wrapper.ArgumentList.Should().BeEquivalentTo(objectCreation.ArgumentList);
            wrapper.TypeAsString(compilation.GetSemanticModel(syntaxTree)).Should().Be("A");
        }

        [TestMethod]
        public void GivenImplicitObjectCreationSyntaxWithMissingType_HasEmptyType()
        {
            var compilation = CreateCompilation("public class B{void Foo(){var bar =new();}}");
            var syntaxTree = compilation.SyntaxTrees.First();
            var objectCreation = (ImplicitObjectCreationExpressionSyntaxWrapper)syntaxTree.GetRoot().DescendantNodes().First((node => node.IsKind(SyntaxKindEx.ImplicitObjectCreationExpression)));
            var wrapper = ObjectCreationFactory.Create(objectCreation);
            wrapper.TypeAsString(compilation.GetSemanticModel(syntaxTree)).Should().BeEmpty();
        }

        [TestMethod]
        public void GivenNull_ThrowsException()
        {
            Action action = () => { ObjectCreationFactory.Create(null); };
#if NET
            action.Should().Throw<ArgumentNullException>().WithMessage("Argument should not be null (Parameter 'node')");
#else
            action.Should().Throw<ArgumentNullException>().WithMessage("Argument should not be null\nParameter name: node");
#endif
        }

        [TestMethod]
        public void GivenNonConstructor_ThrowsException()
        {
            var compilation = CreateCompilation("public class A{}");
            var syntaxTree = compilation.SyntaxTrees.First();
            var classDeclaration = syntaxTree.GetRoot().DescendantNodes().OfType<ClassDeclarationSyntax>().First();
            Action action = () => { ObjectCreationFactory.Create(classDeclaration); };
            action.Should().Throw<InvalidOperationException>().WithMessage("Unexpected type: ClassDeclarationSyntax");
        }

        private static CSharpCompilation CreateCompilation(string code) =>
            CSharpCompilation.Create("TempAssembly.dll")
                .AddSyntaxTrees(CSharpSyntaxTree.ParseText(code))
                .AddReferences(MetadataReferenceFacade.ProjectDefaultReferences)
                .WithOptions(new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
    }
}
