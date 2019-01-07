/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2019 SonarSource SA
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
    public class SymbolHelper_IsExtensionOn
    {
        internal const string TestInput = @"
using System.Linq;

namespace NS
{
  public static class Helper
  {
    public static void ToVoid(this int self){}
  }
  public class Class
  {
    public static void TestMethod()
    {
      new int[] { 0, 1, 2 }.Any();
      Enumerable.Any(new int[] { 0, 1, 2 });

      new int[] { 0, 1, 2 }.Clone();

      new int[] { 0, 1, 2 }.Cast<object>();

      1.ToVoid();
    }
  }
}
";

        private SemanticModel semanticModel;
        private List<StatementSyntax> statements;

        [TestInitialize]
        public void Compile()
        {
            var compilation = SolutionBuilder
                .Create()
                .AddProject(AnalyzerLanguage.CSharp, createExtraEmptyFile: false)
                .AddSnippet(TestInput)
                .GetCompilation();

            var tree = compilation.SyntaxTrees.First();
            this.semanticModel = compilation.GetSemanticModel(tree);
            this.statements = tree.GetRoot().DescendantNodes()
                .OfType<MethodDeclarationSyntax>()
                .First(m => m.Identifier.ValueText == "TestMethod").Body
                .DescendantNodes()
                .OfType<StatementSyntax>().ToList();
        }

        [TestMethod]
        public void Symbol_IsExtensionOnIEnumerable()
        {
            GetMethodSymbolForIndex(3).IsExtensionOn(KnownType.System_Collections_IEnumerable)
                .Should().BeTrue();

            GetMethodSymbolForIndex(2).IsExtensionOn(KnownType.System_Collections_IEnumerable)
                .Should().BeFalse();
            GetMethodSymbolForIndex(1).IsExtensionOn(KnownType.System_Collections_IEnumerable)
                .Should().BeFalse();
        }

        [TestMethod]
        public void Symbol_IsExtensionOnGenericIEnumerable()
        {
            GetMethodSymbolForIndex(0).IsExtensionOn(KnownType.System_Collections_Generic_IEnumerable_T)
                .Should().BeTrue();
            GetMethodSymbolForIndex(1).IsExtensionOn(KnownType.System_Collections_Generic_IEnumerable_T)
                .Should().BeTrue();

            GetMethodSymbolForIndex(2).IsExtensionOn(KnownType.System_Collections_Generic_IEnumerable_T)
                .Should().BeFalse();
            GetMethodSymbolForIndex(3).IsExtensionOn(KnownType.System_Collections_Generic_IEnumerable_T)
                .Should().BeFalse();
        }

        [TestMethod]
        public void Symbol_IsExtensionOnInt()
        {
            GetMethodSymbolForIndex(4).IsExtensionOn(KnownType.System_Int32)
                .Should().BeTrue();
            GetMethodSymbolForIndex(2).IsExtensionOn(KnownType.System_Int32)
                .Should().BeFalse();
        }

        private IMethodSymbol GetMethodSymbolForIndex(int index)
        {
            var statement = (ExpressionStatementSyntax)this.statements[index];
            var methodSymbol = this.semanticModel.GetSymbolInfo(statement.Expression).Symbol as IMethodSymbol;
            return methodSymbol;
        }
    }
}
