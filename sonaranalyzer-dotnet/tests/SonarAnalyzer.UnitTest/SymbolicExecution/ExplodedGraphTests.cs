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

extern alias csharp;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using csharp::SonarAnalyzer.LiveVariableAnalysis.CSharp;
using FluentAssertions;
using FluentAssertions.Execution;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SonarAnalyzer.ControlFlowGraph;
using SonarAnalyzer.ControlFlowGraph.CSharp;
using SonarAnalyzer.SymbolicExecution;
using SonarAnalyzer.SymbolicExecution.Constraints;
using SonarAnalyzer.UnitTest.Helpers;

namespace SonarAnalyzer.UnitTest.SymbolicExecution
{
    [TestClass]
    public class ExplodedGraphTests
    {
        private const string TestInput = @"
namespace NS
{{
  public class Foo
  {{
    private bool Property {{ get; set; }}
    public void Bar(bool inParameter, out bool outParameter)
    {{
      {0}
    }}
  }}
}}";

        [TestMethod]
        [TestCategory("Symbolic execution")]
        public void ExplodedGraph_SequentialInput()
        {
            var testInput = "var a = true; var b = false; b = !b; a = (b);";
            var method = ControlFlowGraphTest.CompileWithMethodBody(string.Format(TestInput, testInput), "Bar", out var semanticModel);
            var methodSymbol = semanticModel.GetDeclaredSymbol(method);
            var varDeclarators = method.DescendantNodes().OfType<VariableDeclaratorSyntax>();
            var aSymbol = semanticModel.GetDeclaredSymbol(varDeclarators.First(d => d.Identifier.ToString() == "a"));
            var bSymbol = semanticModel.GetDeclaredSymbol(varDeclarators.First(d => d.Identifier.ToString() == "b"));

            var cfg = CSharpControlFlowGraph.Create(method.Body, semanticModel);
            var lva = CSharpLiveVariableAnalysis.Analyze(cfg, methodSymbol, semanticModel);

            var explodedGraph = new CSharpExplodedGraph(cfg, methodSymbol, semanticModel, lva);
            var explorationEnded = false;
            explodedGraph.ExplorationEnded += (sender, args) => { explorationEnded = true; };

            var numberOfExitBlockReached = 0;
            explodedGraph.ExitBlockReached += (sender, args) => { numberOfExitBlockReached++; };

            var numberOfProcessedInstructions = 0;
            explodedGraph.InstructionProcessed +=
                (sender, args) =>
                {
                    numberOfProcessedInstructions++;
                    if (args.Instruction.ToString() == "a = true")
                    {
                        args.ProgramState.GetSymbolValue(aSymbol).Should().Be(SymbolicValue.True);
                    }
                    if (args.Instruction.ToString() == "b = false")
                    {
                        args.ProgramState.GetSymbolValue(bSymbol).Should().Be(SymbolicValue.False);
                    }
                    if (args.Instruction.ToString() == "b = !b")
                    {
                        args.ProgramState.GetSymbolValue(bSymbol).Should().NotBe(SymbolicValue.False);
                        args.ProgramState.GetSymbolValue(bSymbol).Should().NotBe(SymbolicValue.True);
                    }
                    if (args.Instruction.ToString() == "a = (b)")
                    {
                        args.ProgramState.GetSymbolValue(bSymbol)
                            .Should().Be(args.ProgramState.GetSymbolValue(aSymbol));
                    }
                };

            explodedGraph.Walk();

            explorationEnded.Should().BeTrue();
            numberOfProcessedInstructions.Should().Be(9);
            numberOfExitBlockReached.Should().Be(1);
        }

        [TestMethod]
        [TestCategory("Symbolic execution")]
        public void ExplodedGraph_SequentialInput_OutParameter()
        {
            var testInput = "outParameter = true;";
            var method = ControlFlowGraphTest.CompileWithMethodBody(string.Format(TestInput, testInput), "Bar", out var semanticModel);
            var methodSymbol = semanticModel.GetDeclaredSymbol(method);
            var parameters = method.DescendantNodes().OfType<ParameterSyntax>();
            var outParameterSymbol = semanticModel.GetDeclaredSymbol(parameters.First(d => d.Identifier.ToString() == "outParameter"));

            var cfg = CSharpControlFlowGraph.Create(method.Body, semanticModel);
            var lva = CSharpLiveVariableAnalysis.Analyze(cfg, methodSymbol, semanticModel);

            var explodedGraph = new CSharpExplodedGraph(cfg, methodSymbol, semanticModel, lva);
            var explorationEnded = false;
            explodedGraph.ExplorationEnded += (sender, args) => { explorationEnded = true; };

            var numberOfExitBlockReached = 0;
            explodedGraph.ExitBlockReached += (sender, args) => { numberOfExitBlockReached++; };

            var numberOfProcessedInstructions = 0;
            explodedGraph.InstructionProcessed +=
                (sender, args) =>
                {
                    numberOfProcessedInstructions++;
                    if (args.Instruction.ToString() == "outParameter = true")
                    {
                        args.ProgramState.GetSymbolValue(outParameterSymbol)
                            .Should().Be(SymbolicValue.True);
                    }
                };

            explodedGraph.Walk();

            explorationEnded.Should().BeTrue();
            numberOfProcessedInstructions.Should().Be(2);
            numberOfExitBlockReached.Should().Be(1);
        }

        [TestMethod]
        [TestCategory("Symbolic execution")]
        public void ExplodedGraph_SequentialInput_Max()
        {
            var inputBuilder = new StringBuilder();
            for (var i = 0; i < CSharpExplodedGraph.MaxStepCount / 2 + 1; i++)
            {
                inputBuilder.AppendLine($"var x{i} = true;");
            }
            var testInput = inputBuilder.ToString();
            var method = ControlFlowGraphTest.CompileWithMethodBody(string.Format(TestInput, testInput), "Bar", out var semanticModel);
            var methodSymbol = semanticModel.GetDeclaredSymbol(method);

            var cfg = CSharpControlFlowGraph.Create(method.Body, semanticModel);
            var lva = CSharpLiveVariableAnalysis.Analyze(cfg, methodSymbol, semanticModel);

            var explodedGraph = new CSharpExplodedGraph(cfg, methodSymbol, semanticModel, lva);
            var explorationEnded = false;
            explodedGraph.ExplorationEnded += (sender, args) => { explorationEnded = true; };
            var maxStepCountReached = false;
            explodedGraph.MaxStepCountReached += (sender, args) => { maxStepCountReached = true; };

            var numberOfExitBlockReached = 0;
            explodedGraph.ExitBlockReached += (sender, args) => { numberOfExitBlockReached++; };

            explodedGraph.Walk();

            explorationEnded.Should().BeFalse();
            maxStepCountReached.Should().BeTrue();
            numberOfExitBlockReached.Should().Be(0);
        }

        [TestMethod]
        [TestCategory("Symbolic execution")]
        public void ExplodedGraph_SingleBranchVisited_If()
        {
            var testInput = "var a = false; bool b; if (a) { b = true; } else { b = false; } a = b;";
            var method = ControlFlowGraphTest.CompileWithMethodBody(string.Format(TestInput, testInput), "Bar", out var semanticModel);
            var methodSymbol = semanticModel.GetDeclaredSymbol(method);
            var varDeclarators = method.DescendantNodes().OfType<VariableDeclaratorSyntax>();
            var aSymbol = semanticModel.GetDeclaredSymbol(varDeclarators.First(d => d.Identifier.ToString() == "a"));
            var bSymbol = semanticModel.GetDeclaredSymbol(varDeclarators.First(d => d.Identifier.ToString() == "b"));

            var cfg = CSharpControlFlowGraph.Create(method.Body, semanticModel);
            var lva = CSharpLiveVariableAnalysis.Analyze(cfg, methodSymbol, semanticModel);

            var explodedGraph = new CSharpExplodedGraph(cfg, methodSymbol, semanticModel, lva);
            var explorationEnded = false;
            explodedGraph.ExplorationEnded += (sender, args) => { explorationEnded = true; };

            var numberOfExitBlockReached = 0;
            explodedGraph.ExitBlockReached += (sender, args) => { numberOfExitBlockReached++; };

            var numberOfLastInstructionVisits = 0;
            var numberOfProcessedInstructions = 0;

            explodedGraph.InstructionProcessed +=
                (sender, args) =>
                {
                    numberOfProcessedInstructions++;
                    if (args.Instruction.ToString() == "a = false")
                    {
                        args.ProgramState.GetSymbolValue(aSymbol).Should().Be(SymbolicValue.False);
                    }
                    if (args.Instruction.ToString() == "b = true")
                    {
                        Execute.Assertion.FailWith("We should never get into this branch");
                    }
                    if (args.Instruction.ToString() == "b = false")
                    {
                        args.ProgramState.GetSymbolValue(bSymbol).Should().Be(SymbolicValue.False);
                        args.ProgramState.GetSymbolValue(aSymbol)
                            .Should().BeNull("a is dead, so there should be no associated value to it.");
                    }
                    if (args.Instruction.ToString() == "a = b")
                    {
                        numberOfLastInstructionVisits++;
                    }
                };

            explodedGraph.Walk();

            explorationEnded.Should().BeTrue();
            numberOfProcessedInstructions.Should().Be(8);
            numberOfExitBlockReached.Should().Be(1);
            numberOfLastInstructionVisits.Should().Be(1);
        }

        [TestMethod]
        [TestCategory("Symbolic execution")]
        public void ExplodedGraph_SingleBranchVisited_And()
        {
            var testInput = "var a = false; if (a && !a) { a = true; }";
            var method = ControlFlowGraphTest.CompileWithMethodBody(string.Format(TestInput, testInput), "Bar", out var semanticModel);
            var methodSymbol = semanticModel.GetDeclaredSymbol(method);
            var varDeclarators = method.DescendantNodes().OfType<VariableDeclaratorSyntax>();
            var aSymbol = semanticModel.GetDeclaredSymbol(varDeclarators.First(d => d.Identifier.ToString() == "a"));

            var cfg = CSharpControlFlowGraph.Create(method.Body, semanticModel);
            var lva = CSharpLiveVariableAnalysis.Analyze(cfg, methodSymbol, semanticModel);

            var explodedGraph = new CSharpExplodedGraph(cfg, methodSymbol, semanticModel, lva);
            var explorationEnded = false;
            explodedGraph.ExplorationEnded += (sender, args) => { explorationEnded = true; };

            var numberOfExitBlockReached = 0;
            explodedGraph.ExitBlockReached += (sender, args) => { numberOfExitBlockReached++; };

            var numberOfProcessedInstructions = 0;

            explodedGraph.InstructionProcessed +=
                (sender, args) =>
                {
                    numberOfProcessedInstructions++;
                    if (args.Instruction.ToString() == "a = !true")
                    {
                        args.ProgramState.GetSymbolValue(aSymbol).Should().Be(SymbolicValue.False); // Roslyn is clever !true has const value.
                    }
                    if (args.Instruction.ToString() == "!a")
                    {
                        Execute.Assertion.FailWith("We should never get into this branch");
                    }
                };

            explodedGraph.Walk();

            explorationEnded.Should().BeTrue();
            numberOfExitBlockReached.Should().Be(1);
        }

        [TestMethod]
        [TestCategory("Symbolic execution")]
        public void ExplodedGraph_BothBranchesVisited()
        {
            var testInput = "var a = false; bool b; if (inParameter) { b = inParameter; } else { b = !inParameter; } a = b;";
            var method = ControlFlowGraphTest.CompileWithMethodBody(string.Format(TestInput, testInput), "Bar", out var semanticModel);
            var methodSymbol = semanticModel.GetDeclaredSymbol(method);

            var varDeclarators = method.DescendantNodes().OfType<VariableDeclaratorSyntax>();
            var aSymbol = semanticModel.GetDeclaredSymbol(varDeclarators.First(d => d.Identifier.ToString() == "a"));
            var bSymbol = semanticModel.GetDeclaredSymbol(varDeclarators.First(d => d.Identifier.ToString() == "b"));

            var parameters = method.DescendantNodes().OfType<ParameterSyntax>();
            var inParameterSymbol = semanticModel.GetDeclaredSymbol(parameters.First(d => d.Identifier.ToString() == "inParameter"));

            var cfg = CSharpControlFlowGraph.Create(method.Body, semanticModel);
            var lva = CSharpLiveVariableAnalysis.Analyze(cfg, methodSymbol, semanticModel);

            var explodedGraph = new CSharpExplodedGraph(cfg, methodSymbol, semanticModel, lva);
            var explorationEnded = false;
            explodedGraph.ExplorationEnded += (sender, args) => { explorationEnded = true; };

            var numberOfExitBlockReached = 0;
            explodedGraph.ExitBlockReached += (sender, args) => { numberOfExitBlockReached++; };

            var numberOfLastInstructionVisits = 0;
            var numberOfProcessedInstructions = 0;

            var visitedBlocks = new HashSet<Block>();
            var branchesVisited = 0;

            explodedGraph.InstructionProcessed +=
                (sender, args) =>
                {
                    visitedBlocks.Add(args.ProgramPoint.Block);

                    numberOfProcessedInstructions++;
                    if (args.Instruction.ToString() == "a = false")
                    {
                        branchesVisited++;

                        args.ProgramState.GetSymbolValue(aSymbol).Should().Be(SymbolicValue.False); // Roslyn is clever !true has const value.
                    }
                    if (args.Instruction.ToString() == "b = inParameter")
                    {
                        branchesVisited++;

                        bSymbol.HasConstraint(BoolConstraint.True, args.ProgramState).Should().BeTrue();
                        inParameterSymbol.HasConstraint(BoolConstraint.True, args.ProgramState).Should().BeTrue();
                    }
                    if (args.Instruction.ToString() == "b = !inParameter")
                    {
                        branchesVisited++;

                        // b has value, but not true or false
                        args.ProgramState.GetSymbolValue(bSymbol).Should().NotBeNull();
                        bSymbol.HasConstraint(BoolConstraint.False, args.ProgramState).Should().BeFalse();
                        bSymbol.HasConstraint(BoolConstraint.True, args.ProgramState).Should().BeFalse();

                        inParameterSymbol.HasConstraint(BoolConstraint.False, args.ProgramState).Should().BeTrue();
                    }
                    if (args.Instruction.ToString() == "a = b")
                    {
                        branchesVisited++;

                        args.ProgramState.GetSymbolValue(inParameterSymbol).Should().BeNull(); // not out/ref parameter and LVA says dead
                        numberOfLastInstructionVisits++;
                    }
                };

            explodedGraph.Walk();

            explorationEnded.Should().BeTrue();
            branchesVisited.Should().Be(4 + 1);
            numberOfExitBlockReached.Should().Be(1,
                "All variables are dead at the ExitBlock, so whenever we get there, the ExplodedGraph nodes should be the same, " +
                "and thus should be processed only once.");
            numberOfLastInstructionVisits.Should().Be(2);

            visitedBlocks.Should().HaveCount(cfg.Blocks.Count() - 1 /* Exit block*/);
        }

        [TestMethod]
        [TestCategory("Symbolic execution")]
        public void ExplodedGraph_BothBranchesVisited_StateMerge()
        {
            var testInput = "var a = !true; bool b; if (inParameter) { b = false; } else { b = false; } a = b;";
            var method = ControlFlowGraphTest.CompileWithMethodBody(string.Format(TestInput, testInput), "Bar", out var semanticModel);
            var methodSymbol = semanticModel.GetDeclaredSymbol(method);

            var varDeclarators = method.DescendantNodes().OfType<VariableDeclaratorSyntax>();
            var aSymbol = semanticModel.GetDeclaredSymbol(varDeclarators.First(d => d.Identifier.ToString() == "a"));

            var cfg = CSharpControlFlowGraph.Create(method.Body, semanticModel);
            var lva = CSharpLiveVariableAnalysis.Analyze(cfg, methodSymbol, semanticModel);

            var explodedGraph = new CSharpExplodedGraph(cfg, methodSymbol, semanticModel, lva);
            var explorationEnded = false;
            explodedGraph.ExplorationEnded += (sender, args) => { explorationEnded = true; };

            var numberOfExitBlockReached = 0;
            explodedGraph.ExitBlockReached += (sender, args) => { numberOfExitBlockReached++; };

            var numberOfLastInstructionVisits = 0;
            var numberOfProcessedInstructions = 0;

            explodedGraph.InstructionProcessed +=
                (sender, args) =>
                {
                    numberOfProcessedInstructions++;
                    if (args.Instruction.ToString() == "a = b")
                    {
                        args.ProgramState.GetSymbolValue(aSymbol).Should().Be(SymbolicValue.False);
                        numberOfLastInstructionVisits++;
                    }
                };

            explodedGraph.Walk();

            explorationEnded.Should().BeTrue();
            numberOfExitBlockReached.Should().Be(1);
            numberOfLastInstructionVisits.Should().Be(1);
        }

        [TestMethod]
        [TestCategory("Symbolic execution")]
        public void ExplodedGraph_BothBranchesVisited_NonCondition()
        {
            var testInput = "var str = this?.ToString();";
            var method = ControlFlowGraphTest.CompileWithMethodBody(string.Format(TestInput, testInput), "Bar", out var semanticModel);
            var methodSymbol = semanticModel.GetDeclaredSymbol(method);

            var cfg = CSharpControlFlowGraph.Create(method.Body, semanticModel);
            var lva = CSharpLiveVariableAnalysis.Analyze(cfg, methodSymbol, semanticModel);

            var explodedGraph = new CSharpExplodedGraph(cfg, methodSymbol, semanticModel, lva);
            var explorationEnded = false;
            explodedGraph.ExplorationEnded += (sender, args) => { explorationEnded = true; };

            var countConditionEvaluated = 0;
            explodedGraph.ConditionEvaluated += (sender, args) => { countConditionEvaluated++; };

            var visitedBlocks = new HashSet<Block>();

            explodedGraph.InstructionProcessed +=
                (sender, args) =>
                {
                    visitedBlocks.Add(args.ProgramPoint.Block);
                };

            explodedGraph.Walk();

            explorationEnded.Should().BeTrue();
            visitedBlocks.Should().HaveCount(cfg.Blocks.Count() - 1 /* Exit block */);
            countConditionEvaluated.Should().Be(0);
        }

        [TestMethod]
        [TestCategory("Symbolic execution")]
        public void ExplodedGraph_AllBranchesVisited()
        {
            var testInput = "int i = 1; switch (i) { case 1: default: cw1(); break; case 2: cw2(); break; }";
            var method = ControlFlowGraphTest.CompileWithMethodBody(string.Format(TestInput, testInput), "Bar", out var semanticModel);
            var methodSymbol = semanticModel.GetDeclaredSymbol(method);

            var cfg = CSharpControlFlowGraph.Create(method.Body, semanticModel);
            var lva = CSharpLiveVariableAnalysis.Analyze(cfg, methodSymbol, semanticModel);

            var explodedGraph = new CSharpExplodedGraph(cfg, methodSymbol, semanticModel, lva);
            var explorationEnded = false;
            explodedGraph.ExplorationEnded += (sender, args) => { explorationEnded = true; };

            var numberOfExitBlockReached = 0;
            explodedGraph.ExitBlockReached += (sender, args) => { numberOfExitBlockReached++; };

            var numberOfCw1InstructionVisits = 0;
            var numberOfCw2InstructionVisits = 0;
            var numberOfProcessedInstructions = 0;

            explodedGraph.InstructionProcessed +=
                (sender, args) =>
                {
                    numberOfProcessedInstructions++;
                    if (args.Instruction.ToString() == "cw1()")
                    {
                        numberOfCw1InstructionVisits++;
                    }
                    if (args.Instruction.ToString() == "cw2()")
                    {
                        numberOfCw2InstructionVisits++;
                    }
                };

            explodedGraph.Walk();

            explorationEnded.Should().BeTrue();
            numberOfExitBlockReached.Should().Be(1);
            numberOfCw1InstructionVisits.Should().Be(2);
            numberOfCw2InstructionVisits.Should().Be(1);
        }

        [TestMethod]
        [TestCategory("Symbolic execution")]
        public void ExplodedGraph_NonDecisionMakingAssignments()
        {
            var testInput = "var a = true; a |= false; var b = 42; b++; ++b;";
            var method = ControlFlowGraphTest.CompileWithMethodBody(string.Format(TestInput, testInput), "Bar", out var semanticModel);
            var methodSymbol = semanticModel.GetDeclaredSymbol(method);
            var varDeclarators = method.DescendantNodes().OfType<VariableDeclaratorSyntax>();
            var aSymbol = semanticModel.GetDeclaredSymbol(varDeclarators.First(d => d.Identifier.ToString() == "a"));
            var bSymbol = semanticModel.GetDeclaredSymbol(varDeclarators.First(d => d.Identifier.ToString() == "b"));

            var cfg = CSharpControlFlowGraph.Create(method.Body, semanticModel);
            var lva = CSharpLiveVariableAnalysis.Analyze(cfg, methodSymbol, semanticModel);

            var explodedGraph = new CSharpExplodedGraph(cfg, methodSymbol, semanticModel, lva);

            SymbolicValue sv = null;
            var numberOfProcessedInstructions = 0;
            var branchesVisited = 0;

            explodedGraph.InstructionProcessed +=
                (sender, args) =>
                {
                    numberOfProcessedInstructions++;
                    if (args.Instruction.ToString() == "a = true")
                    {
                        branchesVisited++;
                        args.ProgramState.GetSymbolValue(aSymbol).Should().Be(SymbolicValue.True);
                    }
                    if (args.Instruction.ToString() == "a |= false")
                    {
                        branchesVisited++;
                        args.ProgramState.GetSymbolValue(aSymbol).Should().NotBeNull();
                        args.ProgramState.GetSymbolValue(aSymbol).Should().NotBe(SymbolicValue.False);
                        args.ProgramState.GetSymbolValue(aSymbol).Should().NotBe(SymbolicValue.True);
                    }
                    if (args.Instruction.ToString() == "b = 42")
                    {
                        branchesVisited++;
                        sv = args.ProgramState.GetSymbolValue(bSymbol);
                        sv.Should().NotBeNull();
                    }
                    if (args.Instruction.ToString() == "b++")
                    {
                        branchesVisited++;
                        var svNew = args.ProgramState.GetSymbolValue(bSymbol);
                        svNew.Should().NotBeNull();
                        svNew.Should().NotBe(sv);
                    }
                    if (args.Instruction.ToString() == "++b")
                    {
                        branchesVisited++;
                        var svNew = args.ProgramState.GetSymbolValue(bSymbol);
                        svNew.Should().NotBeNull();
                        svNew.Should().NotBe(sv);
                    }
                };

            explodedGraph.Walk();

            numberOfProcessedInstructions.Should().Be(11);
            branchesVisited.Should().Be(5);
        }

        [TestMethod]
        [TestCategory("Symbolic execution")]
        public void ExplodedGraph_NonLocalNorFieldSymbolBranching()
        {
            var testInput = "if (Property) { cw(); }";
            var method = ControlFlowGraphTest.CompileWithMethodBody(string.Format(TestInput, testInput), "Bar", out var semanticModel);
            var methodSymbol = semanticModel.GetDeclaredSymbol(method);
            var propertySymbol = semanticModel.GetSymbolInfo(
                method.DescendantNodes().OfType<IdentifierNameSyntax>().First(d => d.Identifier.ToString() == "Property")).Symbol;

            propertySymbol.Should().NotBeNull();

            var cfg = CSharpControlFlowGraph.Create(method.Body, semanticModel);
            var lva = CSharpLiveVariableAnalysis.Analyze(cfg, methodSymbol, semanticModel);

            var explodedGraph = new CSharpExplodedGraph(cfg, methodSymbol, semanticModel, lva);
            var explorationEnded = false;
            explodedGraph.ExplorationEnded += (sender, args) => { explorationEnded = true; };

            var numberOfExitBlockReached = 0;
            explodedGraph.ExitBlockReached += (sender, args) => { numberOfExitBlockReached++; };

            var numberOfProcessedInstructions = 0;

            explodedGraph.InstructionProcessed +=
                (sender, args) =>
                {
                    numberOfProcessedInstructions++;
                    if (args.Instruction.ToString() == "Property")
                    {
                        args.ProgramState.GetSymbolValue(propertySymbol).Should().BeNull();
                    }
                };

            explodedGraph.Walk();

            explorationEnded.Should().BeTrue();
            numberOfExitBlockReached.Should().Be(1);
        }

        [TestMethod]
        [TestCategory("Symbolic execution")]
        public void ExplodedGraph_LoopExploration()
        {
            var testInput = "var i = 0; while (i < 1) { i = i + 1; }";
            var method = ControlFlowGraphTest.CompileWithMethodBody(string.Format(TestInput, testInput), "Bar", out var semanticModel);
            var methodSymbol = semanticModel.GetDeclaredSymbol(method);

            var cfg = CSharpControlFlowGraph.Create(method.Body, semanticModel);
            var lva = CSharpLiveVariableAnalysis.Analyze(cfg, methodSymbol, semanticModel);

            var explodedGraph = new CSharpExplodedGraph(cfg, methodSymbol, semanticModel, lva);
            var explorationEnded = false;
            explodedGraph.ExplorationEnded += (sender, args) => { explorationEnded = true; };

            var exceeded = 0;
            explodedGraph.ProgramPointVisitCountExceedLimit += (sender, args) =>
            {
                exceeded++;
                args.ProgramPoint.Block.Instructions.Should().Contain(i => i.ToString() == "i < 1");
            };

            explodedGraph.Walk();

            explorationEnded.Should().BeTrue();
            exceeded.Should().Be(1);
        }

        [TestMethod]
        [TestCategory("Symbolic execution")]
        public void ExplodedGraph_InternalStateCount_MaxReached()
        {
            if (TestContextHelper.IsAzureDevOpsContext) // FIX ME: test throws OOM on Azure DevOps
            {
                return;
            }

            var testInput = @"
using System;

namespace TesteAnalyzer
{
    class Program
    {
        static bool GetBool() { return bool.Parse(""True""); }

        static void Main(string[] args)
        {
            bool corrupted = false;
            corrupted |= !GetBool();
            corrupted |= !GetBool();
            corrupted |= !GetBool();
            corrupted |= !GetBool();
            corrupted |= !GetBool();
            corrupted |= !GetBool();
            corrupted |= !GetBool();
            corrupted |= !GetBool();
            corrupted |= !GetBool();
            corrupted |= !GetBool();
            corrupted |= !GetBool();
            corrupted |= !GetBool();
            corrupted |= !GetBool();
            corrupted |= !GetBool();
            corrupted |= !GetBool();
            corrupted |= !GetBool();
            corrupted |= !GetBool();

            if (!corrupted)
            {
                Console.Out.WriteLine();
            }
        }
    }
}
";
            var (tree, semanticModel) = TestHelper.Compile(testInput);
            var method = tree.GetRoot().DescendantNodes().OfType<MethodDeclarationSyntax>().First(m => m.Identifier.ValueText == "Main");
            var methodSymbol = semanticModel.GetDeclaredSymbol(method);

            var cfg = CSharpControlFlowGraph.Create(method.Body, semanticModel);
            var lva = CSharpLiveVariableAnalysis.Analyze(cfg, methodSymbol, semanticModel);

            var explodedGraph = new CSharpExplodedGraph(cfg, methodSymbol, semanticModel, lva);
            var explorationEnded = false;
            explodedGraph.ExplorationEnded += (sender, args) => { explorationEnded = true; };
            var maxStepCountReached = false;
            explodedGraph.MaxStepCountReached += (sender, args) => { maxStepCountReached = true; };
            var maxInternalStateCountReached = false;
            explodedGraph.MaxInternalStateCountReached += (sender, args) => { maxInternalStateCountReached = true; };

            explodedGraph.Walk();

            explorationEnded.Should().BeFalse();
            maxStepCountReached.Should().BeFalse();
            maxInternalStateCountReached.Should().BeTrue();
        }

        [TestMethod]
        [TestCategory("Symbolic execution")]
        public void ExplodedGraph_DeclarationStatementVisit()
        {
            const string methodBody =  @"
using System.Collections.Generic;

namespace Namespace
{
  public class DeclarationStatement
  {
    public int Test(Dictionary<string, string> dictionary, string key)
    {
        dictionary.TryGetValue(key, out var value);
    }
  }
}";
            var method = ControlFlowGraphTest.CompileWithMethodBody(methodBody, "Test", out var semanticModel);
            var methodSymbol = semanticModel.GetDeclaredSymbol(method);
            var dictionarySymbol = semanticModel.GetDeclaredSymbol(method.ParameterList.Parameters.First());

            var valueDeclaration = method.DescendantNodes().OfType<DeclarationExpressionSyntax>().First();
            var valueSymbol = semanticModel.GetDeclaredSymbol(valueDeclaration);

            var cfg = CSharpControlFlowGraph.Create(method.Body, semanticModel);
            var lva = CSharpLiveVariableAnalysis.Analyze(cfg, methodSymbol, semanticModel);

            var explodedGraph = new CSharpExplodedGraph(cfg, methodSymbol, semanticModel, lva);

            explodedGraph.InstructionProcessed +=
                (sender, args) =>
                {
                    var instruction = args.Instruction.ToString();
                    switch (instruction)
                    {
                        case "dictionary":
                            args.ProgramState.GetSymbolValue(dictionarySymbol).Should().NotBeNull();
                            dictionarySymbol.HasConstraint(ObjectConstraint.NotNull, args.ProgramState).Should().BeFalse();
                            break;

                        case "dictionary.TryGetValue":
                            args.ProgramState.GetSymbolValue(dictionarySymbol).Should().NotBeNull();
                            dictionarySymbol.HasConstraint(ObjectConstraint.NotNull, args.ProgramState).Should().BeTrue();
                            break;

                        case "key":
                            args.ProgramState.GetSymbolValue(dictionarySymbol).Should().NotBeNull();
                            dictionarySymbol.HasConstraint(ObjectConstraint.NotNull, args.ProgramState).Should().BeTrue();
                            break;

                        case "var value":
                            args.ProgramState.GetSymbolValue(dictionarySymbol).Should().NotBeNull();
                            dictionarySymbol.HasConstraint(ObjectConstraint.NotNull, args.ProgramState).Should().BeTrue();
                            break;

                        case "dictionary.TryGetValue(key, out var value)":
                            args.ProgramState.GetSymbolValue(dictionarySymbol).Should().NotBeNull();
                            dictionarySymbol.HasConstraint(ObjectConstraint.NotNull, args.ProgramState).Should().BeTrue();

                            // Currently the DeclarationExpressionSyntax are ignored so the "value" variable is not
                            // https://github.com/SonarSource/sonar-dotnet/issues/2936
                            args.ProgramState.GetSymbolValue(valueSymbol).Should().BeNull();
                            break;
                    }
                };

            explodedGraph.Walk();
        }

        [TestMethod]
        [TestCategory("Symbolic execution")]
        public void ExplodedGraph_SwitchWithRecursivePatternVisit()
        {
            const string methodBody =  @"
using System.Collections.Generic;

namespace Namespace
{
    public class Address
    {
        public string Name { get; }
        public string State { get; }
    }

    public class Person
    {
        public string Name { get; }
        public Address Address { get; }
    }

    public class DeclarationStatement
    {
public string Test(Person person)
{
    return person switch
        {
            { Address: {State: ""WA"" } address } p => address.Name,
            _ => string.Empty
        };
}
    }
}";
            var method = ControlFlowGraphTest.CompileWithMethodBody(methodBody, "Test", out var semanticModel);
            var methodSymbol = semanticModel.GetDeclaredSymbol(method);
            var personSymbol = semanticModel.GetDeclaredSymbol(method.ParameterList.Parameters.First());

            var declarations = method.DescendantNodes().OfType<SingleVariableDesignationSyntax>().ToList();
            var addressSymbol = semanticModel.GetDeclaredSymbol(declarations[0]);
            var pSymbol = semanticModel.GetDeclaredSymbol(declarations[1]);

            var cfg = CSharpControlFlowGraph.Create(method.Body, semanticModel);
            var lva = CSharpLiveVariableAnalysis.Analyze(cfg, methodSymbol, semanticModel);

            var explodedGraph = new CSharpExplodedGraph(cfg, methodSymbol, semanticModel, lva);

            explodedGraph.InstructionProcessed +=
                (sender, args) =>
                {
                    var instruction = args.Instruction.ToString();
                    switch (instruction)
                    {
                        case "person":
                            args.ProgramState.GetSymbolValue(personSymbol).Should().NotBeNull();
                            break;

                        case "{ Address: {State: \"WA\" } address } p":
                            args.ProgramState.GetSymbolValue(personSymbol).Should().NotBeNull();

                            // Currently the recursive pattern is ignored and the values for "p" and "address" are not created.
                            // https://github.com/SonarSource/sonar-dotnet/issues/2937
                            args.ProgramState.GetSymbolValue(addressSymbol).Should().BeNull();
                            args.ProgramState.GetSymbolValue(pSymbol).Should().BeNull();
                            break;

                        case "{State: \"WA\" } address":
                            args.ProgramState.GetSymbolValue(personSymbol).Should().NotBeNull();

                            // Currently the recursive pattern is ignored and the value for "address" is not created.
                            // https://github.com/SonarSource/sonar-dotnet/issues/2937
                            args.ProgramState.GetSymbolValue(addressSymbol).Should().BeNull();
                            break;

                        case "\"WA\"":
                            args.ProgramState.GetSymbolValue(personSymbol).Should().NotBeNull();
                            break;
                    }
                };

            explodedGraph.Walk();
        }

        [TestMethod]
        [TestCategory("Symbolic execution")]
        public void ExplodedGraph_SwitchExpressionVisit()
        {
            const string methodBody =  @"
namespace Namespace
{
  public class SwitchExpression
  {
    public int Test(string str)
    {
      return str switch { null => 1, """" => 2, _ => 3};
    }
  }
}";
            var method = ControlFlowGraphTest.CompileWithMethodBody(methodBody, "Test", out var semanticModel);
            var methodSymbol = semanticModel.GetDeclaredSymbol(method);
            var strParameter = method.ParameterList.Parameters.First();
            var strSymbol = semanticModel.GetDeclaredSymbol(strParameter);

            var cfg = CSharpControlFlowGraph.Create(method.Body, semanticModel);
            var lva = CSharpLiveVariableAnalysis.Analyze(cfg, methodSymbol, semanticModel);

            var explodedGraph = new CSharpExplodedGraph(cfg, methodSymbol, semanticModel, lva);

            var explorationEnded = false;
            explodedGraph.ExplorationEnded += (sender, args) => { explorationEnded = true; };

            var numberOfExitBlockReached = 0;
            explodedGraph.ExitBlockReached += (sender, args) => { numberOfExitBlockReached++; };

            explodedGraph.InstructionProcessed +=
                (sender, args) =>
                {
                    var instruction = args.Instruction.ToString();

                    switch (instruction)
                    {
                        case "1" :
                            args.ProgramState.GetSymbolValue(strSymbol).Should().NotBe(SymbolicValue.Null);
                            strSymbol.HasConstraint(ObjectConstraint.Null, args.ProgramState).Should().BeTrue();
                            break;

                        case "2":
                            args.ProgramState.GetSymbolValue(strSymbol).Should().NotBe(SymbolicValue.Null);
                            strSymbol.HasConstraint(ObjectConstraint.NotNull, args.ProgramState).Should().BeTrue();
                            break;

                        case "3":
                            args.ProgramState.GetSymbolValue(strSymbol).Should().BeNull();
                            // due to the current shape of the CFG the last node doesn't have the right constrains
                            // https://github.com/SonarSource/sonar-dotnet/issues/2938
                            // strSymbol.HasConstraint(ObjectConstraint.NotNull, args.ProgramState).Should().BeTrue();
                            break;
                    }
                };

            explodedGraph.Walk();
            explorationEnded.Should().BeTrue();
            numberOfExitBlockReached.Should().Be(3);
        }

        [TestMethod]
        [TestCategory("Symbolic execution")]
        public void ExplodedGraph_NullCoalesceAssignmentVisit()
        {
            const string methodBody =  @"
using System.Collections.Generic;

namespace Namespace
{
    public class NullCoalescenceAssignment
    {
        public void Test(string s)
        {
            s ??= ""N/A"";
        }
    }
}";
            var method = ControlFlowGraphTest.CompileWithMethodBody(methodBody, "Test", out var semanticModel);
            var methodSymbol = semanticModel.GetDeclaredSymbol(method);
            var sSymbol = semanticModel.GetDeclaredSymbol(method.ParameterList.Parameters.First());

            var cfg = CSharpControlFlowGraph.Create(method.Body, semanticModel);
            var lva = CSharpLiveVariableAnalysis.Analyze(cfg, methodSymbol, semanticModel);

            var explodedGraph = new CSharpExplodedGraph(cfg, methodSymbol, semanticModel, lva);

            explodedGraph.InstructionProcessed +=
                (sender, args) =>
                {
                    var instruction = args.Instruction.ToString();
                    switch (instruction)
                    {
                        case "s":
                            args.ProgramState.GetSymbolValue(sSymbol).Should().NotBeNull();
                            sSymbol.HasConstraint(ObjectConstraint.NotNull, args.ProgramState).Should().BeFalse();
                            break;

                        case "s ??= \"N/A\"":
                            args.ProgramState.GetSymbolValue(sSymbol).Should().NotBeNull();
                            sSymbol.HasConstraint(ObjectConstraint.NotNull, args.ProgramState).Should().BeTrue();
                            break;
                    }
                };

            explodedGraph.Walk();
        }
    }
}
