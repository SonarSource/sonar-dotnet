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
            const string code1 = @"
public partial class Sample
{
    private int Field = 42;

    public int Foo(int arg)
    {
        Field += arg;
        return Field;
    }
}";
            const string code2 = @"
public partial class Sample
{
    public int Bar()
    {
        return Field;
    }
}";
            var compilation1 = SolutionBuilder.Create().AddProject(AnalyzerLanguage.CSharp, createExtraEmptyFile: false)
                .AddSnippet(code1)
                .GetCompilation();
            var compilation2 = SolutionBuilder.Create().AddProject(AnalyzerLanguage.CSharp, createExtraEmptyFile: false)
                .AddSnippet(code2)
                .GetCompilation();

            var tree1 = compilation1.SyntaxTrees.Single(x => x.GetRoot().DescendantNodes().OfType<MethodDeclarationSyntax>().Any());
            var fooMethodDecl = tree1.GetRoot().DescendantNodes().OfType<MethodDeclarationSyntax>().Single();
            var returnExpression = fooMethodDecl.DescendantNodes().OfType<ReturnStatementSyntax>().Single().Expression;
            var semanticModel = compilation1.GetSemanticModel(tree1);

            var fieldSymbol = semanticModel.GetSymbolInfo(returnExpression).Symbol;
            var knownSymbols = new List<ISymbol> { fieldSymbol };

            // compilation matches semantic model and syntax node
            var usageCollector = new CSharpSymbolUsageCollector(compilation1, knownSymbols);
            usageCollector.Visit(fooMethodDecl);
            usageCollector.UsedSymbols.Should().NotBeEmpty();
            var originallyUsedSymbols = usageCollector.UsedSymbols;

            var tree2 = compilation2.SyntaxTrees.Single(x => x.GetRoot().DescendantNodes().OfType<MethodDeclarationSyntax>().Any());
            var barMethodDecl = tree2.GetRoot().DescendantNodes().OfType<MethodDeclarationSyntax>().Single();
            // compilation doesn't match syntax node, since it belongs to another compilation
            usageCollector.Visit(barMethodDecl);
            usageCollector.UsedSymbols.Should().BeEquivalentTo(originallyUsedSymbols);
        }
    }
}
