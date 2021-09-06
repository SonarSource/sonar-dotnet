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
using SonarAnalyzer.UnitTest.Helpers;

namespace SonarAnalyzer.UnitTest.CFG.Roslyn
{
    [TestClass]
    public class BasicBlockTest
    {
        [TestMethod]
        public void Wrap_ReturnsNull() =>
            BasicBlock.Wrap(null).Should().BeNull();

        [TestMethod]
        public void ValidateReflection()
        {
            const string code = @"
public class Sample
{
    int field;

    public void Method(bool condition)
    {
        if (condition)
            field = 42;
    }
}";
            var cfg = TestHelper.CompileCfg(code);
            /*
             *           Entry 0
             *             |
             *             |
             *           Block 1
             *      BranchValue: condition
             *           /   \
             *     Else /     \ WhenFalse
             *         /       \
             *     Block 2      |
             *     field=42     |
             *        \        /
             *         \      /
             *          \    /
             *           Exit 3
             */
            var entry = cfg.Blocks[0];
            var branch = cfg.Blocks[1];
            var assign = cfg.Blocks[2];
            var exit = cfg.Blocks[3];

            entry.Kind.Should().Be(BasicBlockKind.Entry);
            branch.Kind.Should().Be(BasicBlockKind.Block);
            assign.Kind.Should().Be(BasicBlockKind.Block);
            exit.Kind.Should().Be(BasicBlockKind.Exit);

            entry.BranchValue.Should().BeNull();
            branch.BranchValue.Should().BeAssignableTo<IParameterReferenceOperation>().Subject.Parameter.Name.Should().Be("condition");
            assign.BranchValue.Should().BeNull();
            exit.BranchValue.Should().BeNull();

            entry.ConditionalSuccessor.Should().BeNull();
            branch.ConditionalSuccessor.Should().NotBeNull();
            assign.ConditionalSuccessor.Should().BeNull();
            exit.ConditionalSuccessor.Should().BeNull();

            entry.ConditionKind.Should().Be(ControlFlowConditionKind.None);
            branch.ConditionKind.Should().Be(ControlFlowConditionKind.WhenFalse);
            assign.ConditionKind.Should().Be(ControlFlowConditionKind.None);
            exit.ConditionKind.Should().Be(ControlFlowConditionKind.None);

            entry.EnclosingRegion.Should().Be(cfg.Root);
            branch.EnclosingRegion.Should().Be(cfg.Root);
            assign.EnclosingRegion.Should().Be(cfg.Root);
            exit.EnclosingRegion.Should().Be(cfg.Root);

            entry.FallThroughSuccessor.Should().NotBeNull();
            branch.FallThroughSuccessor.Should().NotBeNull();
            assign.FallThroughSuccessor.Should().NotBeNull();
            exit.FallThroughSuccessor.Should().BeNull();

            entry.IsReachable.Should().Be(true);
            branch.IsReachable.Should().Be(true);
            assign.IsReachable.Should().Be(true);
            exit.IsReachable.Should().Be(true);

            entry.Operations.Should().BeEmpty();
            branch.Operations.Should().BeEmpty();
            assign.Operations.Should().HaveCount(1).And.Subject.Single().Should().BeAssignableTo<IExpressionStatementOperation>();
            exit.Operations.Should().BeEmpty();

            entry.OperationsAndBranchValue.Should().BeEmpty();
            branch.OperationsAndBranchValue.Should().HaveCount(1).And.Subject.Single().Should().BeAssignableTo<IParameterReferenceOperation>();
            assign.OperationsAndBranchValue.Should().HaveCount(1).And.Subject.Single().Should().BeAssignableTo<IExpressionStatementOperation>();
            exit.OperationsAndBranchValue.Should().BeEmpty();

            entry.Ordinal.Should().Be(0);
            branch.Ordinal.Should().Be(1);
            assign.Ordinal.Should().Be(2);
            exit.Ordinal.Should().Be(3);

            entry.Predecessors.Should().BeEmpty();
            branch.Predecessors.Should().HaveCount(1);
            assign.Predecessors.Should().HaveCount(1);
            exit.Predecessors.Should().HaveCount(2);

            entry.SuccessorBlocks.Should().HaveCount(1).And.Subject.Single().Should().Be(branch);
            branch.SuccessorBlocks.Should().HaveCount(2).And.Subject.Should().ContainInOrder(assign, exit);
            assign.SuccessorBlocks.Should().HaveCount(1).And.Subject.Single().Should().Be(exit);
            exit.SuccessorBlocks.Should().HaveCount(0);
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
            var body = cfg.Blocks[1];
            var exit = cfg.ExitBlock;
            entry.Kind.Should().Be(BasicBlockKind.Entry);
            entry.Operations.Should().BeEmpty();
            exit.Kind.Should().Be(BasicBlockKind.Exit);
            exit.Operations.Should().BeEmpty();
            body.Kind.Should().Be(BasicBlockKind.Block);
            body.Operations.Should().HaveCount(1).And.Subject.Single().Should().BeAssignableTo<IExpressionStatementOperation>();
            body.BranchValue.Should().BeAssignableTo<ILiteralOperation>();
            body.OperationsAndBranchValue.Should().OnlyContainInOrder(body.Operations[0], body.BranchValue);
        }

        [TestMethod]
        public void ValidateSwitchExpressionCase()
        {
            const string code = @"
public class Sample
{
    public int Method(bool condition) =>
        condition switch
        {
            true => 42,
            _ => 43
        };
}";
            var cfg = TestHelper.CompileCfg(code);
            /*
             *
             *         Block 1
             *        true => 42
             * Else  /          \    WhenFalse
             *      /            \
             *     /              \
             *  Block 2          Block 3
             *   => 42           _ => 43
             *    |                |     \
             *    |          Else  |      \    WhenFalse from Block 3 - Block 5 should not be reachable
             *    |                |       \
             *    |              Block 4    Block 5
             *    |               => 43     no match => throw exception
             *     \              /
             *      \            /
             *       \          /
             *        \        /
             *         \      /
             *          Block 6
             */
            var block1 = cfg.Blocks[1];
            var block2 = cfg.Blocks[2];
            var block3 = cfg.Blocks[3];
            var block4 = cfg.Blocks[4];
            var block5 = cfg.Blocks[5];
            var block6 = cfg.Blocks[6];
            block1.FallThroughSuccessor.Destination.Should().Be(block2);
            block1.ConditionalSuccessor.Destination.Should().Be(block3);
            block1.SuccessorBlocks.Should().ContainInOrder(block2, block3);
            block2.FallThroughSuccessor.Destination.Should().Be(block6);
            block2.ConditionalSuccessor.Destination.Should().BeNull();
            block2.SuccessorBlocks.Single().Should().Be(block6);
            block3.FallThroughSuccessor.Destination.Should().Be(block4);
            block3.ConditionalSuccessor.Destination.Should().Be(block5);
            block3.SuccessorBlocks.Single().Should().Be(block4);             // We don't add the unreachable ConditionalSuccessor in this case
            block6.SuccessorBlocks.Single().Should().Be(cfg.ExitBlock);
        }
    }
}
