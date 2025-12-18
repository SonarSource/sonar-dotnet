/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2014-2025 SonarSource Sàrl
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

namespace SonarAnalyzer.CSharp.Core.Test.Wrappers;

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
        var method = snippet.Tree.Single<MethodDeclarationSyntax>();
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
        var method = snippet.Tree.Single<LocalFunctionStatementSyntax>();
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
        var method = snippet.Tree.Single<MethodDeclarationSyntax>();
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
        var method = snippet.Tree.Single<ClassDeclarationSyntax>();
        Action a = () => MethodDeclarationFactory.Create(method);
        a.Should().Throw<InvalidOperationException>().WithMessage("Unexpected type: ClassDeclarationSyntax");
    }
}
