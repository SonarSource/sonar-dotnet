/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2020 SonarSource SA
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
    public class ConstantValueFinderTest
    {
        [TestMethod]
        public void FieldDeclaredInAnotherSyntaxTree()
        {
            const string code1 = @"
public partial class Sample
{
    private static int Original = 42;
    private int Field = Original;
}";
            const string code2 = @"
public partial class Sample
{
    public int Method()
    {
        return Field;
    }
}";
            var compilation = SolutionBuilder.Create().AddProject(AnalyzerLanguage.CSharp, createExtraEmptyFile: false)
                .AddSnippet(code1)
                .AddSnippet(code2)
                .GetCompilation();
            var tree = compilation.SyntaxTrees.Single(x => x.GetRoot().DescendantNodes().OfType<MethodDeclarationSyntax>().Any());
            var returnExpression = tree.GetRoot().DescendantNodes().OfType<ReturnStatementSyntax>().Single().Expression;
            var finder = new CSharpConstantValueFinder(compilation.GetSemanticModel(tree));
            finder.FindConstant(returnExpression).Should().Be(42);
        }
    }
}
