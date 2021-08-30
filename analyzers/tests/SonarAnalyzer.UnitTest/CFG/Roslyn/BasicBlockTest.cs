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

            entry.Ordinal.Should().Be(0);
            branch.Ordinal.Should().Be(1);
            assign.Ordinal.Should().Be(2);
            exit.Ordinal.Should().Be(3);

            entry.Predecessors.Should().BeEmpty();
            branch.Predecessors.Should().HaveCount(1);
            assign.Predecessors.Should().HaveCount(1);
            exit.Predecessors.Should().HaveCount(2);
        }
    }
}
