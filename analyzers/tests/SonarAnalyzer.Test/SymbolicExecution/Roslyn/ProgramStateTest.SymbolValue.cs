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

using SonarAnalyzer.SymbolicExecution;
using SonarAnalyzer.SymbolicExecution.Roslyn;
using SonarAnalyzer.Test.TestFramework.SymbolicExecution;

namespace SonarAnalyzer.Test.SymbolicExecution.Roslyn;

public partial class ProgramStateTest
{
    [TestMethod]
    public void SetSymbolValue_ReturnsValues()
    {
        var value1 = SymbolicValue.Empty;
        var value2 = SymbolicValue.Empty;
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
        sut.SetSymbolValue(symbol, SymbolicValue.Empty);
        sut[symbol].Should().BeNull(nameof(sut.SetSymbolValue) + " returned new ProgramState instance.");
    }

    [TestMethod]
    public void SetSymbolValue_Overwrites()
    {
        var value1 = SymbolicValue.Empty;
        var value2 = SymbolicValue.Empty;
        var symbol = CreateSymbols().First();
        var sut = ProgramState.Empty;

        sut[symbol].Should().BeNull();
        sut = sut.SetSymbolValue(symbol, value1);
        sut = sut.SetSymbolValue(symbol, value2);
        sut[symbol].Should().Be(value2);
    }

    [TestMethod]
    public void SetSymbolValue_NullValue_RemovesSymbol()
    {
        var value = SymbolicValue.Empty;
        var symbol = CreateSymbols().First();
        var sut = ProgramState.Empty;

        sut = sut.SetSymbolValue(symbol, value);
        sut[symbol].Should().NotBeNull().And.HaveNoConstraints();
        sut = sut.SetSymbolValue(symbol, null);
        sut[symbol].Should().BeNull().And.HaveNoConstraints();
    }

    [TestMethod]
    public void SetSymbolValue_NullSymbol_Throws() =>
        ProgramState.Empty.Invoking(x => x.SetSymbolValue(null, SymbolicValue.Empty)).Should().Throw<ArgumentNullException>();

    [TestMethod]
    public void SymbolsWith_ReturnCorrectSymbols()
    {
        var value0 = SymbolicValue.Empty.WithConstraint(DummyConstraint.Dummy);
        var value1 = SymbolicValue.Empty.WithConstraint(TestConstraint.First);
        var value2 = SymbolicValue.Empty.WithConstraint(TestConstraint.First);
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

    [TestMethod]
    public void Preserve_PreservedSymbolCannotBeRemoved()
    {
        var symbolicValue = SymbolicValue.Empty.WithConstraint(DummyConstraint.Dummy);
        var symbol = CreateSymbols().First();
        var sut = ProgramState.Empty.SetSymbolValue(symbol, symbolicValue)
            .Preserve(symbol)
            .RemoveSymbols(x => true);
        sut.SymbolsWith(DummyConstraint.Dummy).Should().Contain(symbol);
    }

    [TestMethod]
    public void RemoveSymbols_RemovesSymbolsMatchingThePredicate()
    {
        var symbolicValue = SymbolicValue.Empty.WithConstraint(DummyConstraint.Dummy);
        var symbols = CreateSymbols().ToArray();
        var sut = ProgramState.Empty.SetSymbolValue(symbols[0], symbolicValue)
            .SetSymbolValue(symbols[1], symbolicValue)
            .SetSymbolValue(symbols[2], symbolicValue)
            .RemoveSymbols(x => !SymbolEqualityComparer.Default.Equals(x, symbols[1]));
        sut.SymbolsWith(DummyConstraint.Dummy).Should().Contain(symbols[1]).And.HaveCount(1);
    }

    [TestMethod]
    public void SetSymbolConstraint_NoValue_CreatesNewValue()
    {
        var symbol = CreateSymbols().First();
        var sut = ProgramState.Empty.SetSymbolConstraint(symbol, DummyConstraint.Dummy);
        sut[symbol].Should().HaveOnlyConstraint(DummyConstraint.Dummy);
    }

    [TestMethod]
    public void SetSymbolConstraint_ExistingValue_PreservesOtherConstraints()
    {
        var symbol = CreateSymbols().First();
        var sut = ProgramState.Empty
            .SetSymbolValue(symbol, SymbolicValue.Empty.WithConstraint(TestConstraint.First))
            .SetSymbolConstraint(symbol, DummyConstraint.Dummy);
        sut[symbol].Should().HaveOnlyConstraints(new SymbolicConstraint[] { TestConstraint.First, DummyConstraint.Dummy }, "original constraints of different type should be preserved");
    }
}
