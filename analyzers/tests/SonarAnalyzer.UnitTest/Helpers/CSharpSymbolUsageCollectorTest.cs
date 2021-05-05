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
    private int Field = 42;
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
            var fooMethod = firstTree.GetRoot().DescendantNodes().OfType<MethodDeclarationSyntax>().Single();
            var firstCompilationSemanticModel = firstCompilation.GetSemanticModel(firstTree);
            var firstCompilationFieldSymbol = firstCompilationSemanticModel.GetSymbolInfo(fooMethod.DescendantNodes().OfType<ReturnStatementSyntax>().Single().Expression).Symbol;
            var firstCompilationKnownSymbols = new List<ISymbol> { firstCompilationFieldSymbol };

            // compilation matches semantic model and syntax node
            var firstCompilationUsageCollector = new CSharpSymbolUsageCollector(firstCompilation, firstCompilationKnownSymbols);
            firstCompilationUsageCollector.Visit(fooMethod);
            firstCompilationUsageCollector.UsedSymbols.Should().NotBeEmpty();
            var firstCompilationUsedSymbols = firstCompilationUsageCollector.UsedSymbols;

            // compilation doesn't match syntax node, since it belongs to another compilation
            var secondTree = secondCompilation.SyntaxTrees.Single();
            var barMethod = secondTree.GetRoot().DescendantNodes().OfType<MethodDeclarationSyntax>().Single();
            firstCompilationUsageCollector.Visit(barMethod);
            firstCompilationUsageCollector.UsedSymbols.Should().BeEquivalentTo(firstCompilationUsedSymbols);
        }
    }
}
