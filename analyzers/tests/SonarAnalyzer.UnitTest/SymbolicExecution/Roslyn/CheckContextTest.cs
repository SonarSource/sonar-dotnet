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
using StyleCop.Analyzers.Lightup;

namespace SonarAnalyzer.UnitTest.SymbolicExecution.Roslyn
{
    [TestClass]
    public class CheckContextTest
    {
        [TestMethod]
        public void NullArgument_Throws()
        {
            var counter = new SymbolicValueCounter();
            var operation = CreateOperation();

            ((Func<CheckContext>)(() => new CheckContext(null, operation, ProgramState.Empty))).Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("symbolicValueCounter");
            ((Func<CheckContext>)(() => new CheckContext(counter, null, ProgramState.Empty))).Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("operation");
            ((Func<CheckContext>)(() => new CheckContext(counter, operation, null))).Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("state");
        }

        [TestMethod]
        public void PropertiesArePersisted()
        {
            var counter = new SymbolicValueCounter();
            var operation = CreateOperation();
            var state = ProgramState.Empty.SetOperationValue(operation, new SymbolicValue(counter));

            var sut = new CheckContext(counter, operation, state);
            sut.Operation.Should().Be(operation);
            sut.State.Should().Be(state);
        }

        [TestMethod]
        public void CreateSymbolicValue_UsesSymbolicValueCounter()
        {
            var counter = new SymbolicValueCounter();
            counter.NextIdentifier().Should().Be(1);    // Skip first values
            counter.NextIdentifier().Should().Be(2);
            var sut = new CheckContext(counter, CreateOperation(), ProgramState.Empty);
            sut.CreateSymbolicValue().ToString().Should().Be("SV_3");
            counter.NextIdentifier().Should().Be(4);
        }

        private static IOperationWrapperSonar CreateOperation() =>
            new(TestHelper.CompileCfgBodyCS("var value = 42;").Blocks[1].Operations[0]);
    }
}
