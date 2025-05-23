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

using SonarAnalyzer.CFG.Roslyn;

namespace SonarAnalyzer.Test.CFG.Roslyn
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
            var cfg = TestHelper.CompileCfgCS(code);
            var entry = cfg.EntryBlock;
            var exit = cfg.ExitBlock;

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
    int field;

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
            var cfg = TestHelper.CompileCfgCS(code);
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
            var initBlock = cfg.Blocks[1];
            var tryBlock = cfg.Blocks[2];
            var finallyBlock = cfg.Blocks[3];
            var exitBlock = cfg.ExitBlock;
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
    }
}
