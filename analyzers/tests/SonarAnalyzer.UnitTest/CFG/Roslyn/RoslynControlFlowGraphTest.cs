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
using Microsoft.CodeAnalysis.Operations;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SonarAnalyzer.CFG.Roslyn;
using StyleCop.Analyzers.Lightup;
using FlowAnalysis = Microsoft.CodeAnalysis.FlowAnalysis;

namespace SonarAnalyzer.UnitTest.CFG.Roslyn
{
    [TestClass]
    public class RoslynControlFlowGraphTest
    {
        [TestMethod]
        public void IsAvailable_IsTrue() =>
            ControlFlowGraph.IsAvailable.Should().BeTrue();

        [TestMethod]
        public void Create_ReturnsCfg_CS()
        {
            const string code = @"
public class Sample
{
    public void Method()
    {
        return 42;
    }
}";
            TestHelper.CompileCfg(code).Should().NotBeNull();
            var (tree, semanticModel) = TestHelper.Compile(code);
            var declaration = tree.GetRoot().DescendantNodes().OfType<Microsoft.CodeAnalysis.CSharp.Syntax.MethodDeclarationSyntax>().Single();
            ControlFlowGraph.CreateCore(semanticModel.GetOperation(declaration)).Should().NotBeNull();
        }

        [TestMethod]
        public void Create_ReturnsCfg_VB()
        {
            const string code = @"
Public Class Sample
    Public Function Method() As Integer
        Return 42
    End Function
End Class";
            TestHelper.CompileCfg(code, false).Should().NotBeNull();
            var (tree, semanticModel) = TestHelper.Compile(code, false);
            var declaration = tree.GetRoot().DescendantNodes().Single(x => x.RawKind == (int)Microsoft.CodeAnalysis.VisualBasic.SyntaxKind.FunctionBlock);
            ControlFlowGraph.CreateCore(semanticModel.GetOperation(declaration)).Should().NotBeNull();
        }

        [TestMethod]
        public void ValidateReflection()
        {
            const string code = @"
public class Sample
{
    public void Method()
    {
        System.Action a = () => { };
        return LocalMethod();

        int LocalMethod() => 42;
    }
}";
            var cfg = TestHelper.CompileCfg(code);
            cfg.Should().NotBeNull();
            cfg.Root.Should().NotBeNull();
            cfg.Blocks.Should().NotBeNull().And.HaveCount(3); // Enter, Instructions, Exit
            cfg.OriginalOperation.Should().NotBeNull().And.BeAssignableTo<IMethodBodyOperation>();
            cfg.LocalFunctions.Should().HaveCount(1);
            cfg.GetLocalFunctionControlFlowGraph(cfg.LocalFunctions.Single()).Should().NotBeNull();
            var anonymousFunction = cfg.Blocks.SelectMany(x => x.Operations).SelectMany(x => x.DescendantsAndSelf()).OfType<FlowAnalysis.IFlowAnonymousFunctionOperation>().Single();
            cfg.GetAnonymousFunctionControlFlowGraph(IFlowAnonymousFunctionOperationWrapper.FromOperation(anonymousFunction)).Should().NotBeNull();
        }
    }
}
