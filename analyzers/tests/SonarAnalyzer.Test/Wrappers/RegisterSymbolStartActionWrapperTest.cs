/*
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

using SonarAnalyzer.ShimLayer.AnalysisContext;

namespace SonarAnalyzer.Test.Wrappers;

[TestClass]
public class RegisterSymbolStartActionWrapperTest
{
    public class TestDiagnosticAnalyzer : DiagnosticAnalyzer
    {
        public TestDiagnosticAnalyzer(Action<ShimLayer.AnalysisContext.SymbolStartAnalysisContext> action, SymbolKind symbolKind)
        {
            Action = action;
            SymbolKind = symbolKind;
        }
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
            ImmutableArray.Create(new DiagnosticDescriptor("TEST", "Test", "Test", "Test", DiagnosticSeverity.Warning, true));

        public Action<ShimLayer.AnalysisContext.SymbolStartAnalysisContext> Action { get; }
        public SymbolKind SymbolKind { get; }

        public override void Initialize(Microsoft.CodeAnalysis.Diagnostics.AnalysisContext context) =>
            context.RegisterCompilationStartAction(start =>
            {
                start.RegisterSymbolStartAction(c =>
                {
                }, SymbolKind.Method);
                CompilationStartAnalysisContextExtensions.RegisterSymbolStartAction(start, Action, SymbolKind);
            });
    }
    [TestMethod]
    public async Task Test()
    {
        var code = """
            public class C
            {
                int i = 0;
                public void M()
                {
                    ToString();
                }
            }
            """;
        var snippet = new SnippetCompiler(code);
        var visitedCodeBlocks = new List<string>();
        var compilation = snippet.Compilation.WithAnalyzers(ImmutableArray.Create<DiagnosticAnalyzer>(
            new TestDiagnosticAnalyzer(symbolStart =>
            {
                symbolStart.RegisterCodeBlockAction(block =>
                {
                    var node = block.CodeBlock.ToString();
                    visitedCodeBlocks.Add(node.Substring(0, node.IndexOf('\n') is var pos and >= 0 ? pos : node.Length));
                });
            }, SymbolKind.NamedType)));
        var diagnostics = await compilation.GetAnalyzerDiagnosticsAsync();
        visitedCodeBlocks.Should().BeEquivalentTo("int i = 0;", "public void M()");
    }
}
