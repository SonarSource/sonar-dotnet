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
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SonarAnalyzer.CFG.Roslyn;
using SonarAnalyzer.Extensions;
using SonarAnalyzer.UnitTest.Helpers;

namespace SonarAnalyzer.UnitTest.CFG.Roslyn
{
    [TestClass]
    public class ControlFlowBranchTest
    {
        [TestMethod]
        public void ValidateReflection()
        {
            const string code = @"
public class Sample
{
    public void Method(bool condition) { } // Empty, just Entry and Exit block
}";
            var cfg = TestHelper.CompileCfg(code);
            var entry = cfg.EntryBlock;
            var exit = cfg.ExitBlock;
            entry.Kind.Should().Be(BasicBlockKind.Entry);
            entry.Operations.Should().BeEmpty();
            entry.BranchValue.Should().BeNull();
            entry.OperationsAndBranchValue.Should().BeEmpty();
            exit.Kind.Should().Be(BasicBlockKind.Exit);
            exit.Operations.Should().BeEmpty();
            exit.Operations.Should().BeEmpty();
            exit.BranchValue.Should().BeNull();
            exit.OperationsAndBranchValue.Should().BeEmpty();

            var branch = entry.FallThroughSuccessor;
            branch.Source.Should().Be(entry);
            branch.Destination.Should().Be(exit);
            branch.Semantics.Should().Be(ControlFlowBranchSemantics.Regular);
            branch.IsConditionalSuccessor.Should().Be(false);
            branch.EnteringRegions.Should().BeEmpty();
            branch.LeavingRegions.Should().BeEmpty();
            branch.FinallyRegions.Should().BeEmpty();
        }

        [TestMethod]
        public void ValidateReflection_Regions()
        {
            const string code = @"
public class Sample
{
    public void Method(bool condition)
    {
        field = 0;
        try
        {
            field = 1;
        }
        finally
        {
            field = 42;
        }
    }
}";
            var cfg = TestHelper.CompileCfg(code);
            /*
             *          Entry 0
             *            |
             *            V
             *          Block 1
             *          field = 0
             *            |
             *  +---------+-- TryAndFinally region ------------------------------+
             *  |         |                                                      |
             *  |  +--Try-+-region -+  +-- Finally region --------------------+  |
             *  |  |      |         |  |                                      |  |
             *  |  |      v         |  |    Block 3                           |  |
             *  |  |    Block 2     |  |    field = 42                        |  |
             *  |  |    field = 1   |  |      |                               |  |
             *  |  |      |         |  |      |  StructuredExceptionHandling  |  |
             *  |  +------+---------+  |      V                               |  |
             *  |         |            |    (null)                            |  |
             *  |         |            |                                      |  |
             *  |         |            +--------------------------------------+  |
             *  +---------+------------------------------------------------------+
             *            |
             *            v
             *         Exit 4
             */
            var entryBlock = cfg.EntryBlock;
            var initBlock = cfg.Blocks[1];
            var tryBlock = cfg.Blocks[2];
            var finallyBlock = cfg.Blocks[3];
            var exitBlock = cfg.ExitBlock;
            entryBlock.Kind.Should().Be(BasicBlockKind.Entry);
            initBlock.Kind.Should().Be(BasicBlockKind.Block);
            tryBlock.Kind.Should().Be(BasicBlockKind.Block);
            finallyBlock.Kind.Should().Be(BasicBlockKind.Block);
            exitBlock.Kind.Should().Be(BasicBlockKind.Exit);

            var tryAndFinallyRegion = cfg.Root.NestedRegions.Single(x => x.Kind == ControlFlowRegionKind.TryAndFinally);
            var tryRegion = tryAndFinallyRegion.NestedRegions.Single(x => x.Kind == ControlFlowRegionKind.Try);
            var finallyRegion = tryAndFinallyRegion.NestedRegions.Single(x => x.Kind == ControlFlowRegionKind.Finally);

            var entering = initBlock.FallThroughSuccessor;
            entering.Destination.Should().Be(tryBlock);
            entering.EnteringRegions.Should().HaveCount(2).And.ContainInOrder(tryAndFinallyRegion, tryRegion);
            entering.LeavingRegions.Should().BeEmpty();
            entering.FinallyRegions.Should().BeEmpty();

            var exiting = tryBlock.FallThroughSuccessor;
            exiting.Destination.Should().Be(exitBlock);
            exiting.EnteringRegions.Should().BeEmpty();
            exiting.LeavingRegions.Should().HaveCount(2).And.ContainInOrder(tryRegion, tryAndFinallyRegion);
            exiting.FinallyRegions.Should().HaveCount(1).And.Contain(finallyRegion);

            var insideFinally = finallyBlock.FallThroughSuccessor;
            insideFinally.Destination.Should().BeNull();
            insideFinally.Semantics.Should().Be(ControlFlowBranchSemantics.StructuredExceptionHandling);
            entering.EnteringRegions.Should().HaveCount(2).And.ContainInOrder(tryAndFinallyRegion, tryRegion); // Weird, but Roslyn does it this way.
            entering.LeavingRegions.Should().BeEmpty();
            entering.FinallyRegions.Should().BeEmpty();
        }

        [TestMethod]
        public void ValidateOperations()
        {
            const string code = @"
public class Sample
{
    int Pow(int num, int exponent)
    {
        num = num * Pow(num, exponent - 1);
        return 42;
    }
}";
            var cfg = TestHelper.CompileCfg(code);
            var entry = cfg.EntryBlock;
            var exit = cfg.ExitBlock;
            var body = cfg.Blocks[1];
            entry.Kind.Should().Be(BasicBlockKind.Entry);
            entry.Operations.Should().BeEmpty();
            exit.Kind.Should().Be(BasicBlockKind.Exit);
            exit.Operations.Should().BeEmpty();
            body.Kind.Should().Be(BasicBlockKind.Block);
            body.Operations.Length.Should().Be(1);
            body.Operations[0].Syntax.Kind().Should().Be(SyntaxKind.ExpressionStatement);
            body.BranchValue.Syntax.Kind().Should().Be(SyntaxKind.NumericLiteralExpression);
            body.OperationsAndBranchValue.Should().OnlyContainInOrder(body.Operations[0], body.BranchValue);
        }

        [TestMethod]
        public void ValidateAnonymousFunctionFinder()
        {
            const string code = @"
using System;
public class Sample {
    private Action<int> Simple()
    {
        var x = 42;
        return (x) => {  };
    }
}";
            var cfg = TestHelper.CompileCfg(code);
            var anonymousFunctionOperations = cfg.FlowAnonymousFunctionOperations().ToList();
            anonymousFunctionOperations.Count.Should().Be(1);
            var anonymousCfg = cfg.GetAnonymousFunctionControlFlowGraph(anonymousFunctionOperations[0]);
            anonymousCfg.Should().NotBeNull();
        }
    }
}
