/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2022 SonarSource SA
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
using SonarAnalyzer.SymbolicExecution.Roslyn;
using SonarAnalyzer.UnitTest.TestFramework.SymbolicExecution;

namespace SonarAnalyzer.UnitTest.SymbolicExecution.Roslyn
{
    public partial class ProgramStateTest
    {
        [TestMethod]
        public void SetSymbolValue_ReturnsValues()
        {
            var counter = new SymbolicValueCounter();
            var value1 = new SymbolicValue(counter);
            var value2 = new SymbolicValue(counter);
            var symbols = CreateSymbols();
            var sut = ProgramState.Empty;

            sut[symbols[0]].Should().BeNull();
            sut[symbols[1]].Should().BeNull();
            sut[symbols[2]].Should().BeNull();

            sut = sut.SetSymbolValue(symbols[0], value1);
            sut = sut.SetSymbolValue(symbols[1], value2);

            sut[symbols[0]].Should().Be(value1);
            sut[symbols[1]].Should().Be(value2);
            sut[symbols[2]].Should().BeNull();     // Value was not set
        }

        [TestMethod]
        public void SetSymbolValue_IsImmutable()
        {
            var symbol = CreateSymbols().First();
            var sut = ProgramState.Empty;

            sut[symbol].Should().BeNull();
            sut.SetSymbolValue(symbol, new SymbolicValue(new SymbolicValueCounter()));
            sut[symbol].Should().BeNull(nameof(sut.SetSymbolValue) + " returned new ProgramState instance.");
        }

        [TestMethod]
        public void SetSymbolValue_Overwrites()
        {
            var counter = new SymbolicValueCounter();
            var value1 = new SymbolicValue(counter);
            var value2 = new SymbolicValue(counter);
            var symbol = CreateSymbols().First();
            var sut = ProgramState.Empty;

            sut[symbol].Should().BeNull();
            sut = sut.SetSymbolValue(symbol, value1);
            sut = sut.SetSymbolValue(symbol, value2);
            sut[symbol].Should().Be(value2);
        }

        [TestMethod]
        public void SetSymbolValue_NullSymbol_Throws() =>
            ProgramState.Empty.Invoking(x => x.SetSymbolValue(null, new SymbolicValue(new SymbolicValueCounter()))).Should().Throw<ArgumentNullException>();

        [TestMethod]
        public void SymbolsWith_ReturnCorrectSymbols()
        {
            var counter = new SymbolicValueCounter();
            var value0 = new SymbolicValue(counter).WithConstraint(DummyConstraint.Dummy);
            var value1 = new SymbolicValue(counter).WithConstraint(TestConstraint.First);
            var value2 = new SymbolicValue(counter).WithConstraint(TestConstraint.First);
            var symbols = CreateSymbols();
            var sut = ProgramState.Empty
                .SetSymbolValue(symbols[0], value0)
                .SetSymbolValue(symbols[1], value1)
                .SetSymbolValue(symbols[2], value2);

            sut.SymbolsWith(DummyConstraint.Dummy).Should().ContainSingle().Which.Should().Be(symbols[0]);
            sut.SymbolsWith(TestConstraint.First).Should().HaveCount(2);
            sut.SymbolsWith(TestConstraint.Second).Should().BeEmpty();
        }

        [TestMethod]
        public void SymbolsWith_IgnoreNullValue()
        {
            var symbol = CreateSymbols().First();
            var sut = ProgramState.Empty.SetSymbolValue(symbol, null);
            sut.SymbolsWith(DummyConstraint.Dummy).Should().BeEmpty();
        }
    }
}
