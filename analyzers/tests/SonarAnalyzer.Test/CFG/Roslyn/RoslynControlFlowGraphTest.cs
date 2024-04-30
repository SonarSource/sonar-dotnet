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

using Microsoft.CodeAnalysis.Operations;
using SonarAnalyzer.CFG.Helpers;
using SonarAnalyzer.CFG.Roslyn;
using StyleCop.Analyzers.Lightup;
using FlowAnalysis = Microsoft.CodeAnalysis.FlowAnalysis;

namespace SonarAnalyzer.Test.CFG.Roslyn;

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
    public int Method()
    {
        return 42;
    }
}";
        TestHelper.CompileCfgCS(code).Should().NotBeNull();
    }

    [TestMethod]
    public void Create_ReturnsCfg_TopLevelStatements()
    {
        const string code = """
            MethodA();
            MethodB();

            void MethodA() { }
            void MethodB() { }
            """;
        TestHelper.CompileCfg(code, AnalyzerLanguage.CSharp, outputKind: OutputKind.ConsoleApplication).Should().NotBeNull();
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
        TestHelper.CompileCfg(code, AnalyzerLanguage.VisualBasic).Should().NotBeNull();
    }

    [TestMethod]
    public void ValidateReflection()
    {
        const string code = @"
public class Sample
{
    public int Method()
    {
        System.Action a = () => { };
        return LocalMethod();

        int LocalMethod() => 42;
    }
}";
        var cfg = TestHelper.CompileCfgCS(code);
        cfg.Should().NotBeNull();
        cfg.Root.Should().NotBeNull();
        cfg.Blocks.Should().NotBeNull().And.HaveCount(3); // Enter, Instructions, Exit
        cfg.OriginalOperation.Should().NotBeNull().And.BeAssignableTo<IMethodBodyOperation>();
        cfg.Parent.Should().BeNull();
        cfg.LocalFunctions.Should().HaveCount(1);

        var localFunctionCfg = cfg.GetLocalFunctionControlFlowGraph(cfg.LocalFunctions.Single(), default);
        localFunctionCfg.Should().NotBeNull();
        localFunctionCfg.Parent.Should().Be(cfg);

        var anonymousFunction = cfg.Blocks.SelectMany(x => x.Operations).SelectMany(x => x.DescendantsAndSelf()).OfType<FlowAnalysis.IFlowAnonymousFunctionOperation>().Single();
        cfg.GetAnonymousFunctionControlFlowGraph(IFlowAnonymousFunctionOperationWrapper.FromOperation(anonymousFunction), default).Should().NotBeNull();
    }

    [TestMethod]
    public void FlowAnonymousFunctionOperations_FindsAll()
    {
        const string code = @"
public class Sample {
    private System.Action<int> Simple(int a)
    {
        var x = 42;
        if (a == 42)
        {
            return (x) => {  };
        }
        return x => {  };
    }
}";
        var cfg = TestHelper.CompileCfgCS(code);
        var anonymousFunctionOperations = SonarAnalyzer.Extensions.ControlFlowGraphExtensions.FlowAnonymousFunctionOperations(cfg).ToList();
        anonymousFunctionOperations.Should().HaveCount(2);
        cfg.GetAnonymousFunctionControlFlowGraph(anonymousFunctionOperations[0], default).Should().NotBeNull();
        cfg.GetAnonymousFunctionControlFlowGraph(anonymousFunctionOperations[1], default).Should().NotBeNull();
    }

    [TestMethod]
    public void RoslynCfgSupportedVersions()
    {
        // We are running on 3 rd major version - it is the minimum requirement
        RoslynHelper.IsRoslynCfgSupported().Should().BeTrue();
        // If we set minimum requirement to 2 - we will able to pass the check even with old MsBuild
        RoslynHelper.IsRoslynCfgSupported(2).Should().BeTrue();
        // If we set minimum requirement to 100 - we won't be able to pass the check
        RoslynHelper.IsRoslynCfgSupported(100).Should().BeFalse();
    }
}
