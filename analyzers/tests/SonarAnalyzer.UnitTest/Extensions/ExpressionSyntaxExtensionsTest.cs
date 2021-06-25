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

using System.Linq;
using FluentAssertions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SonarAnalyzer.Extensions;
using SonarAnalyzer.UnitTest.MetadataReferences;

namespace SonarAnalyzer.UnitTest.Extensions
{
    [TestClass]
    public class ExpressionSyntaxExtensionsTest
    {
        [TestMethod]
        [DataTestMethod]
        [DataRow("null", false)]
        [DataRow("var o = new object();", true)]
        [DataRow("int? x = 1", true)]
        [DataRow("int x = 1;", false)]
        public void CanBeNull(string code, bool expected)
        {
            var (expression, semanticModel) = Compile(code);

            expression.CanBeNull(semanticModel).Should().Be(expected);
        }

        private static (ExpressionSyntax expression, SemanticModel semanticModel) Compile(string code)
        {
            var tree = CSharpSyntaxTree.ParseText(code);
            var compilation = CSharpCompilation.Create("TempAssembly.dll")
                                               .AddSyntaxTrees(tree)
                                               .AddReferences(MetadataReferenceFacade.ProjectDefaultReferences);

            var semanticModel = compilation.GetSemanticModel(tree);

            return (tree.GetRoot().DescendantNodes().OfType<ExpressionSyntax>().First(), semanticModel);
        }
    }
}
