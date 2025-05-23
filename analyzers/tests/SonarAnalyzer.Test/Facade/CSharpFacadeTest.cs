﻿/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2024 SonarSource SA
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
using StyleCop.Analyzers.Lightup;

namespace SonarAnalyzer.Test.Facade;

[TestClass]
public class CSharpFacadeTest
{
    [TestMethod]
    public void MethodParameterLookup_ForInvocation()
    {
        var sut = CSharpFacade.Instance;
        var code = """
            public class C
            {
                public int M(int arg) =>
                    M(1);
            }
            """;
        var (tree, model) = TestHelper.CompileCS(code);
        var root = tree.GetRoot();
        var invocation = root.DescendantNodes().OfType<InvocationExpressionSyntax>().First();
        var method = model.GetDeclaredSymbol(root.DescendantNodes().OfType<MethodDeclarationSyntax>().First());
        var actual = sut.MethodParameterLookup(invocation, method);
        actual.Should().NotBeNull().And.BeOfType<CSharpMethodParameterLookup>();
    }

    [TestMethod]
    public void MethodParameterLookup_SemanticModelOverload()
    {
        var code = """
            public class C
            {
                public int M(int arg) =>
                    M(1);
            }
            """;
        var (tree, model) = TestHelper.CompileCS(code);
        var lookup = CSharpFacade.Instance.MethodParameterLookup(tree.GetRoot().DescendantNodes().OfType<InvocationExpressionSyntax>().First(), model);
        lookup.Should().NotBeNull().And.BeOfType<CSharpMethodParameterLookup>();
    }

    [TestMethod]
    public void MethodParameterLookup_ForObjectCreation()
    {
        var sut = CSharpFacade.Instance;
        var code = """
            public class C
            {
                public C(int arg) { }

                public C M() =>
                    new C(1);
            }
            """;
        var (tree, model) = TestHelper.CompileCS(code);
        var root = tree.GetRoot();
        var creation = root.DescendantNodes().OfType<ObjectCreationExpressionSyntax>().First();
        var constructor = model.GetDeclaredSymbol(root.DescendantNodes().OfType<ConstructorDeclarationSyntax>().First());
        var actual = sut.MethodParameterLookup(creation, constructor);
        actual.Should().NotBeNull().And.BeOfType<CSharpMethodParameterLookup>();
    }

    [TestMethod]
    public void MethodParameterLookup_ForImplicitObjectCreation()
    {
        var sut = CSharpFacade.Instance;
        var code = """
            public class C
            {
                public C(int arg) { }

                public C M() =>
                    new(1);
            }
            """;
        var (tree, model) = TestHelper.CompileCS(code);
        var root = tree.GetRoot();
        var creation = root.DescendantNodes().First(x => x.IsKind(SyntaxKindEx.ImplicitObjectCreationExpression));
        var constructor = model.GetDeclaredSymbol(root.DescendantNodes().OfType<ConstructorDeclarationSyntax>().First());
        var actual = sut.MethodParameterLookup(creation, constructor);
        actual.Should().NotBeNull().And.BeOfType<CSharpMethodParameterLookup>();
    }

    [TestMethod]
    public void MethodParameterLookup_ForArgumentList()
    {
        var sut = CSharpFacade.Instance;
        var code = """
            public class C
            {
                public int M(int arg) =>
                    M(1);
            }
            """;
        var (tree, model) = TestHelper.CompileCS(code);
        var root = tree.GetRoot();
        var argumentList = root.DescendantNodes().OfType<ArgumentListSyntax>().First();
        var method = model.GetDeclaredSymbol(root.DescendantNodes().OfType<MethodDeclarationSyntax>().First());
        var actual = () => sut.MethodParameterLookup(argumentList, method);
        actual.Should().Throw<InvalidOperationException>();
    }

    [TestMethod]
    public void MethodParameterLookup_UnsupportedSyntaxKind()
    {
        var sut = CSharpFacade.Instance;
        var code = """
            public class C
            {
                public int M(int arg) =>
                    M(1);
            }
            """;
        var (tree, model) = TestHelper.CompileCS(code);
        var root = tree.GetRoot();
        var methodDeclaration = root.DescendantNodes().OfType<MethodDeclarationSyntax>().First();
        var method = model.GetDeclaredSymbol(methodDeclaration);
        var actual = () => sut.MethodParameterLookup(methodDeclaration, method); // MethodDeclarationSyntax passed instead of invocation
        actual.Should().Throw<InvalidOperationException>().Which.Message.Should().StartWith("The node of kind MethodDeclaration does not have an ArgumentList.");
    }

    [TestMethod]
    public void MethodParameterLookup_Null()
    {
        var sut = CSharpFacade.Instance;
        var code = """
            public class C
            {
                public int M(int arg) =>
                    M(1);
            }
            """;
        var (tree, model) = TestHelper.CompileCS(code);
        var root = tree.GetRoot();
        var method = model.GetDeclaredSymbol(root.DescendantNodes().OfType<MethodDeclarationSyntax>().First());
        var actual = sut.MethodParameterLookup(null, method);
        actual.Should().BeNull();
    }

    [TestMethod]
    public void MethodParameterLookup_Null_SemanticModelOverload()
    {
        var sut = CSharpFacade.Instance;
        var code = """
            public class C
            {
                public int M(int arg) =>
                    M(1);
            }
            """;
        var (_, model) = TestHelper.CompileCS(code);
        var actual = sut.MethodParameterLookup(null, model);
        actual.Should().BeNull();
    }
}
