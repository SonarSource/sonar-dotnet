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

using SonarAnalyzer.ShimLayer.AnalysisContext;
using CS = Microsoft.CodeAnalysis.CSharp;
using VB = Microsoft.CodeAnalysis.VisualBasic;

namespace SonarAnalyzer.Test.Wrappers;

[TestClass]
public class RegisterSymbolStartActionWrapperTest
{
    [TestMethod]
    public async Task RegisterSymbolStartAction_SymbolStartProperties()
    {
        var snippet = new SnippetCompiler("""
            public class C
            {
                int i = 0;
                public void M() => ToString();
            }
            """);
        var symbolStartWasCalled = false;
        var compilation = snippet.Compilation.WithAnalyzers([new TestDiagnosticAnalyzer(symbolStart =>
            {
                symbolStart.CancellationToken.IsCancellationRequested.Should().BeFalse();
                symbolStart.Compilation.SyntaxTrees.Should().ContainSingle();
                symbolStart.Options.Should().NotBeNull();
                symbolStart.Symbol.Should().BeAssignableTo<INamedTypeSymbol>().Which.Name.Should().Be("C");
                symbolStartWasCalled = true;
            }, SymbolKind.NamedType)]);
        var diagnostics = await compilation.GetAnalyzerDiagnosticsAsync();
        symbolStartWasCalled.Should().BeTrue();
        diagnostics.Should().BeEmpty();
    }

    [TestMethod]
    public async Task RegisterSymbolStartAction_RegisterCodeBlockAction()
    {
        var snippet = new SnippetCompiler("""
            public class C
            {
                int i = 0;
                public void M() => ToString();
            }
            """);
        var visitedCodeBlocks = new List<string>();
        var compilation = snippet.Compilation.WithAnalyzers([new TestDiagnosticAnalyzer(symbolStart =>
            {
                symbolStart.RegisterCodeBlockAction(block =>
                {
                    var node = block.CodeBlock.ToString();
                    visitedCodeBlocks.Add(node);
                });
            }, SymbolKind.NamedType)]);
        var diagnostics = await compilation.GetAnalyzerDiagnosticsAsync();
        diagnostics.Should().BeEmpty();
        visitedCodeBlocks.Should().BeEquivalentTo("int i = 0;", "public void M() => ToString();");
    }

    [TestMethod]
    public async Task RegisterSymbolStartAction_RegisterCodeBlockAction_ConditionalRegistration()
    {
        var snippet = new SnippetCompiler("""
            public class C
            {
                int i = 0;
                public void M() => ToString();
            }
            public class D
            {
                int j = 0;
                public void N() => ToString();
            }
            """);
        var visitedCodeBlocks = new List<string>();
        var compilation = snippet.Compilation.WithAnalyzers([new TestDiagnosticAnalyzer(symbolStart =>
            {
                if (symbolStart.Symbol.Name == "C")
                {
                    return;
                }

                symbolStart.RegisterCodeBlockAction(block =>
                {
                    var node = block.CodeBlock.ToString();
                    visitedCodeBlocks.Add(node);
                });
            }, SymbolKind.NamedType)]);
        var diagnostics = await compilation.GetAnalyzerDiagnosticsAsync();
        diagnostics.Should().BeEmpty();
        visitedCodeBlocks.Should().BeEquivalentTo("int j = 0;", "public void N() => ToString();");
    }

    [TestMethod]
    public async Task RegisterSymbolStartAction_RegisterCodeBlockStartAction_CS()
    {
        var snippet = new SnippetCompiler("""
            public class C
            {
                int i = 0;
                public void M() => ToString();
            }
            """);
        var visited = new List<string>();
        var compilation = snippet.Compilation.WithAnalyzers([new TestDiagnosticAnalyzer(symbolStart =>
            {
                symbolStart.RegisterCodeBlockStartAction<CS.SyntaxKind>(blockStart =>
                {
                    var node = blockStart.CodeBlock.ToString();
                    visited.Add(node);
                    blockStart.RegisterSyntaxNodeAction(nodeContext => visited.Add(nodeContext.Node.ToString()), CS.SyntaxKind.InvocationExpression);
                });
            }, SymbolKind.NamedType)]);
        var diagnostics = await compilation.GetAnalyzerDiagnosticsAsync();
        diagnostics.Should().BeEmpty();
        visited.Should().BeEquivalentTo("int i = 0;", "public void M() => ToString();", "ToString()");
    }

    [TestMethod]
    public async Task RegisterSymbolStartAction_RegisterCodeBlockStartAction_VB()
    {
        var snippet = new SnippetCompiler("""
            Public Class C
                Private i As Integer = 0

                Public Sub M()
                    Call ToString()
                End Sub
            End Class
            """, ignoreErrors: false, AnalyzerLanguage.VisualBasic);
        var visited = new List<string>();
        var compilation = snippet.Compilation.WithAnalyzers([new TestDiagnosticAnalyzer(symbolStart =>
            {
                symbolStart.RegisterCodeBlockStartAction<VB.SyntaxKind>(blockStart =>
                {
                    var node = blockStart.CodeBlock.ToString();
                    visited.Add(node);
                    blockStart.RegisterSyntaxNodeAction(nodeContext => visited.Add(nodeContext.Node.ToString()), VB.SyntaxKind.InvocationExpression);
                });
            }, SymbolKind.NamedType)]);
        var diagnostics = await compilation.GetAnalyzerDiagnosticsAsync();
        diagnostics.Should().BeEmpty();
        visited.Should().BeEquivalentTo([
            "Private i As Integer = 0",
            """
            Public Sub M()
                    Call ToString()
                End Sub
            """,
            "ToString()"]);
    }

    [TestMethod]
    public async Task RegisterSymbolStartAction_RegisterOperationAction()
    {
        var snippet = new SnippetCompiler("""
            public class C
            {
                int i = 0;
                public void M()
                {
                    ToString();
                }
            }
            """);
        var visited = new List<string>();
        var compilation = snippet.Compilation.WithAnalyzers([new TestDiagnosticAnalyzer(symbolStart =>
            {
                symbolStart.RegisterOperationAction(operationContext =>
                {
                    var operation = operationContext.Operation.Syntax.ToString();
                    visited.Add(operation);
                }, [OperationKind.Invocation]);
            }, SymbolKind.NamedType)]);
        var diagnostics = await compilation.GetAnalyzerDiagnosticsAsync();
        diagnostics.Should().BeEmpty();
        visited.Should().BeEquivalentTo("ToString()");
    }

    [TestMethod]
    public async Task RegisterSymbolStartAction_RegisterOperationBlockAction()
    {
        var snippet = new SnippetCompiler("""
            public class C
            {
                int i = 0;
                public void M() => ToString();
            }
            """);
        var visited = new List<string>();
        var compilation = snippet.Compilation.WithAnalyzers([new TestDiagnosticAnalyzer(symbolStart =>
            {
                symbolStart.RegisterOperationBlockAction(operationBlockContext =>
                {
                    var operation = operationBlockContext.OperationBlocks.First().Syntax.ToString();
                    visited.Add(operation);
                });
            }, SymbolKind.NamedType)]);
        var diagnostics = await compilation.GetAnalyzerDiagnosticsAsync();
        diagnostics.Should().BeEmpty();
        visited.Should().BeEquivalentTo("= 0", "=> ToString()");
    }

    [TestMethod]
    public async Task RegisterSymbolStartAction_RegisterOperationBlockStartAction()
    {
        var snippet = new SnippetCompiler("""
            public class C
            {
                int i = 0;
                public void M() => ToString();
            }
            """);
        var visited = new List<string>();
        var compilation = snippet.Compilation.WithAnalyzers([new TestDiagnosticAnalyzer(symbolStart =>
            {
                symbolStart.RegisterOperationBlockStartAction(operationBlockStartContext =>
                {
                    var operation = operationBlockStartContext.OperationBlocks.First().Syntax.ToString();
                    visited.Add(operation);
                    operationBlockStartContext.RegisterOperationAction(operationContext => visited.Add(operationContext.Operation.Syntax.ToString()), OperationKind.Invocation);
                });
            }, SymbolKind.NamedType)]);
        var diagnostics = await compilation.GetAnalyzerDiagnosticsAsync();
        diagnostics.Should().BeEmpty();
        visited.Should().BeEquivalentTo("= 0", "=> ToString()", "ToString()");
    }

    [TestMethod]
    public async Task RegisterSymbolStartAction_RegisterRegisterSymbolEndAction()
    {
        var snippet = new SnippetCompiler("""
            public class C
            {
                int i = 0;
                public void M() => ToString();
            }
            """);
        var visited = new List<string>();
        var compilation = snippet.Compilation.WithAnalyzers([new TestDiagnosticAnalyzer(symbolStart =>
            {
                symbolStart.RegisterSymbolEndAction(symbolContext =>
                {
                    var symbolName = symbolContext.Symbol.Name;
                    visited.Add(symbolName);
                });
            }, SymbolKind.NamedType)]);
        var diagnostics = await compilation.GetAnalyzerDiagnosticsAsync();
        diagnostics.Should().BeEmpty();
        visited.Should().BeEquivalentTo("C");
    }

    [TestMethod]
    public async Task RegisterSymbolStartAction_RegisterSyntaxNodeAction_CS()
    {
        var snippet = new SnippetCompiler("""
            public class C
            {
                int i = 0;
                public void M() => ToString();
            }
            """);
        var visited = new List<string>();
        var compilation = snippet.Compilation.WithAnalyzers([new TestDiagnosticAnalyzer(symbolStart =>
            {
                symbolStart.RegisterSyntaxNodeAction(syntaxNodeContext =>
                {
                    var nodeName = syntaxNodeContext.Node.ToString();
                    visited.Add(nodeName);
                }, CS.SyntaxKind.InvocationExpression, CS.SyntaxKind.EqualsValueClause);
            }, SymbolKind.NamedType)]);
        var diagnostics = await compilation.GetAnalyzerDiagnosticsAsync();
        diagnostics.Should().BeEmpty();
        visited.Should().BeEquivalentTo("= 0", "ToString()");
    }

    [TestMethod]
    public async Task RegisterSymbolStartAction_RegisterSyntaxNodeAction_VB()
    {
        var snippet = new SnippetCompiler("""
            Public Class C
                Private i As Integer = 0

                Public Sub M()
                    Call ToString()
                End Sub
            End Class
            """, ignoreErrors: false, AnalyzerLanguage.VisualBasic);
        var visited = new List<string>();
        var compilation = snippet.Compilation.WithAnalyzers([new TestDiagnosticAnalyzer(symbolStart =>
            {
                symbolStart.RegisterSyntaxNodeAction(syntaxNodeContext =>
                {
                    var nodeName = syntaxNodeContext.Node.ToString();
                    visited.Add(nodeName);
                }, VB.SyntaxKind.InvocationExpression);
            }, SymbolKind.NamedType)]);
        var diagnostics = await compilation.GetAnalyzerDiagnosticsAsync();
        diagnostics.Should().BeEmpty();
        visited.Should().BeEquivalentTo("ToString()");
    }

#pragma warning disable RS1001 // Missing diagnostic analyzer attribute
#pragma warning disable RS1025 // Configure generated code analysis
#pragma warning disable RS1026 // Enable concurrent execution
    private sealed class TestDiagnosticAnalyzer : DiagnosticAnalyzer
    {
        public Action<SymbolStartAnalysisContextWrapper> Action { get; }
        public SymbolKind SymbolKind { get; }
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
            [AnalysisScaffolding.CreateDescriptor("TEST")];

        public TestDiagnosticAnalyzer(Action<SymbolStartAnalysisContextWrapper> action, SymbolKind symbolKind)
        {
            Action = action;
            SymbolKind = symbolKind;
        }

        public override void Initialize(Microsoft.CodeAnalysis.Diagnostics.AnalysisContext context) =>
            context.RegisterCompilationStartAction(x =>
                CompilationStartAnalysisContextExtensions.RegisterSymbolStartAction(x, Action, SymbolKind));
    }
}
