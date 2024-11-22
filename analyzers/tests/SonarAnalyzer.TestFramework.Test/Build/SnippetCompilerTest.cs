/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2014-2024 SonarSource SA
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

namespace SonarAnalyzer.Test.TestFramework.Tests.Build;

[TestClass]
public class SnippetCompilerTest
{
    [TestMethod]
    public void AllowsUnsafe_CS()
    {
        const string code = """
            public class Sample
            {
                public void Main()
                {
                    int i = 0;
                    unsafe
                    {
                        i = 42;
                    }
                }
            }
            """;
        var sut = new SnippetCompiler(code);
        sut.SyntaxTree.Should().NotBeNull();
        sut.SemanticModel.Should().NotBeNull();
    }

    [TestMethod]
    public void ImportsDefaultNamespaces_VB()
    {
        const string code = """
            ' No Using statement for System.Exception
            Public Class Sample
                Inherits Exception

            End Class
            """;
        var sut = new SnippetCompiler(code, false, AnalyzerLanguage.VisualBasic);
        sut.SyntaxTree.Should().NotBeNull();
        sut.SemanticModel.Should().NotBeNull();
    }

    [TestMethod]
    public void IgnoreErrors_CreatesCompilation()
    {
        const string code = """
            public class Sample
            {
            // Error CS1519 Invalid token '{' in class, record, struct, or interface member declaration
            // Error CS1513 } expected
            {
            """;
        var sut = new SnippetCompiler(code, true, AnalyzerLanguage.CSharp);
        sut.SyntaxTree.Should().NotBeNull();
        sut.SemanticModel.Should().NotBeNull();
    }

    [TestMethod]
    public void ValidCode_DoNotIgnoreErrors_CreatesCompilation()
    {
        const string code = """
            public class Sample
            {
                public void Main()
                {
                }
            }
            """;
        var sut = new SnippetCompiler(code);
        sut.SyntaxTree.Should().NotBeNull();
        sut.SemanticModel.Should().NotBeNull();
    }

    [TestMethod]
    public void CodeWithWarning_DoNotIgnoreErrors_CreatesCompilation()
    {
        const string code = """
            public class Sample
            {
                public void Main()
                {
                    // Warning CS0219 The variable 'i' is assigned but its value is never used
                    int i = 42;
                    // Warning CS1030 #warning: 'Show CS1030' on this line
            #warning Show CS1030 on this line
                }
            }
            """;
        // Severity=Warning is not an error and should not throw
        var sut = new SnippetCompiler(code);
        sut.SyntaxTree.Should().NotBeNull();
        sut.SemanticModel.Should().NotBeNull();
    }

    [TestMethod]
    public void InvalidCode_DoNotIgnoreErrors_Throws()
    {
        const string code = """
            public class Sample
            {
            // Error CS1519 Invalid token '{' in class, record, struct, or interface member declaration
            // Error CS1513 } expected
            {
            """;
        using var log = new LogTester();
        Func<SnippetCompiler> f = () => new SnippetCompiler(code);
        f.Should().Throw<InvalidOperationException>();
        log.AssertContain("CS1519 Line: 4: Invalid token '{' in class, record, struct, or interface member declaration");
        log.AssertContain("CS1513 Line: 4: } expected");
    }

    [TestMethod]
    public void GetNamespaceSymbol_NamespaceExists_ReturnsSymbol()
    {
        const string code = """
            namespace Test {
                public class Sample { }
            }
            """;
        var sut = new SnippetCompiler(code);
        var namespaceSymbol = sut.GetNamespaceSymbol("Test");
        namespaceSymbol.Name.Should().Be("Test");
    }

    [TestMethod]
    public void GetNamespaceSymbol_NestedNamespaces_ReturnsSymbols()
    {
        const string code = """
            namespace Test {
                namespace Nested {
                    public class Sample { }
                }
            }
            """;
        var sut = new SnippetCompiler(code);
        var namespaceSymbol = sut.GetNamespaceSymbol("Test");
        namespaceSymbol.Name.Should().Be("Test");
        var nestedNamespaceSymbol = sut.GetNamespaceSymbol("Nested");
        nestedNamespaceSymbol.Name.Should().Be("Nested");
    }

    [TestMethod]
    public void GetNamespaceSymbol_NestedNamespacesWithDot_ReturnsSymbols()
    {
        const string code = """
            namespace Test.Nested {
                 public class Sample { }
            }
            """;
        var sut = new SnippetCompiler(code);
        // Searching in nested namespaces of the form Namespace.NestedNamespace
        // is not supported by this method. We can get back the symbol of the most nested namespace.
        var namespaceSymbol = sut.GetNamespaceSymbol("Nested");
        namespaceSymbol.Name.Should().Be("Nested");
    }

    [TestMethod]
    public void GetNamespaceSymbol_FileScopedNamespace_ReturnsSymbol()
    {
        const string code = """
            namespace Test;
            public class Sample { }

            """;
        var sut = new SnippetCompiler(code);
        var namespaceSymbol = sut.GetNamespaceSymbol("Test");
        namespaceSymbol.Name.Should().Be("Test");
    }

    [TestMethod]
    public void GetMethodDeclaration_CS()
    {
        var sut = new SnippetCompiler("""
            public class Sample
            {
                public int WrongOne() => 42;
                public int Method() => 42;
            }
            """);
        sut.GetMethodDeclaration("Sample.Method").Should().NotBeNull().And.Subject.ToString().Should().Contain("Method()");
    }

    [TestMethod]
    public void GetMethodDeclaration_VB()
    {
        var sut = new SnippetCompiler("""
            Public Class Sample
                Public Sub WrongOne() : End Sub
                Public Sub Method1() : End Sub
                Public Function Method2() : End Function
            End Class
            """, false, AnalyzerLanguage.VisualBasic);
        sut.GetMethodDeclaration("Sample.Method1").Should().NotBeNull().And.Subject.ToString().Should().Contain("Method1()");
        sut.GetMethodDeclaration("Sample.Method2").Should().NotBeNull().And.Subject.ToString().Should().Contain("Method2()");
    }

    [TestMethod]
    public void GetTypeSymbol_CS()
    {
        var sut = new SnippetCompiler("public class Sample { public class Nested { } }");
        var type = sut.GetTypeSymbol("Nested");
        type.Should().NotBeNull();
        type.Name.Should().Be("Nested");
    }

    [TestMethod]
    public void GetTypeSymbol_VB()
    {
        var sut = new SnippetCompiler("Public Class Sample : Public Class Nested : End Class : End Class", false, AnalyzerLanguage.VisualBasic);
        var type = sut.GetTypeSymbol("Nested");
        type.Should().NotBeNull();
        type.Name.Should().Be("Nested");
    }
}
