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

using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using StyleCop.Analyzers.Lightup;

namespace SonarAnalyzer.UnitTest.Facade;

[TestClass]
public class CSharpFacadeTests
{
    [TestMethod]
    public void MethodParameterLookupForInvocation()
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
    public void MethodParameterLookupForObjectCreation()
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
    public void MethodParameterLookupForImplicitObjectCreation()
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
    public void MethodParameterLookupUnsupportedSyntaxKind()
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
        actual.Should().Throw<ArgumentException>().WithMessage("Microsoft.CodeAnalysis.CSharp.Syntax.MethodDeclarationSyntax does not contain an ArgumentList. (Parameter 'invocation')");
    }

    [TestMethod]
    public void MethodParameterLookupNull()
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
}
