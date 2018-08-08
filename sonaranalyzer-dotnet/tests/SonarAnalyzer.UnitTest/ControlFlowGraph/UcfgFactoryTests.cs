/*
* SonarAnalyzer for .NET
* Copyright (C) 2015-2018 SonarSource SA
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

extern alias csharp;
using System;
using System.Linq;
using csharp::SonarAnalyzer.ControlFlowGraph.CSharp;
using FluentAssertions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using SonarAnalyzer.ControlFlowGraph;

namespace SonarAnalyzer.UnitTest.ControlFlowGraph
{
    [TestClass]
    public class UcfgFactoryTests
    {
        [TestMethod]
        public void Create_DecoratesAndReThrowsExceptions()
        {
            // Arrange
            const string code = @"
namespace Ns1
{
  class MyClass
  {
    void DoStuff(){}
  }
}
";
            var (syntaxTree, semanticModel) = TestHelper.Compile(code);
            var factory = new UcfgFactory(semanticModel);

            var methodNode = syntaxTree.GetRoot()
                .DescendantNodes()
                .OfType<MethodDeclarationSyntax>()
                .First();
            var methodSymbol = (IMethodSymbol)semanticModel.GetDeclaredSymbol(methodNode);

            var mockCFG = new Mock<IControlFlowGraph>(MockBehavior.Strict); // strict -> throw on any usage

            Action act = () => factory.Create(methodNode, methodSymbol, mockCFG.Object);

            // Act
            act.Should().ThrowExactly<UcfgException>().And.Message.Should().ContainAll("Error processing method: DoStuff", "Inner exception: ");

        }
    }
}
