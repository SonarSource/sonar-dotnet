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
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SonarAnalyzer.SymbolicExecution.Roslyn;
using SonarAnalyzer.UnitTest.TestFramework.SymbolicExecution;
using StyleCop.Analyzers.Lightup;

namespace SonarAnalyzer.UnitTest.SymbolicExecution.Roslyn
{
    [TestClass]
    public class SymbolicContextTest
    {
        [TestMethod]
        public void NullArgument_Throws()
        {
            var counter = new SymbolicValueCounter();
            var operation = CreateOperation();

            ((Func<SymbolicContext>)(() => new SymbolicContext(null, operation, ProgramState.Empty))).Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("symbolicValueCounter");
            ((Func<SymbolicContext>)(() => new SymbolicContext(counter, operation, null))).Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("state");
        }

        [TestMethod]
        public void NullOperation_SetsOperationToNull() =>
            new SymbolicContext(new SymbolicValueCounter(), null, ProgramState.Empty).Operation.Should().Be(null);

        [TestMethod]
        public void PropertiesArePersisted()
        {
            var counter = new SymbolicValueCounter();
            var operation = CreateOperation();
            var state = ProgramState.Empty.SetOperationValue(operation, new SymbolicValue(counter));

            var sut = new SymbolicContext(counter, operation, state);
            sut.Operation.Should().Be(operation);
            sut.State.Should().Be(state);
        }

        [TestMethod]
        public void CreateSymbolicValue_UsesSymbolicValueCounter()
        {
            var counter = new SymbolicValueCounter();
            counter.NextIdentifier().Should().Be(1);    // Skip first values
            counter.NextIdentifier().Should().Be(2);
            var sut = new SymbolicContext(counter, CreateOperation(), ProgramState.Empty);
            sut.CreateSymbolicValue().ToString().Should().Be("SV_3");
            counter.NextIdentifier().Should().Be(4);
        }

        [TestMethod]
        public void SetOperationConstraint_WithExistingValue_ReusesProgramState()
        {
            var counter = new SymbolicValueCounter();
            var operation = CreateOperation();
            var state = ProgramState.Empty.SetOperationValue(operation, new SymbolicValue(counter));

            var sut = new SymbolicContext(counter, operation, state);
            var result = sut.SetOperationConstraint(DummyConstraint.Dummy);
            result.Should().Be(state);
            result[operation].HasConstraint(DummyConstraint.Dummy).Should().BeTrue();
        }

        [TestMethod]
        public void SetOperationConstraint_WithNewValue_CreatesNewProgramState()
        {
            var counter = new SymbolicValueCounter();
            var operation = CreateOperation();
            var state = ProgramState.Empty;

            var sut = new SymbolicContext(counter, operation, state);
            var result = sut.SetOperationConstraint(DummyConstraint.Dummy);
            result.Should().NotBe(state, "new ProgramState instance should be created");
            result[operation].HasConstraint(DummyConstraint.Dummy).Should().BeTrue();
        }

        private static IOperationWrapperSonar CreateOperation() =>
            new(TestHelper.CompileCfgBodyCS("var value = 42;").Blocks[1].Operations[0]);
    }
}
