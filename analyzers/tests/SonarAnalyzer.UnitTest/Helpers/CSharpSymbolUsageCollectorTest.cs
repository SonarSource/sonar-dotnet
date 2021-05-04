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

using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SonarAnalyzer.Common;
using SonarAnalyzer.Helpers;
using SonarAnalyzer.UnitTest.TestFramework;

namespace SonarAnalyzer.UnitTest.Helpers
{
    [TestClass]
    public class CSharpSymbolUsageCollectorTest
    {
        [TestMethod]
        public void VerifyUsagesBeingCollectedOnMatchingSyntaxNodes()
        {
            const string firstSnippet = @"
public class Foo
{
    private int Field = 42;

    public int FooMethod(int arg)
    {
        Field += arg;
        return Field;
    }
}";
            const string secondSnippet = @"
public class Bar
{
    public int BarMethod()
    {
        return Field;
    }
}";
            var firstCompilation = SolutionBuilder.Create().AddProject(AnalyzerLanguage.CSharp, createExtraEmptyFile: false)
                .AddSnippet(firstSnippet)
                .GetCompilation();
            var secondCompilation = SolutionBuilder.Create().AddProject(AnalyzerLanguage.CSharp, createExtraEmptyFile: false)
                .AddSnippet(secondSnippet)
                .GetCompilation();

            var firstTree = firstCompilation.SyntaxTrees.Single();
            var fooMethodDecl = firstTree.GetRoot().DescendantNodes().OfType<MethodDeclarationSyntax>().Single();
            var semanticModel = firstCompilation.GetSemanticModel(firstTree);

            var fieldSymbol = semanticModel.GetSymbolInfo(fooMethodDecl.DescendantNodes().OfType<ReturnStatementSyntax>().Single().Expression).Symbol;
            var knownSymbolsFirstCompilation = new List<ISymbol> { fieldSymbol };

            // compilation matches semantic model and syntax node
            var usageCollectorFirstCompilation = new CSharpSymbolUsageCollector(firstCompilation, knownSymbolsFirstCompilation);
            usageCollectorFirstCompilation.Visit(fooMethodDecl);
            usageCollectorFirstCompilation.UsedSymbols.Should().NotBeEmpty();
            var originallyUsedSymbols = usageCollectorFirstCompilation.UsedSymbols;

            var secondTree = secondCompilation.SyntaxTrees.Single();
            var barMethodDecl = secondTree.GetRoot().DescendantNodes().OfType<MethodDeclarationSyntax>().Single();
            // compilation doesn't match syntax node, since it belongs to another compilation
            usageCollectorFirstCompilation.Visit(barMethodDecl);
            usageCollectorFirstCompilation.UsedSymbols.Should().BeEquivalentTo(originallyUsedSymbols);
        }
    }
}
