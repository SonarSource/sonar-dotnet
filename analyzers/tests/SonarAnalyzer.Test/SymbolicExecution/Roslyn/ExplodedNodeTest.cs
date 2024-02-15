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

using System.Xml.Linq;
using SonarAnalyzer.Extensions;
using SonarAnalyzer.SymbolicExecution.Roslyn;

namespace SonarAnalyzer.Test.SymbolicExecution.Roslyn;

[TestClass]
public class ExplodedNodeTest
{
    [TestMethod]
    public void Constructor_NullState_Throws()
    {
        var cfg = TestHelper.CompileCfgBodyCS();
        ((Action)(() => new ExplodedNode(cfg.EntryBlock, null, null))).Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("state");
    }

    [TestMethod]
    public void CreateNext_NullState_Throws()
    {
        var cfg = TestHelper.CompileCfgBodyCS();
        var validNode = new ExplodedNode(cfg.EntryBlock, ProgramState.Empty, null);
        ((Action)(() => validNode.CreateNext(null))).Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("state");
    }

    [TestMethod]
    public void FromBasicBlock_Empty_HasNullOperations()
    {
        var cfg = TestHelper.CompileCfgBodyCS();
        var sut = new ExplodedNode(cfg.EntryBlock, ProgramState.Empty, null);
        sut.Operation.Instance.Should().BeNull();
    }

    [TestMethod]
    public void IteratesExecutionOrder_CS()
    {
        var block = TestHelper.CompileCfgBodyCS("var value = 42;").Blocks[1];
        // Visualize operations to understand the rest of this UT
        block.OperationsAndBranchValue.ToExecutionOrder().Select(TestHelper.Serialize).Should().Equal(
             "LocalReference: value = 42 (Implicit)",
             "Literal: 42",
             "SimpleAssignment: value = 42 (Implicit)");

        var current = new ExplodedNode(block, ProgramState.Empty, null);
        TestHelper.Serialize(current.Operation).Should().Be("LocalReference: value = 42 (Implicit)");

        current = current.CreateNext(ProgramState.Empty);
        TestHelper.Serialize(current.Operation).Should().Be("Literal: 42");

        current = current.CreateNext(ProgramState.Empty);
        TestHelper.Serialize(current.Operation).Should().Be("SimpleAssignment: value = 42 (Implicit)");

        current = current.CreateNext(ProgramState.Empty);
        current.Operation.Instance.Should().BeNull();
    }

    [TestMethod]
    public void IteratesExecutionOrder_VB()
    {
        var block = TestHelper.CompileCfgBodyVB("Dim Value As Integer = 42").Blocks[1];
        // Visualize operations to understand the rest of this UT
        block.OperationsAndBranchValue.ToExecutionOrder().Select(TestHelper.Serialize).Should().Equal(
            "LocalReference: Value (Implicit)",
            "Literal: 42",
            "SimpleAssignment: Value As Integer = 42 (Implicit)");

        var sut = new ExplodedNode(block, ProgramState.Empty, null);
        TestHelper.Serialize(sut.Operation).Should().Be("LocalReference: Value (Implicit)");

        sut = sut.CreateNext(ProgramState.Empty);
        TestHelper.Serialize(sut.Operation).Should().Be("Literal: 42");

        sut = sut.CreateNext(ProgramState.Empty);
        TestHelper.Serialize(sut.Operation).Should().Be("SimpleAssignment: Value As Integer = 42 (Implicit)");

        sut = sut.CreateNext(ProgramState.Empty);
        sut.Operation.Instance.Should().BeNull();
    }

    [TestMethod]
    public void Equals_ReturnsTrueForEquivalent()
    {
        var block = TestHelper.CompileCfgBodyCS("var a = true;").Blocks[1];
        var basic = new ExplodedNode(block, ProgramState.Empty, null);
        var same = new ExplodedNode(block, ProgramState.Empty, null);
        var differentLocation = basic.CreateNext(ProgramState.Empty);
        var differentState = new ExplodedNode(block, ProgramState.Empty.SetOperationValue(block.Operations[0], SymbolicValue.Empty), null);

        basic.Equals(same).Should().BeTrue();
        basic.Equals(differentLocation).Should().BeFalse();
        basic.Equals(differentState).Should().BeFalse();
        basic.Equals("different type").Should().BeFalse();
        basic.Equals((object)null).Should().BeFalse();
        basic.Equals((ExplodedNode)null).Should().BeFalse();    // Explicit cast to ensure correct overload
    }

    [TestMethod]
    public void Equals_FinallyPoints_Single()
    {
        var cfg = TestHelper.CompileCfgBodyCS("""
            try
            {
                if (condition)
                {
                    return; // Branch goes to exit 1->5, skipping AfterFinally
                }
                if (condition)
                {
                    return; // Branch goes to exit 2->5, skipping AfterFinally
                }
                // Branch goes AfterFinally 2->4
            }
            finally
            {
                System.Console.WriteLine("Finally");
            }
            System.Console.WriteLine("AfterFinally");
            """, "bool condition");
        var block = cfg.Blocks[0];
        var from1to5 = cfg.Blocks[1].Successors.Single(x => x.Destination.Ordinal == 5);
        var from2to5 = cfg.Blocks[2].Successors.Single(x => x.Destination.Ordinal == 5);
        var from2to4 = cfg.Blocks[2].Successors.Single(x => x.Destination.Ordinal == 4);
        var node = new ExplodedNode(block, ProgramState.Empty, new FinallyPoint(null, from1to5));
        var noFinally = new ExplodedNode(block, ProgramState.Empty, null);

        node.Equals(new ExplodedNode(block, ProgramState.Empty, new FinallyPoint(null, from2to5))).Should().BeTrue("We consider only same destination block");
        node.Equals(new ExplodedNode(block, ProgramState.Empty, new FinallyPoint(null, from2to4))).Should().BeFalse("It has different destination block");
        node.Equals(new ExplodedNode(block, ProgramState.Empty, new FinallyPoint(null, from1to5, 1))).Should().BeFalse("It has different BlockIndex");
        node.Equals(noFinally).Should().BeFalse();
        noFinally.Equals(node).Should().BeFalse();
    }

    [TestMethod]
    public void Equals_FinallyPoints_WithPrevious()
    {
        var cfg = TestHelper.CompileCfgBodyCS("""
            try
            {
                if (condition)
                {
                    return; // Branch goes to exit, skipping AfterFinally
                }
                if (condition)
                {
                    return; // Branch goes to exit, skipping AfterFinally
                }
                // Branch goes AfterFinally
            }
            finally
            {
                System.Console.WriteLine("Finally");
            }
            System.Console.WriteLine("AfterFinally");
            """, "bool condition");
        var block = cfg.Blocks[0];
        var outerSame = cfg.Blocks[0].Successors.Single();
        var from1to5 = cfg.Blocks[1].Successors.Single(x => x.Destination.Ordinal == 5);
        var from2to5 = cfg.Blocks[2].Successors.Single(x => x.Destination.Ordinal == 5);
        var from2to4 = cfg.Blocks[2].Successors.Single(x => x.Destination.Ordinal == 4);
        var node = new ExplodedNode(block, ProgramState.Empty, new FinallyPoint(new FinallyPoint(null, from1to5), outerSame));
        var noFinally = new ExplodedNode(block, ProgramState.Empty, Wrap(null));

        node.Equals(new ExplodedNode(block, ProgramState.Empty, Wrap(new FinallyPoint(null, from2to5)))).Should().BeTrue("We consider only same destination block");
        node.Equals(new ExplodedNode(block, ProgramState.Empty, Wrap(new FinallyPoint(null, from2to4)))).Should().BeFalse("It has different destination block");
        node.Equals(new ExplodedNode(block, ProgramState.Empty, Wrap(new FinallyPoint(null, from1to5, 1)))).Should().BeFalse("It has different BlockIndex");
        node.Equals(noFinally).Should().BeFalse();
        noFinally.Equals(node).Should().BeFalse();

        FinallyPoint Wrap(FinallyPoint inner) =>
            new(inner, outerSame);
    }

    [TestMethod]
    public void GetHashCode_ReturnsSameForEquivalent()
    {
        var cfg = TestHelper.CompileCfgBodyCS("var a = true;");
        var block = cfg.Blocks[1];
        var finallyPoint = new FinallyPoint(null, block.Successors.Single());
        var basic = new ExplodedNode(block, ProgramState.Empty, finallyPoint);
        var same = new ExplodedNode(block, ProgramState.Empty, finallyPoint);
        var differentLocation = basic.CreateNext(ProgramState.Empty);
        var differentState = new ExplodedNode(block, ProgramState.Empty.SetOperationValue(block.Operations[0], SymbolicValue.Empty), finallyPoint);
        var differentNoFinallyPoint = new ExplodedNode(block, ProgramState.Empty, null);
        var differentFinallyPointBlockIndex = new ExplodedNode(block, ProgramState.Empty, new(null, cfg.Blocks[0].Successors.Single()));

        basic.GetHashCode().Should().Be(basic.GetHashCode(), "value should be deterministic");
        basic.GetHashCode().Should().Be(same.GetHashCode());
        basic.GetHashCode().Should().NotBe(differentLocation.GetHashCode());
        basic.GetHashCode().Should().NotBe(differentState.GetHashCode());
        basic.GetHashCode().Should().NotBe(differentNoFinallyPoint.GetHashCode());
        basic.GetHashCode().Should().NotBe(differentFinallyPointBlockIndex.GetHashCode());
    }

    [TestMethod]
    public void ToString_SerializeOperationAndState()
    {
        var cfg = TestHelper.CompileCfgBodyCS("var a = true;");
        var state = ProgramState.Empty.SetSymbolValue(cfg.Blocks[1].Operations[0].ChildOperations.First().TrackedSymbol(ProgramState.Empty), SymbolicValue.Empty);

        new ExplodedNode(cfg.Blocks[0], ProgramState.Empty, null).ToString().Should().BeIgnoringLineEndings("""
            Block #0, Branching
            Empty

            """);

        new ExplodedNode(cfg.Blocks[1], state, null).ToString().Should().BeIgnoringLineEndings("""
            Block #1, Operation #0, LocalReferenceOperation: a = true
            Symbols:
            a: No constraints

            """);
        new ExplodedNode(cfg.ExitBlock, state, null).ToString().Should().BeIgnoringLineEndings("""
            Block #2, Branching
            Symbols:
            a: No constraints

            """);
    }

    [TestMethod]
    public void AddVisit_ModifiesState()
    {
        var cfg = TestHelper.CompileCfgBodyCS("var a = true;");
        var sut = new ExplodedNode(cfg.Blocks[1], ProgramState.Empty, null);
        sut.State.Should().Be(ProgramState.Empty);
        sut.AddVisit().Should().Be(1);
        sut.AddVisit().Should().Be(2);
        sut.AddVisit().Should().Be(3);
        sut.State.Should().Be(ProgramState.Empty, "Visits doesn't change equality");
        ReferenceEquals(sut.State, ProgramState.Empty).Should().BeFalse();
    }

    [TestMethod]
    public void CreateNext_PreservesFinallyBlock()
    {
        var cfg = TestHelper.CompileCfgBodyCS("var value = 42;");
        var finallyPoint = new FinallyPoint(null, cfg.Blocks[0].FallThroughSuccessor);
        var sut = new ExplodedNode(cfg.Blocks[1], ProgramState.Empty, finallyPoint);
        sut.FinallyPoint.Should().NotBeNull();
        sut = sut.CreateNext(ProgramState.Empty);
        sut.FinallyPoint.Should().NotBeNull();
    }
}
