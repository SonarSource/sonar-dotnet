﻿/*
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

using System.Text;
using FluentAssertions.Execution;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SonarAnalyzer.CFG.Helpers;
using SonarAnalyzer.CFG.Sonar;
using SonarAnalyzer.Extensions;
using SonarAnalyzer.LiveVariableAnalysis.CSharp;
using SonarAnalyzer.SymbolicExecution.Constraints;
using SonarAnalyzer.SymbolicExecution.Sonar;
using SonarAnalyzer.Test.CFG.Sonar;
using SonarAnalyzer.Test.Helpers;

namespace SonarAnalyzer.Test.SymbolicExecution.Sonar
{
    [TestClass]
    public class SonarExplodedGraphTest
    {
        private const string TestInput = @"
namespace NS
{{
  public class Foo
  {{
    private bool Property {{ get; set; }}
    public void Main(bool inParameter, out bool outParameter)
    {{
      {0}
    }}
  }}
}}";

        [TestMethod]
        public void ExplodedGraph_SequentialInput()
        {
            const string testInput = "var a = true; var b = false; b = !b; a = (b);";
            var context = new ExplodedGraphContext(testInput);
            var aSymbol = context.GetSymbol("a");
            var bSymbol = context.GetSymbol("b");
            context.ExplodedGraph.InstructionProcessed +=
                (sender, args) =>
                {
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

            context.WalkWithInstructions(9);
        }

        [TestMethod]
        public void ExplodedGraph_SequentialInput_OutParameter()
        {
            const string testInput = "outParameter = true;";
            var context = new ExplodedGraphContext(testInput);
            var parameters = context.MainMethod.DescendantNodes().OfType<ParameterSyntax>();
            var outParameterSymbol = context.SemanticModel.GetDeclaredSymbol(parameters.First(d => d.Identifier.ToString() == "outParameter"));
            context.ExplodedGraph.InstructionProcessed +=
                (sender, args) =>
                {
                    if (args.Instruction.ToString() == "outParameter = true")
                    {
                        args.ProgramState.GetSymbolValue(outParameterSymbol)
                            .Should().Be(SymbolicValue.True);
                    }
                };

            context.WalkWithInstructions(2);
        }

        [TestMethod]
        public void ExplodedGraph_SequentialInput_Max()
        {
            var inputBuilder = new StringBuilder();
            for (var i = 0; i < SonarExplodedGraph.MaxStepCount / 2 + 1; i++)
            {
                inputBuilder.AppendLine($"var x{i} = true;");
            }
            var testInput = inputBuilder.ToString();
            var context = new ExplodedGraphContext(testInput);

            context.ExplodedGraph.Walk();  // Special case with manual checks

            context.ExplorationEnded.Should().Be(false);
            context.NumberOfExitBlockReached.Should().Be(0);
            context.MaxStepCountReached.Should().Be(true);
        }

        [TestMethod]
        public void ExplodedGraph_SingleBranchVisited_If()
        {
            const string testInput = "var a = false; bool b; if (a) { b = true; } else { b = false; } a = b;";
            var context = new ExplodedGraphContext(testInput);
            var aSymbol = context.GetSymbol("a");
            var bSymbol = context.GetSymbol("b");
            var numberOfLastInstructionVisits = 0;
            context.ExplodedGraph.InstructionProcessed +=
                (sender, args) =>
                {
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

            context.WalkWithInstructions(8);

            numberOfLastInstructionVisits.Should().Be(1);
        }

        [TestMethod]
        public void ExplodedGraph_SingleBranchVisited_And()
        {
            const string testInput = "var a = false; if (a && !a) { a = !true; } else { a = true; }";
            var context = new ExplodedGraphContext(testInput);
            var aSymbol = context.GetSymbol("a");
            context.ExplodedGraph.InstructionProcessed +=
                (sender, args) =>
                {
                    switch (args.Instruction.ToString())
                    {
                        case "a = true":
                            args.ProgramState.GetSymbolValue(aSymbol).Should().Be(SymbolicValue.True);
                            break;
                        case "a = !true":
                        case "!a":
                            Execute.Assertion.FailWith("We should never get into this branch");
                            break;
                        default:
                            // We are interested only in certain instructions.
                            break;
                    }
                };

            context.WalkWithInstructions(5);
        }

        [TestMethod]
        public void ExplodedGraph_BothBranchesVisited()
        {
            const string testInput = "var a = false; bool b; if (inParameter) { b = inParameter; } else { b = !inParameter; } a = b;";
            var context = new ExplodedGraphContext(testInput);
            var aSymbol = context.GetSymbol("a");
            var bSymbol = context.GetSymbol("b");
            var parameters = context.MainMethod.DescendantNodes().OfType<ParameterSyntax>();
            var inParameterSymbol = context.SemanticModel.GetDeclaredSymbol(parameters.First(d => d.Identifier.ToString() == "inParameter"));
            var numberOfLastInstructionVisits = 0;
            var visitedBlocks = new HashSet<Block>();
            var branchesVisited = 0;
            context.ExplodedGraph.InstructionProcessed +=
                (sender, args) =>
                {
                    visitedBlocks.Add(args.ProgramPoint.Block);

                    if (args.Instruction.ToString() == "a = false")
                    {
                        branchesVisited++;

                        args.ProgramState.GetSymbolValue(aSymbol).Should().Be(SymbolicValue.False);
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

            // Number of ExitBlocks is still 1 in this case:
            // All variables are dead at the ExitBlock, so whenever we get there,
            // the ExplodedGraph nodes should be the same, and thus should be processed only once.
            context.WalkWithInstructions(13);

            branchesVisited.Should().Be(4 + 1);
            numberOfLastInstructionVisits.Should().Be(2);
            visitedBlocks.Should().HaveCount(context.ControlFlowGraph.Blocks.Count() - 1 /* Exit block*/);
        }

        [TestMethod]
        public void ExplodedGraph_BothBranchesVisited_StateMerge()
        {
            const string testInput = "var a = !true; bool b; if (inParameter) { b = false; } else { b = false; } a = b;";
            var context = new ExplodedGraphContext(testInput);
            var aSymbol = context.GetSymbol("a");
            var numberOfLastInstructionVisits = 0;
            context.ExplodedGraph.InstructionProcessed +=
                (sender, args) =>
                {
                    if (args.Instruction.ToString() == "a = b")
                    {
                        args.ProgramState.GetSymbolValue(aSymbol).Should().Be(SymbolicValue.False);
                        numberOfLastInstructionVisits++;
                    }
                };

            context.WalkWithInstructions(11);

            numberOfLastInstructionVisits.Should().Be(1);
        }

        [TestMethod]
        public void ExplodedGraph_BothBranchesVisited_NonCondition()
        {
            const string testInput = "var str = this?.ToString();";
            var context = new ExplodedGraphContext(testInput);
            var visitedBlocks = new HashSet<Block>();
            var countConditionEvaluated = 0;
            context.ExplodedGraph.ConditionEvaluated += (sender, args) => { countConditionEvaluated++; };
            context.ExplodedGraph.InstructionProcessed +=
                (sender, args) =>
                {
                    visitedBlocks.Add(args.ProgramPoint.Block);
                };

            context.WalkWithInstructions(5);

            visitedBlocks.Should().HaveCount(context.ControlFlowGraph.Blocks.Count() - 1 /* Exit block */);
            countConditionEvaluated.Should().Be(0);
        }

        [TestMethod]
        public void ExplodedGraph_AllBranchesVisited()
        {
            const string testInput = "int i = 1; switch (i) { case 1: default: cw1(); break; case 2: cw2(); break; }";
            var context = new ExplodedGraphContext(testInput);
            var numberOfCw1InstructionVisits = 0;
            var numberOfCw2InstructionVisits = 0;
            context.ExplodedGraph.InstructionProcessed +=
                (sender, args) =>
                {
                    if (args.Instruction.ToString() == "cw1()")
                    {
                        numberOfCw1InstructionVisits++;
                    }
                    if (args.Instruction.ToString() == "cw2()")
                    {
                        numberOfCw2InstructionVisits++;
                    }
                };

            context.WalkWithInstructions(11);

            numberOfCw1InstructionVisits.Should().Be(2);
            numberOfCw2InstructionVisits.Should().Be(1);
        }

        [TestMethod]
        public void ExplodedGraph_NonDecisionMakingAssignments()
        {
            const string testInput = "var a = true; a |= false; var b = 42; b++; ++b;";
            var context = new ExplodedGraphContext(testInput);
            var aSymbol = context.GetSymbol("a");
            var bSymbol = context.GetSymbol("b");
            var branchesVisited = 0;
            SymbolicValue sv = null;
            context.ExplodedGraph.InstructionProcessed +=
                (sender, args) =>
                {
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

            context.WalkWithInstructions(11);

            branchesVisited.Should().Be(5);
        }

        [TestMethod]
        public void ExplodedGraph_NonLocalNorFieldSymbolBranching()
        {
            const string testInput = "if (Property) { cw(); }";
            var context = new ExplodedGraphContext(testInput);
            var propertySymbol = context.SemanticModel.GetSymbolInfo(context.MainMethod.DescendantNodes()
                .OfType<IdentifierNameSyntax>().First(d => d.Identifier.ToString() == "Property")).Symbol;
            propertySymbol.Should().NotBeNull();
            context.ExplodedGraph.InstructionProcessed +=
                (sender, args) =>
                {
                    if (args.Instruction.ToString() == "Property")
                    {
                        args.ProgramState.GetSymbolValue(propertySymbol).Should().BeNull();
                    }
                };

            context.WalkWithInstructions(3);
        }

        [TestMethod]
        public void ExplodedGraph_LoopExploration()
        {
            const string testInput = "var i = 0; while (i < 1) { i = i + 1; }";
            var context = new ExplodedGraphContext(testInput);
            var exceeded = 0;
            context.ExplodedGraph.ProgramPointVisitCountExceedLimit += (sender, args) =>
            {
                exceeded++;
                args.ProgramPoint.Block.Instructions.Should().Contain(i => i.ToString() == "i < 1");
            };

            context.ExplodedGraph.Walk();

            exceeded.Should().Be(1);
        }

        [TestMethod]
        public void ExplodedGraph_InternalStateCount_MaxReached()
        {
            if (TestContextHelper.IsAzureDevOpsContext) // ToDo: test throws OutOfMemory on Azure DevOps
            {
                return;
            }

            const string testInput = @"
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
            var context = new ExplodedGraphContext(TestHelper.CompileCS(testInput));
            var maxInternalStateCountReached = false;
            context.ExplodedGraph.MaxInternalStateCountReached += (sender, args) => { maxInternalStateCountReached = true; };

            context.ExplodedGraph.Walk();  // Special case, walk and check everythink manually

            maxInternalStateCountReached.Should().BeTrue();
            context.NumberOfExitBlockReached.Should().Be(0);
            context.ExplorationEnded.Should().BeFalse();
            context.MaxStepCountReached.Should().BeFalse();
        }

        [TestMethod]
        public void ExplodedGraph_StaticLocalFunctions()
        {
            const string testInput = @"static string Local(object o) {return o.ToString()} Local(null);";
            var context = new ExplodedGraphContext(testInput);
            var numberOfValidatedInstructions = 0;
            context.ExplodedGraph.InstructionProcessed += (sender, args) =>
            {
                if (args.Instruction.ToString() == "o.ToString()")
                {
                    numberOfValidatedInstructions++;
                }
            };

            context.WalkWithInstructions(3);

            numberOfValidatedInstructions.Should().Be(0);   // Local functions are not supported by CFG (yet)
        }

        [TestMethod]
        public void ExplodedGraph_DeclarationExpressionVisit_AsOutParameter_AddsNotNullConstraintForValueType()
        {
            const string testInput = @"
using System.Collections.Generic;

namespace Namespace
{
  public class DeclarationStatement
  {
    public void Main(Dictionary<string, int> dictionary, string key)
    {
        dictionary.TryGetValue(key, out var value);
    }
  }
}";
            var context = new ExplodedGraphContext(TestHelper.CompileCS(testInput));
            var dictionarySymbol = context.SemanticModel.GetDeclaredSymbol(context.MainMethod.ParameterList.Parameters.First());
            var valueSymbol = context.GetSymbol("value", ExplodedGraphContext.SymbolType.Declaration);
            context.ExplodedGraph.InstructionProcessed +=
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

                            args.ProgramState.GetSymbolValue(valueSymbol).Should().NotBeNull();
                            valueSymbol.HasConstraint(ObjectConstraint.NotNull, args.ProgramState).Should().BeTrue();
                            break;

                        case "dictionary.TryGetValue(key, out var value)":
                            args.ProgramState.GetSymbolValue(dictionarySymbol).Should().NotBeNull();
                            dictionarySymbol.HasConstraint(ObjectConstraint.NotNull, args.ProgramState).Should().BeTrue();

                            args.ProgramState.GetSymbolValue(valueSymbol).Should().NotBeNull();
                            valueSymbol.HasConstraint(ObjectConstraint.NotNull, args.ProgramState).Should().BeTrue();

                            args.ProgramState.HasValue.Should().BeFalse();
                            break;

                        default:
                            throw new NotImplementedException();
                    }
                };

            context.WalkWithInstructions(5);
        }

        [TestMethod]
        public void ExplodedGraph_DeclarationExpressionVisit_AsOutParameter_AddsNoConstraintForReferenceType()
        {
            const string testInput = @"
using System.Collections.Generic;

namespace Namespace
{
  public class DeclarationStatement
  {
    public void Main(Dictionary<string, string> dictionary, string key)
    {
        dictionary.TryGetValue(key, out var value);
    }
  }
}";
            var context = new ExplodedGraphContext(TestHelper.CompileCS(testInput));
            var dictionarySymbol = context.SemanticModel.GetDeclaredSymbol(context.MainMethod.ParameterList.Parameters.First());
            var valueSymbol = context.GetSymbol("value", ExplodedGraphContext.SymbolType.Declaration);
            context.ExplodedGraph.InstructionProcessed +=
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

                            args.ProgramState.GetSymbolValue(valueSymbol).Should().NotBeNull();
                            valueSymbol.HasConstraint(ObjectConstraint.NotNull, args.ProgramState).Should().BeFalse();
                            break;

                        case "dictionary.TryGetValue(key, out var value)":
                            args.ProgramState.GetSymbolValue(dictionarySymbol).Should().NotBeNull();
                            dictionarySymbol.HasConstraint(ObjectConstraint.NotNull, args.ProgramState).Should().BeTrue();

                            args.ProgramState.GetSymbolValue(valueSymbol).Should().NotBeNull();
                            valueSymbol.HasConstraint(ObjectConstraint.NotNull, args.ProgramState).Should().BeFalse();

                            args.ProgramState.HasValue.Should().BeFalse();
                            break;

                        default:
                            throw new NotImplementedException();
                    }
                };

            context.WalkWithInstructions(5);
        }

        [TestMethod]
        public void ExplodedGraph_DeclarationExpressionVisit_AsOutParameter_Discard()
        {
            const string testInput =
                "string key = null; Dictionary<string, int> dictionary = new Dictionary<string, int>(); dictionary.TryGetValue(key, out var _); dictionary.TryGetValue(key, out _);";
            var context = new ExplodedGraphContext(testInput);
            var dictionarySymbol = context.GetSymbol("dictionary");
            var instructionsInspected = 0;
            context.ExplodedGraph.InstructionProcessed +=
                (sender, args) =>
                {
                    var instruction = args.Instruction.ToString();
                    switch (instruction)
                    {
                        case "key":
                            args.ProgramState.HasValue.Should().BeTrue();
                            instructionsInspected++;
                            break;

                        case "dictionary.TryGetValue(key, out var _)":
                            args.ProgramState.GetSymbolValue(dictionarySymbol).Should().NotBeNull();
                            dictionarySymbol.HasConstraint(ObjectConstraint.NotNull, args.ProgramState).Should().BeTrue();

                            args.ProgramState.HasValue.Should().BeFalse();
                            instructionsInspected++;
                            break;

                        case "dictionary.TryGetValue(key, out _)":
                            args.ProgramState.GetSymbolValue(dictionarySymbol).Should().NotBeNull();
                            dictionarySymbol.HasConstraint(ObjectConstraint.NotNull, args.ProgramState).Should().BeTrue();

                            args.ProgramState.HasValue.Should().BeFalse();
                            instructionsInspected++;
                            break;

                        default:
                            // We are interested only in certain instructions.
                            break;
                    }
                };

            context.WalkWithInstructions(14);
            instructionsInspected.Should().Be(4);
        }

        [TestMethod]
        public void ExplodedGraph_VariableDesignationVisit_InsideIf()
        {
            const string testInput = "if (value is int x) { }";
            var context = new ExplodedGraphContext(testInput);
            var xSymbol = context.GetSymbol("x", ExplodedGraphContext.SymbolType.Declaration);
            context.ExplodedGraph.InstructionProcessed +=
                (sender, args) =>
                {
                    var instruction = args.Instruction.ToString();

                    if (instruction == "value is int x")
                    {
                        args.ProgramState.GetSymbolValue(xSymbol).Should().NotBeNull();
                        xSymbol.HasConstraint(ObjectConstraint.NotNull, args.ProgramState).Should().BeTrue();
                    }
                };

            context.WalkWithInstructions(2);
        }

        [TestMethod]
        public void ExplodedGraph_SwitchExpression_SimpleExpression()
        {
            const string testInput = @"string s = null; s = (s == null) switch { true => ""Value"", _ => s}; s.ToString();";
            var context = new ExplodedGraphContext(testInput);
            var sSymbol = context.GetSymbol("s");
            var numberOfValidatedInstructions = 0;
            context.ExplodedGraph.InstructionProcessed += (sender, args) =>
            {
                if (args.Instruction.ToString() == "s.ToString()")
                {
                    numberOfValidatedInstructions++;
                    args.ProgramState.HasConstraint(args.ProgramState.GetSymbolValue(sSymbol), ObjectConstraint.NotNull).Should().BeTrue();
                }
            };

            context.WalkWithInstructions(14);

            numberOfValidatedInstructions.Should().Be(1);
        }

        [TestMethod]
        public void ExplodedGraph_IsPatternVisit_DeclarationPattern_Discard()
        {
            const string testInput = @"var x = options is Options _;";

            var context = new ExplodedGraphContext(testInput);
            var xSymbol = context.GetSymbol("x");

            context.ExplodedGraph.InstructionProcessed += (sender, args) =>
            {
                var instruction = args.Instruction.ToString();

                switch (instruction)
                {
                    case "options":
                        args.ProgramState.HasValue.Should().BeTrue();
                        break;

                    case "options is Options _":
                        args.ProgramState.HasValue.Should().BeTrue();
                        break;

                    case "x = options is Options _":
                        xSymbol.HasConstraint(ObjectConstraint.NotNull, args.ProgramState).Should().BeTrue();

                        args.ProgramState.HasValue.Should().BeFalse();
                        break;

                    default:
                        throw new NotImplementedException();
                }
            };

            context.WalkWithInstructions(3);
        }

        [TestMethod]
        public void ExplodedGraph_SwitchStatement()
        {
            const string testInput = @"string s=null; switch(s==null) {case true: s=""Value""; break; default : break;}; s.ToString();";
            var context = new ExplodedGraphContext(testInput);
            var sSymbol = context.GetSymbol("s");
            var numberOfValidatedInstructions = 0;
            context.ExplodedGraph.InstructionProcessed += (sender, args) =>
            {
                if (args.Instruction.ToString() == "s.ToString()")
                {
                    numberOfValidatedInstructions++;
                    args.ProgramState.HasConstraint(args.ProgramState.GetSymbolValue(sSymbol), ObjectConstraint.NotNull).Should().BeTrue();
                }
            };

            context.WalkWithInstructions(14);

            numberOfValidatedInstructions.Should().Be(1);
        }

        [TestMethod]
        public void ExplodedGraph_SwitchWithRecursivePatternVisit()
        {
            const string testInput = @"
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
        public string Main(Person person)
        {
            return person switch
                {
                    { Address: {State: ""WA"" } address } p => address.Name,
                    _ => string.Empty
                };
        }
    }
}";
            var context = new ExplodedGraphContext(TestHelper.CompileCS(testInput));
            var personSymbol = context.SemanticModel.GetDeclaredSymbol(context.MainMethod.ParameterList.Parameters.First());
            var declarations = context.MainMethod.DescendantNodes().OfType<SingleVariableDesignationSyntax>().ToList();
            var addressSymbol = context.SemanticModel.GetDeclaredSymbol(declarations[0]);
            var pSymbol = context.SemanticModel.GetDeclaredSymbol(declarations[1]);
            var instructionsInspected = 0;
            context.ExplodedGraph.InstructionProcessed +=
                (sender, args) =>
                {
                    var instruction = args.Instruction.ToString();
                    switch (instruction)
                    {
                        case "person":
                            instructionsInspected++;
                            args.ProgramState.GetSymbolValue(personSymbol).Should().NotBeNull();
                            break;

                        case "{ Address: {State: \"WA\" } address } p":
                            instructionsInspected++;
                            args.ProgramState.GetSymbolValue(personSymbol).Should().NotBeNull();

                            // Currently the recursive pattern is ignored and the values for "p" and "address" are not created.
                            // https://github.com/SonarSource/sonar-dotnet/issues/2937
                            args.ProgramState.GetSymbolValue(addressSymbol).Should().BeNull();
                            args.ProgramState.GetSymbolValue(pSymbol).Should().BeNull();
                            break;

                        default:
                            // We are interested only in certain instructions.
                            break;
                    }
                };

            context.WalkWithInstructions(6);

            instructionsInspected.Should().Be(2);
        }

        [TestMethod]
        public void ExplodedGraph_IsPattern_WithRecursivePattern()
        {
            const string testInput = @"var x = address is Address { State: ""WA"" } addr;";

            var context = new ExplodedGraphContext(testInput);
            var addrSymbol = context.GetSymbol("addr", ExplodedGraphContext.SymbolType.Declaration);

            context.ExplodedGraph.InstructionProcessed += (sender, args) =>
            {
                var instruction = args.Instruction.ToString();

                switch (instruction)
                {
                    case "address":
                        args.ProgramState.GetSymbolValue(addrSymbol).Should().BeNull();
                        break;

                    case "Address { State: \"WA\" } addr":
                        args.ProgramState.GetSymbolValue(addrSymbol).Should().BeNull();
                        break;

                    case "address is Address { State: \"WA\" } addr":
                        args.ProgramState.GetSymbolValue(addrSymbol).Should().BeNull();
                        args.ProgramState.HasValue.Should().BeTrue();
                        break;

                    case "x = address is Address { State: \"WA\" } addr":
                        args.ProgramState.GetSymbolValue(addrSymbol).Should().BeNull();
                        args.ProgramState.HasValue.Should().BeFalse();
                        break;

                    default:
                        throw new NotImplementedException();
                }
            };

            context.WalkWithInstructions(4);
        }

        [TestMethod]
        public void ExplodedGraph_IsPattern_WithPositionalPattern()
        {
            const string testInput = @"var x = obj is (string s, int i) t;";

            var context = new ExplodedGraphContext(testInput);
            var tSymbol = context.GetSymbol("t", ExplodedGraphContext.SymbolType.Declaration);

            context.ExplodedGraph.InstructionProcessed += (sender, args) =>
            {
                var instruction = args.Instruction.ToString();

                switch (instruction)
                {
                    case "obj":
                        args.ProgramState.GetSymbolValue(tSymbol).Should().BeNull();
                        break;

                    case "(string s, int i) t":
                        args.ProgramState.GetSymbolValue(tSymbol).Should().BeNull();
                        break;

                    case "obj is (string s, int i) t":
                        args.ProgramState.GetSymbolValue(tSymbol).Should().BeNull();
                        args.ProgramState.HasValue.Should().BeTrue();
                        break;

                    case "x = obj is (string s, int i) t":
                        args.ProgramState.GetSymbolValue(tSymbol).Should().BeNull();
                        args.ProgramState.HasValue.Should().BeFalse();
                        break;

                    default:
                        throw new NotImplementedException();
                }
            };

            context.WalkWithInstructions(4);
        }

        [DataTestMethod]
        [DataRow("int")]
        [DataRow("(string s)")]
        [DataRow("1 or 2")]
        [DataRow("1 and 2")]
        [DataRow("not 1")]
        [DataRow("> 5")]
        [DataRow("[] empty")]
        public void ExplodedGraph_SwitchStatement_UnsupportedPatternKinds(string pattern)
        {
            var testInput = $$"""
                switch (new object())
                {
                    case {{pattern}}: break;
                }
                """;
            var context = new ExplodedGraphContext(testInput);
            context.WalkWithExitBlocks(2, 0);
        }

        [TestMethod]
        public void ExplodedGraph_SwitchExpressionVisit()
        {
            const string testInput = @"
namespace Namespace
{
  public class SwitchExpression
  {
    public int Main(string str)
    {
      return str switch { null => 1, """" => 2, _ => 3};
    }
  }
}";
            var context = new ExplodedGraphContext(TestHelper.CompileCS(testInput));
            var strParameter = context.MainMethod.ParameterList.Parameters.First();
            var strSymbol = context.SemanticModel.GetDeclaredSymbol(strParameter);
            context.ExplodedGraph.InstructionProcessed +=
                (sender, args) =>
                {
                    var instruction = args.Instruction.ToString();

                    switch (instruction)
                    {
                        case "1":
                            args.ProgramState.GetSymbolValue(strSymbol).Should().NotBe(SymbolicValue.Null);
                            strSymbol.HasConstraint(ObjectConstraint.Null, args.ProgramState).Should().BeTrue();
                            break;

                        case "2":
                            args.ProgramState.GetSymbolValue(strSymbol).Should().NotBe(SymbolicValue.Null);
                            strSymbol.HasConstraint(ObjectConstraint.NotNull, args.ProgramState).Should().BeTrue();
                            break;

                        case "3":
                            args.ProgramState.GetSymbolValue(strSymbol).Should().NotBe(SymbolicValue.Null);
                            strSymbol.HasConstraint(ObjectConstraint.NotNull, args.ProgramState).Should().BeTrue();
                            break;
                        default:
                            // We are interested only in certain instructions.
                            break;
                    }
                };

            context.WalkWithExitBlocks(7, 3);
        }

        [TestMethod]
        public void ExplodedGraph_SwitchExpression_AllPatterns()
        {
            const string testInput = @"string str = GetStr(); var x = str switch { null => 1, int i => 2, { } => 3, _ => 4};";
            var context = new ExplodedGraphContext(testInput);
            var strSymbol = context.GetSymbol("str");
            var instructionsInspected = 0;
            var assignmentInspected = 0;
            context.ExplodedGraph.InstructionProcessed +=
                (sender, args) =>
                {
                    var instruction = args.Instruction.ToString();

                    switch (instruction)
                    {
                        case "1":
                            args.ProgramState.GetSymbolValue(strSymbol).Should().NotBe(SymbolicValue.Null);
                            args.ProgramState.HasValue.Should().BeTrue();
                            strSymbol.HasConstraint(ObjectConstraint.Null, args.ProgramState).Should().BeTrue();
                            instructionsInspected++;
                            break;

                        case "2":
                            args.ProgramState.GetSymbolValue(strSymbol).Should().NotBe(SymbolicValue.Null);
                            args.ProgramState.HasValue.Should().BeTrue();
                            strSymbol.HasConstraint(ObjectConstraint.NotNull, args.ProgramState).Should().BeTrue();
                            instructionsInspected++;
                            break;

                        case "3":
                            args.ProgramState.GetSymbolValue(strSymbol).Should().NotBe(SymbolicValue.Null);
                            args.ProgramState.HasValue.Should().BeTrue();
                            strSymbol.HasConstraint(ObjectConstraint.NotNull, args.ProgramState).Should().BeTrue();
                            instructionsInspected++;
                            break;

                        case "4":
                            args.ProgramState.GetSymbolValue(strSymbol).Should().NotBe(SymbolicValue.Null);
                            args.ProgramState.HasValue.Should().BeTrue();
                            strSymbol.HasConstraint(ObjectConstraint.NotNull, args.ProgramState).Should().BeTrue();
                            instructionsInspected++;
                            break;

                        case @"x = str switch { null => 1, int i => 2, { } => 3, _ => 4}":
                            args.ProgramState.HasValue.Should().BeFalse();
                            instructionsInspected++;
                            assignmentInspected++;
                            break;

                        default:
                            // We are interested only in certain instructions.
                            break;
                    }
                };

            context.WalkWithInstructions(16);
            instructionsInspected.Should().Be(8);
            assignmentInspected.Should().Be(4);
        }

        [TestMethod]
        public void ExplodedGraph_SwitchExpression_NullConstraint()
        {
            const string testInput = @"const string a = null; string str = GetStr(); var x = str switch { a => 1, _ => 2};";
            var context = new ExplodedGraphContext(testInput);
            var strSymbol = context.GetSymbol("str");
            var instructionsInspected = 0;
            var assignmentInspected = 0;
            context.ExplodedGraph.InstructionProcessed +=
                (sender, args) =>
                {
                    var instruction = args.Instruction.ToString();

                    switch (instruction)
                    {
                        case "1":
                            args.ProgramState.GetSymbolValue(strSymbol).Should().NotBe(SymbolicValue.Null);
                            args.ProgramState.HasValue.Should().BeTrue();
                            strSymbol.HasConstraint(ObjectConstraint.Null, args.ProgramState).Should().BeTrue();
                            instructionsInspected++;
                            break;

                        case "2":
                            args.ProgramState.GetSymbolValue(strSymbol).Should().NotBe(SymbolicValue.Null);
                            args.ProgramState.HasValue.Should().BeTrue();
                            strSymbol.HasConstraint(ObjectConstraint.NotNull, args.ProgramState).Should().BeTrue();
                            instructionsInspected++;
                            break;

                        case @"x = str switch { a => 1, _ => 2}":
                            args.ProgramState.HasValue.Should().BeFalse();
                            instructionsInspected++;
                            assignmentInspected++;
                            break;

                        default:
                            // We are interested only in certain instructions.
                            break;
                    }
                };

            context.WalkWithInstructions(11);
            instructionsInspected.Should().Be(4);
            assignmentInspected.Should().Be(2);
        }

        [TestMethod]
        public void ExplodedGraph_NullCoalesceAssignmentVisit()
        {
            const string testInput = @"string s = null; s ??= ""N/A""; s.ToString();";
            var context = new ExplodedGraphContext(testInput);
            var sSymbol = context.GetSymbol("s");
            context.ExplodedGraph.InstructionProcessed +=
                (sender, args) =>
                {
                    var instruction = args.Instruction.ToString();
                    switch (instruction)
                    {
                        case "s = null":
                            args.ProgramState.GetSymbolValue(sSymbol).Should().NotBeNull();
                            sSymbol.HasConstraint(ObjectConstraint.NotNull, args.ProgramState).Should().BeFalse();
                            break;

                        case "s ??= \"N/A\"":
                            args.ProgramState.GetSymbolValue(sSymbol).Should().NotBeNull();
                            sSymbol.HasConstraint(ObjectConstraint.NotNull, args.ProgramState).Should().BeTrue();
                            break;

                        default:
                            // We are interested only in certain instructions.
                            break;
                    }
                };

            context.WalkWithInstructions(8);
        }

        [TestMethod]
        public void ExplodedGraph_DefaultLiteral()
        {
            const string testInput = "var i = default(int); int j = default; System.IO.File k = default;";
            var context = new ExplodedGraphContext(testInput);
            var iSymbol = context.GetSymbol("i");
            var jSymbol = context.GetSymbol("j");
            var kSymbol = context.GetSymbol("k");
            context.ExplodedGraph.InstructionProcessed +=
                (sender, args) =>
                {
                    var instruction = args.Instruction.ToString();
                    if (instruction == "i = default(int)")
                    {
                        args.ProgramState.GetSymbolValue(iSymbol).Should().NotBeNull();
                        iSymbol.HasConstraint(ObjectConstraint.NotNull, args.ProgramState).Should().BeTrue();
                    }
                    if (instruction == "j = default")
                    {
                        args.ProgramState.GetSymbolValue(jSymbol).Should().NotBeNull();
                        jSymbol.HasConstraint(ObjectConstraint.NotNull, args.ProgramState).Should().BeTrue();
                    }
                    if (instruction == "k = default")
                    {
                        args.ProgramState.GetSymbolValue(kSymbol).Should().NotBeNull();
                        kSymbol.HasConstraint(ObjectConstraint.NotNull, args.ProgramState).Should().BeFalse();
                        kSymbol.HasConstraint(ObjectConstraint.Null, args.ProgramState).Should().BeTrue();
                    }
                };

            context.WalkWithInstructions(6);
        }

        [TestMethod]
        public void ExplodedGraph_TupleExpressions()
        {
            const string testInput = "var myTuple = (1, 2); (object a, object b) c = (1, null); (object d, object e) = (1, null);";
            var context = new ExplodedGraphContext(testInput);
            var myTupleSymbol = context.GetSymbol("myTuple");
            var cSymbol = context.GetSymbol("c");
            context.ExplodedGraph.InstructionProcessed +=
                (sender, args) =>
                {
                    var instruction = args.Instruction.ToString();

                    if (instruction == "myTuple = (1, 2)")
                    {
                        args.ProgramState.GetSymbolValue(myTupleSymbol).Should().NotBeNull();
                    }
                    if (instruction == "c = (1, null)")
                    {
                        args.ProgramState.GetSymbolValue(cSymbol).Should().NotBeNull();
                    }

                    // Symbolic value for tuples are in the stack for compatibility
                    if (instruction == "(object d, object e)")
                    {
                        args.ProgramState.HasValue.Should().BeTrue();
                    }
                    // Stack is clean after assignment
                    if (instruction == "(object d, object e) = (1, null)")
                    {
                        args.ProgramState.HasValue.Should().BeFalse();
                    }
                };

            context.WalkWithInstructions(15);
        }

        [TestMethod]
        public void ExplodedGraph_DeclarationExpression()
        {
            const string testInput = @"
namespace Namespace
{
    public class DeclarationExpression
    {
        public void Main(IDecoder decoder)
        {
            var result = decoder.Convert(out int bytesUsed, out int charsUsed);
        }
    }
    public interface IDecoder
    {
        bool Convert(out int bytesUsed, out int charsUsed);
    }
}";
            var context = new ExplodedGraphContext(TestHelper.CompileCS(testInput));
            var bytesUsedSymbol = context.GetSymbol("bytesUsed", ExplodedGraphContext.SymbolType.Declaration);
            var charsUsedSymbol = context.GetSymbol("charsUsed", ExplodedGraphContext.SymbolType.Declaration);
            context.ExplodedGraph.InstructionProcessed +=
                (sender, args) =>
                {
                    var instruction = args.Instruction.ToString();

                    switch (instruction)
                    {
                        case "decoder":
                            args.ProgramState.GetSymbolValue(bytesUsedSymbol).Should().BeNull();
                            args.ProgramState.GetSymbolValue(charsUsedSymbol).Should().BeNull();
                            break;

                        case "decoder.Convert":
                            args.ProgramState.GetSymbolValue(bytesUsedSymbol).Should().BeNull();
                            args.ProgramState.GetSymbolValue(charsUsedSymbol).Should().BeNull();
                            break;

                        case "int bytesUsed":
                            args.ProgramState.GetSymbolValue(bytesUsedSymbol).Should().NotBeNull();
                            args.ProgramState.GetSymbolValue(charsUsedSymbol).Should().BeNull();
                            break;

                        case "int charsUsed":
                            args.ProgramState.GetSymbolValue(bytesUsedSymbol).Should().NotBeNull();
                            args.ProgramState.GetSymbolValue(charsUsedSymbol).Should().NotBeNull();
                            break;

                        case "decoder.Convert(out int bytesUsed, out int charsUsed)":
                            args.ProgramState.GetSymbolValue(bytesUsedSymbol).Should().NotBeNull();
                            args.ProgramState.GetSymbolValue(charsUsedSymbol).Should().NotBeNull();
                            break;

                        case "result = decoder.Convert(out int bytesUsed, out int charsUsed)":
                            args.ProgramState.GetSymbolValue(bytesUsedSymbol).Should().NotBeNull();
                            args.ProgramState.GetSymbolValue(charsUsedSymbol).Should().NotBeNull();
                            args.ProgramState.HasValue.Should().BeFalse();
                            break;

                        default:
                            // We are interested only in certain instructions.
                            break;
                    }
                };

            context.WalkWithInstructions(6);
        }

        [TestMethod]
        public void ExplodedGraph_RefExpressions()
        {
            const string testInput = @"
using System;

namespace Test
{
    public class Program
    {
        public static Program Empty = new Program();
        protected ref readonly Program Main() => ref Empty;
    }
}
";
            var context = new ExplodedGraphContext(TestHelper.CompileCS(testInput));

            var identifierSymbol = context.GetSymbol("Empty", ExplodedGraphContext.SymbolType.Identifier);

            context.ExplodedGraph.InstructionProcessed +=
                (sender, args) =>
                {
                    args.ProgramPoint.Block.Instructions.Should().HaveCount(2);
                    args.ProgramPoint.Block.Instructions.Should().Contain(i => i.ToString() == "Empty");
                    args.ProgramPoint.Block.Instructions.Should().Contain(i => i.ToString() == "ref Empty");
                    args.ProgramState.GetSymbolValue(identifierSymbol).Should().NotBeNull();
                };

            context.WalkWithInstructions(2);
        }

        [TestMethod]
        public void ExplodedGraph_TupleExpressionsDeconstruct()
        {
            const string testInput = "var (projectInstance, diagnostics) = loader;";

            var context = new ExplodedGraphContext(testInput);
            var projectInstanceSymbol = context.GetSymbol("projectInstance", ExplodedGraphContext.SymbolType.Declaration);
            var diagnosticsSymbol = context.GetSymbol("diagnostics", ExplodedGraphContext.SymbolType.Declaration);

            context.ExplodedGraph.InstructionProcessed +=
                (sender, args) =>
                {
                    var instruction = args.Instruction.ToString();

                    switch (instruction)
                    {
                        case "var (projectInstance, diagnostics)":
                            args.ProgramState.GetSymbolValue(projectInstanceSymbol).Should().NotBeNull();
                            args.ProgramState.GetSymbolValue(diagnosticsSymbol).Should().NotBeNull();
                            break;

                        case "loader":
                            args.ProgramState.GetSymbolValue(projectInstanceSymbol).Should().NotBeNull();
                            args.ProgramState.GetSymbolValue(diagnosticsSymbol).Should().NotBeNull();
                            break;

                        case "var (projectInstance, diagnostics) = loader":
                            args.ProgramState.GetSymbolValue(projectInstanceSymbol).Should().NotBeNull();
                            args.ProgramState.GetSymbolValue(diagnosticsSymbol).Should().NotBeNull();
                            args.ProgramState.HasValue.Should().BeFalse();
                            break;

                        default:
                            // We are interested only in certain instructions.
                            break;
                    }
                };

            context.WalkWithInstructions(3);
        }

        [TestMethod]
        public void ExplodedGraph_TupleExpressionsWithDiscardDeconstruct()
        {
            const string testInput = "var (projectInstance, _) = loader;";

            var context = new ExplodedGraphContext(testInput);
            var projectInstanceSymbol = context.GetSymbol("projectInstance", ExplodedGraphContext.SymbolType.Declaration);

            context.ExplodedGraph.InstructionProcessed +=
                (sender, args) =>
                {
                    var instruction = args.Instruction.ToString();

                    switch (instruction)
                    {
                        case "var (projectInstance, _)":
                            args.ProgramState.GetSymbolValue(projectInstanceSymbol).Should().NotBeNull();
                            break;

                        case "loader":
                            args.ProgramState.GetSymbolValue(projectInstanceSymbol).Should().NotBeNull();
                            break;

                        case "var (projectInstance, _) = loader":
                            args.ProgramState.GetSymbolValue(projectInstanceSymbol).Should().NotBeNull();
                            args.ProgramState.HasValue.Should().BeFalse();
                            break;

                        default:
                            // We are interested only in certain instructions.
                            break;
                    }
                };

            context.WalkWithInstructions(3);
        }

        [TestMethod]
        public void ExplodedGraph_DiscardDesignationAsFunctionArgument()
        {
            const string testInput = @"var result = obj.Read(out int _);";

            var context = new ExplodedGraphContext(testInput);
            var resultSymbol = context.GetSymbol("result");

            context.ExplodedGraph.InstructionProcessed +=
                (sender, args) =>
                {
                    var instruction = args.Instruction.ToString();

                    switch (instruction)
                    {
                        case "obj":
                            args.ProgramState.GetSymbolValue(resultSymbol).Should().BeNull();
                            break;

                        case "obj.Read":
                            args.ProgramState.GetSymbolValue(resultSymbol).Should().BeNull();
                            break;

                        case "int _":
                            args.ProgramState.GetSymbolValue(resultSymbol).Should().BeNull();
                            break;

                        case "obj.Read(out int _)":
                            args.ProgramState.GetSymbolValue(resultSymbol).Should().BeNull();
                            break;

                        case "result = obj.Read(out int _)":
                            args.ProgramState.GetSymbolValue(resultSymbol).Should().NotBeNull();
                            args.ProgramState.HasValue.Should().BeFalse();
                            break;

                        default:
                            // We are interested only in certain instructions.
                            break;
                    }
                };

            context.WalkWithInstructions(5);
        }

        [TestMethod]
        public void ExplodedGraph_CoalesceAssignmentOnProperty()
        {
            var context = new ExplodedGraphContext("return options.Property ??= 1");

            context.ExplodedGraph.InstructionProcessed +=
                (sender, args) =>
                {
                    var instruction = args.Instruction.ToString();

                    if (instruction != "options.Property ??= 1")
                    {
                        return;
                    }

                    // The symbolic value corresponding to the expression result should have the NotNull constraint an all branches.
                    args.ProgramState.HasConstraint(args.ProgramState.PeekValue(), ObjectConstraint.NotNull).Should().BeTrue();
                };

            context.WalkWithExitBlocks(5, 2);
        }

#if NET

        [TestMethod]
        public void ExplodedGraph_IndiceFromVariable()
        {
            const string testInput = @"
namespace Namespace
{
    public class CollectionInitializer
    {
        public System.Index Main(int i)
        {
            return ^i;
        }
    }
}";
            var context = new ExplodedGraphContext(TestHelper.CompileCS(testInput));
            var isIndiceVisited = false;

            context.ExplodedGraph.InstructionProcessed +=
                (sender, args) =>
                {
                    isIndiceVisited = isIndiceVisited || args.Instruction.ToString() == "^i";
                };

            context.WalkWithExitBlocks(1, 1);
            isIndiceVisited.Should().BeTrue();
        }

        [TestMethod]
        public void ExplodedGraph_CollectionInitializerWithIndice()
        {
            const string testInput = @"
namespace Namespace
{
    public class CollectionInitializer
    {
        public string[] Main(string[] list)
        {
            return new[] { list[^1] };
        }
    }
}";
            var context = new ExplodedGraphContext(TestHelper.CompileCS(testInput));
            var isIndexerVisited = false;

            context.ExplodedGraph.InstructionProcessed +=
                (sender, args) =>
                {
                    isIndexerVisited = isIndexerVisited || args.Instruction.ToString() == "list[^1]";
                };

            context.WalkWithExitBlocks(5, 1);
            isIndexerVisited.Should().BeTrue();
        }

        [TestMethod]
        public void ExplodedGraph_RangeFromVariables()
        {
            const string testInput = @"
namespace Namespace
{
    public class CollectionInitializer
    {
        public System.Range Main(int min, int max)
        {
            return min..max;
        }
    }
}";
            var context = new ExplodedGraphContext(TestHelper.CompileCS(testInput));
            var isRangeVisited = false;

            context.ExplodedGraph.InstructionProcessed +=
                (sender, args) =>
                {
                    isRangeVisited = isRangeVisited || args.Instruction.ToString() == "min..max";
                };

            context.WalkWithExitBlocks(1, 1);
            isRangeVisited.Should().BeTrue();
        }

        [TestMethod]
        public void ExplodedGraph_CollectionInitializerWithRange()
        {
            const string testInput = @"
namespace Namespace
{
    using System.Collections.Generic;

    public class CollectionInitializer
    {
        public List<string[]> Main(string[] list)
        {
            return new List<string[]> { list[2..4] };
        }
    }
}";
            var context = new ExplodedGraphContext(TestHelper.CompileCS(testInput));
            var isIndexerVisited = false;

            context.ExplodedGraph.InstructionProcessed +=
                (sender, args) =>
                isIndexerVisited = isIndexerVisited || args.Instruction.ToString() == "list[2..4]";

            context.WalkWithExitBlocks(5, 1);
            isIndexerVisited.Should().BeTrue();
        }

#endif

        private class ExplodedGraphContext
        {
            public readonly SemanticModel SemanticModel;
            public readonly MethodDeclarationSyntax MainMethod;
            public readonly IMethodSymbol MainMethodSymbol;
            public readonly SonarCSharpLiveVariableAnalysis LiveVariableAnalysis;
            public readonly IControlFlowGraph ControlFlowGraph;
            public readonly SonarExplodedGraph ExplodedGraph;

            public bool ExplorationEnded;
            public bool MaxStepCountReached;
            public int NumberOfExitBlockReached;
            public int NumberOfProcessedInstructions;

            public ExplodedGraphContext(string methodBody)
                : this(SonarControlFlowGraphTest.CompileWithMethodBody(string.Format(TestInput, methodBody)))
            { }

            public ExplodedGraphContext((SyntaxTree tree, SemanticModel semanticModel) compilation)
                : this((compilation.tree.GetRoot().DescendantNodes().OfType<MethodDeclarationSyntax>().Single(m => m.Identifier.ValueText == "Main"), compilation.semanticModel))
            { }

            private ExplodedGraphContext((MethodDeclarationSyntax MainMethod, SemanticModel Model) tuple)
            {
                MainMethod = tuple.MainMethod;
                SemanticModel = tuple.Model;
                MainMethodSymbol = tuple.Model.GetDeclaredSymbol(MainMethod);
                var methodBody = (CSharpSyntaxNode)MainMethod.Body ?? MainMethod.ExpressionBody;
                ControlFlowGraph = CSharpControlFlowGraph.Create(methodBody, tuple.Model);
                LiveVariableAnalysis = new SonarCSharpLiveVariableAnalysis(ControlFlowGraph, MainMethodSymbol, tuple.Model, default);
                ExplodedGraph = new SonarExplodedGraph(ControlFlowGraph, MainMethodSymbol, tuple.Model, LiveVariableAnalysis);
                ExplodedGraph.InstructionProcessed += (sender, args) => { NumberOfProcessedInstructions++; };
                ExplodedGraph.ExplorationEnded += (sender, args) => { ExplorationEnded = true; };
                ExplodedGraph.MaxStepCountReached += (sender, args) => { MaxStepCountReached = true; };
                ExplodedGraph.ExitBlockReached += (sender, args) => { NumberOfExitBlockReached++; };
            }

            public enum SymbolType
            {
                Variable,
                Identifier,
                Declaration
            }

            public void WalkWithExitBlocks(int expectedProcessedInstructions, int expectedExitBlocks) =>
                WalkAndCheck(expectedProcessedInstructions, expectedExitBlocks);

            public void WalkWithInstructions(int expectedProcessedInstructions) =>
                WalkAndCheck(expectedProcessedInstructions, 1);

            internal ISymbol GetSymbol(string identifier, SymbolType st = SymbolType.Variable)
            {
                var expression = st switch
                {
                    SymbolType.Variable => MainMethod
                        .DescendantNodes()
                        .OfType<VariableDeclaratorSyntax>()
                        .First(d => d.Identifier.ToString() == identifier),

                    SymbolType.Identifier => (CSharpSyntaxNode)MainMethod
                        .DescendantNodes()
                        .OfType<IdentifierNameSyntax>()
                        .First(d => d.Identifier.ToString() == identifier),

                    SymbolType.Declaration => MainMethod
                        .DescendantNodes()
                        .OfType<SingleVariableDesignationSyntax>()
                        .First(d => d.Identifier.Text == identifier),

                    _ => throw new NotSupportedException()
                };

                return SemanticModel.GetDeclaredSymbol(expression)
                    ?? SemanticModel.GetSymbolOrCandidateSymbol(expression);
            }

            private void WalkAndCheck(int expectedProcessedInstructions, int expectedExitBlocks)
            {
                ExplodedGraph.Walk();
                ExplorationEnded.Should().Be(true);
                NumberOfProcessedInstructions.Should().Be(expectedProcessedInstructions);
                NumberOfExitBlockReached.Should().Be(expectedExitBlocks);
                MaxStepCountReached.Should().Be(false);
            }
        }

    }
}
