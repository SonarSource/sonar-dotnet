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

using Microsoft.CodeAnalysis.VisualBasic;
using Microsoft.CodeAnalysis.VisualBasic.Syntax;

namespace SonarAnalyzer.UnitTest.Facade;

[TestClass]
public class VisualBasicFacadeTests
{
    [TestMethod]
    public void MethodParameterLookupForInvocation()
    {
        var sut = VisualBasicFacade.Instance;
        var code = """
            Public Class C
                Public Function M(arg As Integer) As Integer
                    Return M(1)
                End Function
            End Class
            """;
        var (tree, model) = TestHelper.CompileVB(code);
        var root = tree.GetRoot();
        var invocation = root.DescendantNodes().OfType<InvocationExpressionSyntax>().First();
        var method = model.GetDeclaredSymbol(root.DescendantNodes().OfType<MethodStatementSyntax>().First());
        var actual = sut.MethodParameterLookup(invocation, method);
        actual.Should().NotBeNull().And.BeOfType<VisualBasicMethodParameterLookup>();
    }

    [TestMethod]
    public void MethodParameterLookupAlternativeOverload()
    {
        var sut = VisualBasicFacade.Instance;
        var code = """
            Public Class C
                Public Function M(arg As Integer) As Integer
                    Return M(1)
                End Function
            End Class
            """;
        var (tree, model) = TestHelper.CompileVB(code);
        var root = tree.GetRoot();
        var invocation = root.DescendantNodes().OfType<InvocationExpressionSyntax>().First();
        var actual = sut.MethodParameterLookup(invocation, model);
        actual.Should().NotBeNull().And.BeOfType<VisualBasicMethodParameterLookup>();
    }

    [TestMethod]
    public void MethodParameterLookupForObjectCreation()
    {
        var sut = VisualBasicFacade.Instance;
        var code = """
            Public Class C
                Public Sub New(arg As Integer)
                End Sub

                Public Function M() As C
                    Return New C(1)
                End Function
            End Class
            """;
        var (tree, model) = TestHelper.CompileVB(code);
        var root = tree.GetRoot();
        var creation = root.DescendantNodes().OfType<ObjectCreationExpressionSyntax>().First();
        var constructor = model.GetDeclaredSymbol(root.DescendantNodes().OfType<SubNewStatementSyntax>().First());
        var actual = sut.MethodParameterLookup(creation, constructor);
        actual.Should().NotBeNull().And.BeOfType<VisualBasicMethodParameterLookup>();
    }

    [TestMethod]
    public void MethodParameterLookupUnsupportedSyntaxKind()
    {
        var sut = VisualBasicFacade.Instance;
        var code = """
            Public Class C
                Public Function M(arg As Integer) As Integer
                    Return M(1)
                End Function
            End Class
            """;
        var (tree, model) = TestHelper.CompileVB(code);
        var root = tree.GetRoot();
        var methodDeclaration = root.DescendantNodes().OfType<MethodStatementSyntax>().First();
        var method = model.GetDeclaredSymbol(methodDeclaration);
        var actual = () => sut.MethodParameterLookup(methodDeclaration, method); // MethodDeclarationSyntax passed instead of invocation
        actual.Should().Throw<ArgumentException>().Which.Message.Should().StartWith("Microsoft.CodeAnalysis.VisualBasic.Syntax.MethodStatementSyntax does not contain an ArgumentList.");
    }

    [TestMethod]
    public void MethodParameterLookupNull()
    {
        var sut = VisualBasicFacade.Instance;
        var code = """
            Public Class C
                Public Function M(arg As Integer) As Integer
                    Return M(1)
                End Function
            End Class
            """;
        var (tree, model) = TestHelper.CompileVB(code);
        var root = tree.GetRoot();
        var method = model.GetDeclaredSymbol(root.DescendantNodes().OfType<MethodStatementSyntax>().First());
        var actual = sut.MethodParameterLookup(null, method);
        actual.Should().BeNull();
    }
}
