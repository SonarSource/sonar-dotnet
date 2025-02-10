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

namespace SonarAnalyzer.VisualBasic.Core.Facade.Test;

[TestClass]
public class VisualBasicFacadeTest
{
    [TestMethod]
    public void MethodParameterLookup_ForInvocation()
    {
        var sut = VisualBasicFacade.Instance;
        var code = """
            Public Class C
                Public Function M(arg As Integer) As Integer
                    Return M(1)
                End Function
            End Class
            """;
        var (tree, model) = TestCompiler.CompileVB(code);
        var root = tree.GetRoot();
        var invocation = root.DescendantNodes().OfType<InvocationExpressionSyntax>().First();
        var method = model.GetDeclaredSymbol(root.DescendantNodes().OfType<MethodStatementSyntax>().First());
        var actual = sut.MethodParameterLookup(invocation, method);
        actual.Should().NotBeNull().And.BeOfType<VisualBasicMethodParameterLookup>();
    }

    [TestMethod]
    public void MethodParameterLookup_SemanticModelOverload()
    {
        var code = """
            Public Class C
                Public Function M(arg As Integer) As Integer
                    Return M(1)
                End Function
            End Class
            """;
        var (tree, model) = TestCompiler.CompileVB(code);
        var actual = VisualBasicFacade.Instance.MethodParameterLookup(tree.GetRoot().DescendantNodes().OfType<InvocationExpressionSyntax>().First(), model);
        actual.Should().NotBeNull().And.BeOfType<VisualBasicMethodParameterLookup>();
    }

    [TestMethod]
    public void MethodParameterLookup_ForObjectCreation()
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
        var (tree, model) = TestCompiler.CompileVB(code);
        var root = tree.GetRoot();
        var creation = root.DescendantNodes().OfType<ObjectCreationExpressionSyntax>().First();
        var constructor = model.GetDeclaredSymbol(root.DescendantNodes().OfType<SubNewStatementSyntax>().First());
        var actual = sut.MethodParameterLookup(creation, constructor);
        actual.Should().NotBeNull().And.BeOfType<VisualBasicMethodParameterLookup>();
    }

    [TestMethod]
    public void MethodParameterLookup_ForArgumentList()
    {
        var sut = VisualBasicFacade.Instance;
        var code = """
            Public Class C
                Public Function M(arg As Integer) As Integer
                    Return M(1)
                End Function
            End Class
            """;
        var (tree, model) = TestCompiler.CompileVB(code);
        var root = tree.GetRoot();
        var argumentList = root.DescendantNodes().OfType<ArgumentListSyntax>().First();
        var method = model.GetDeclaredSymbol(root.DescendantNodes().OfType<MethodStatementSyntax>().First());
        var actual = sut.MethodParameterLookup(argumentList, method);
        actual.Should().NotBeNull().And.BeOfType<VisualBasicMethodParameterLookup>();
    }

    [TestMethod]
    public void MethodParameterLookup_UnsupportedSyntaxKind()
    {
        var sut = VisualBasicFacade.Instance;
        var code = """
            Public Class C
                Public Function M(arg As Integer) As Integer
                    Return M(1)
                End Function
            End Class
            """;
        var (tree, model) = TestCompiler.CompileVB(code);
        var root = tree.GetRoot();
        var methodDeclaration = root.DescendantNodes().OfType<MethodStatementSyntax>().First();
        var method = model.GetDeclaredSymbol(methodDeclaration);
        var actual = () => sut.MethodParameterLookup(methodDeclaration, method); // MethodDeclarationSyntax passed instead of invocation
        actual.Should().Throw<InvalidOperationException>().Which.Message.Should().StartWith("The node of kind FunctionStatement does not have an ArgumentList.");
    }

    [TestMethod]
    public void MethodParameterLookup_Null()
    {
        var sut = VisualBasicFacade.Instance;
        var code = """
            Public Class C
                Public Function M(arg As Integer) As Integer
                    Return M(1)
                End Function
            End Class
            """;
        var (tree, model) = TestCompiler.CompileVB(code);
        var root = tree.GetRoot();
        var method = model.GetDeclaredSymbol(root.DescendantNodes().OfType<MethodStatementSyntax>().First());
        var actual = sut.MethodParameterLookup(null, method);
        actual.Should().BeNull();
    }

    [TestMethod]
    public void MethodParameterLookup_Null_SemanticModelOverload()
    {
        var sut = VisualBasicFacade.Instance;
        var code = """
            Public Class C
                Public Function M(arg As Integer) As Integer
                    Return M(1)
                End Function
            End Class
            """;
        var (_, model) = TestCompiler.CompileVB(code);
        var actual = sut.MethodParameterLookup(null, model);
        actual.Should().BeNull();
    }
}
