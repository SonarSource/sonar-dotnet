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

using System;
using System.Linq;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SonarAnalyzer.CFG.Roslyn;
using SonarAnalyzer.Extensions;
using SonarAnalyzer.SymbolicExecution.Roslyn;
using SonarAnalyzer.UnitTest.Helpers;
using StyleCop.Analyzers.Lightup;

namespace SonarAnalyzer.UnitTest.SymbolicExecution.Roslyn
{
    [TestClass]
    public class ExplodedNodeTest
    {
        [TestMethod]
        public void Constructor_NullState_Throws()
        {
            var cfg = TestHelper.CompileCfgBodyCS();
            var validNode = new ExplodedNode(cfg.EntryBlock, ProgramState.Empty);

            ((Action)(() => new ExplodedNode(validNode, null))).Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("state");
            ((Action)(() => new ExplodedNode(cfg.EntryBlock, null))).Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("state");
        }

        [TestMethod]
        public void FromBasicBlock_Empty_HasNullOperations()
        {
            var cfg = TestHelper.CompileCfgBodyCS();
            var sut = new ExplodedNode(cfg.EntryBlock, ProgramState.Empty);
            sut.Operation.Should().BeNull();
        }

        [TestMethod]
        public void IteratesExecutionOrder_CS()
        {
            var block = TestHelper.CompileCfgBodyCS("var value = 42;").Blocks[1];
            // Visualize operations to understand the rest of this UT
            block.OperationsAndBranchValue.ToExecutionOrder().Select(TestHelper.Serialize).Should().OnlyContainInOrder(
                 "LocalReference: value = 42 (Implicit)",
                 "Literal: 42",
                 "SimpleAssignment: value = 42 (Implicit)");

            var sut = new ExplodedNode(block, ProgramState.Empty);
            TestHelper.Serialize(sut.Operation).Should().Be("LocalReference: value = 42 (Implicit)");

            sut = new ExplodedNode(sut, ProgramState.Empty);
            TestHelper.Serialize(sut.Operation).Should().Be("Literal: 42");

            sut = new ExplodedNode(sut, ProgramState.Empty);
            TestHelper.Serialize(sut.Operation).Should().Be("SimpleAssignment: value = 42 (Implicit)");

            sut = new ExplodedNode(sut, ProgramState.Empty);
            sut.Operation.Should().BeNull();
        }

        [TestMethod]
        public void IteratesExecutionOrder_VB()
        {
            var block = TestHelper.CompileCfgBodyVB("Dim Value As Integer = 42").Blocks[1];
            // Visualize operations to understand the rest of this UT
            block.OperationsAndBranchValue.ToExecutionOrder().Select(TestHelper.Serialize).Should().OnlyContainInOrder(
                "LocalReference: Value (Implicit)",
                "Literal: 42",
                "SimpleAssignment: Value As Integer = 42 (Implicit)");

            var sut = new ExplodedNode(block, ProgramState.Empty);
            TestHelper.Serialize(sut.Operation).Should().Be("LocalReference: Value (Implicit)");

            sut = new ExplodedNode(sut, ProgramState.Empty);
            TestHelper.Serialize(sut.Operation).Should().Be("Literal: 42");

            sut = new ExplodedNode(sut, ProgramState.Empty);
            TestHelper.Serialize(sut.Operation).Should().Be("SimpleAssignment: Value As Integer = 42 (Implicit)");

            sut = new ExplodedNode(sut, ProgramState.Empty);
            sut.Operation.Should().BeNull();
        }
    }
}
