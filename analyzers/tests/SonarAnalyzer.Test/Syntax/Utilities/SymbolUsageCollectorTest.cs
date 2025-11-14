/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2014-2025 SonarSource Sàrl
 * mailto:info AT sonarsource DOT com
 * This program is free software; you can redistribute it and/or
 * modify it under the terms of the Sonar Source-Available License Version 1, as published by SonarSource Sàrl.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
 * See the Sonar Source-Available License for more details.
 *
 * You should have received a copy of the Sonar Source-Available License
 * along with this program; if not, see https://sonarsource.com/license/ssal/
 */

using Microsoft.CodeAnalysis.CSharp.Syntax;
using SonarAnalyzer.CSharp.Syntax.Utilities;

namespace SonarAnalyzer.Test.Syntax.Utilities;

[TestClass]
public class SymbolUsageCollectorTest
{
    [TestMethod]
    public void VerifyUsagesBeingCollectedOnMatchingSyntaxNodes()
    {
        const string firstSnippet = """
            public class Foo
            {
                private int Field = 42;
                public int FooMethod(int arg)
                {
                    Field += arg;
                    return Field;
                }
            }
            """;
        const string secondSnippet = """
            public class Bar
            {
                private int Field = 42;
                public int BarMethod()
                {
                    return Field;
                }
            }
            """;
        var firstCompilation = SolutionBuilder.Create().AddProject(AnalyzerLanguage.CSharp).AddSnippet(firstSnippet).GetCompilation();
        var secondCompilation = SolutionBuilder.Create().AddProject(AnalyzerLanguage.CSharp).AddSnippet(secondSnippet).GetCompilation();

        var firstTree = firstCompilation.SyntaxTrees.Single();
        var fooMethod = firstTree.Single<MethodDeclarationSyntax>();
        var firstCompilationModel = firstCompilation.GetSemanticModel(firstTree);
        var firstCompilationFieldSymbol = firstCompilationModel.GetSymbolInfo(fooMethod.DescendantNodes().OfType<ReturnStatementSyntax>().Single().Expression).Symbol;
        var firstCompilationKnownSymbols = new List<ISymbol> { firstCompilationFieldSymbol };

        // compilation matches semantic model and syntax node
        var firstCompilationUsageCollector = new SymbolUsageCollector(firstCompilation, firstCompilationKnownSymbols);
        firstCompilationUsageCollector.Visit(fooMethod);
        firstCompilationUsageCollector.UsedSymbols.Should().NotBeEmpty();
        var firstCompilationUsedSymbols = firstCompilationUsageCollector.UsedSymbols;

        // compilation doesn't match syntax node, since it belongs to another compilation
        var secondTree = secondCompilation.SyntaxTrees.Single();
        var barMethod = secondTree.Single<MethodDeclarationSyntax>();
        firstCompilationUsageCollector.Visit(barMethod);
        firstCompilationUsageCollector.UsedSymbols.Should().BeEquivalentTo(firstCompilationUsedSymbols);
    }
}
