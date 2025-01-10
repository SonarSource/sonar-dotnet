/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2014-2025 SonarSource SA
 * mailto:info AT sonarsource DOT com
 * This program is free software; you can redistribute it and/or
 * modify it under the terms of the Sonar Source-Available License Version 1, as published by SonarSource SA.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
 * See the Sonar Source-Available License for more details.
 *
 * You should have received a copy of the Sonar Source-Available License
 * along with this program; if not, see https://sonarsource.com/license/ssal/
 */

using Microsoft.CodeAnalysis.Operations;
using SonarAnalyzer.CFG.Common;
using SonarAnalyzer.CFG.Extensions;
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
        const string code = """
            public class Sample
            {
                public int Method()
                {
                    return 42;
                }
            }
            """;
        TestCompiler.CompileCfgCS(code).Should().NotBeNull();
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
        TestCompiler.CompileCfg(code, AnalyzerLanguage.CSharp, outputKind: OutputKind.ConsoleApplication).Should().NotBeNull();
    }

    [TestMethod]
    public void Create_ReturnsCfg_VB()
    {
        const string code = """
            Public Class Sample
                Public Function Method() As Integer
                    Return 42
                End Function
            End Class
            """;
        TestCompiler.CompileCfg(code, AnalyzerLanguage.VisualBasic).Should().NotBeNull();
    }

    [TestMethod]
    public void ValidateReflection()
    {
        const string code = """
            public class Sample
            {
                public int Method()
                {
                    System.Action a = () => { };
                    return LocalMethod();

                    int LocalMethod() => 42;
                }
            }
            """;
        var cfg = TestCompiler.CompileCfgCS(code);
        cfg.Should().NotBeNull();
        cfg.Root.Should().NotBeNull();
        cfg.Blocks.Should().NotBeNull().And.HaveCount(3); // Enter, Instructions, Exit
        cfg.OriginalOperation.Should().NotBeNull().And.BeAssignableTo<IMethodBodyOperation>();
        cfg.Parent.Should().BeNull();
        cfg.LocalFunctions.Should().HaveCount(1);

        var localFunctionCfg = cfg.GetLocalFunctionControlFlowGraph(cfg.LocalFunctions.Single(), default);
        localFunctionCfg.Should().NotBeNull();
        localFunctionCfg.Parent.Should().Be(cfg);

        var anonymousFunction = cfg.Blocks.SelectMany(x => x.Operations).SelectMany(OperationExtensions.DescendantsAndSelf).OfType<FlowAnalysis.IFlowAnonymousFunctionOperation>().Single();
        cfg.GetAnonymousFunctionControlFlowGraph(IFlowAnonymousFunctionOperationWrapper.FromOperation(anonymousFunction), default).Should().NotBeNull();
    }

    [TestMethod]
    public void FlowAnonymousFunctionOperations_FindsAll()
    {
        const string code = """
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
            }
            """;
        var cfg = TestCompiler.CompileCfgCS(code);
        var anonymousFunctionOperations = ControlFlowGraphExtensions.FlowAnonymousFunctionOperations(cfg).ToList();
        anonymousFunctionOperations.Should().HaveCount(2);
        cfg.GetAnonymousFunctionControlFlowGraph(anonymousFunctionOperations[0], default).Should().NotBeNull();
        cfg.GetAnonymousFunctionControlFlowGraph(anonymousFunctionOperations[1], default).Should().NotBeNull();
    }

    [TestMethod]
    public void RoslynCfgSupportedVersions()
    {
        // We are running on 3 rd major version - it is the minimum requirement
        RoslynVersion.IsRoslynCfgSupported().Should().BeTrue();
        // If we set minimum requirement to 2 - we will able to pass the check even with old MsBuild
        RoslynVersion.IsRoslynCfgSupported(2).Should().BeTrue();
        // If we set minimum requirement to 100 - we won't be able to pass the check
        RoslynVersion.IsRoslynCfgSupported(100).Should().BeFalse();
    }
}
