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

using SonarAnalyzer.CFG.Roslyn;
using SonarAnalyzer.SymbolicExecution.Constraints;
using SonarAnalyzer.SymbolicExecution.Roslyn;
using SonarAnalyzer.Test.TestFramework.SymbolicExecution;
using StyleCop.Analyzers.Lightup;

namespace SonarAnalyzer.Test.SymbolicExecution.Roslyn;

[TestClass]
public class SymbolicContextTest
{
    [TestMethod]
    public void NullArgument_State_Throws()
    {
        var block = CreateBlock();
        var create = () => new SymbolicContext(block, new IOperationWrapperSonar(block.Operations[0]), null, false, Array.Empty<ISymbol>());
        create.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("state");
    }

    [TestMethod]
    public void NullArgument_CapturedVariables_Throws()
    {
        var block = CreateBlock();
        var create = () => new SymbolicContext(block, new IOperationWrapperSonar(block.Operations[0]), ProgramState.Empty, false, null);
        create.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("capturedVariables");
    }

    [TestMethod]
    public void NullOperation_SetsOperationToNull() =>
        new SymbolicContext(null, default, ProgramState.Empty, false, Array.Empty<ISymbol>()).Operation.Instance.Should().BeNull();

    [TestMethod]
    public void PropertiesArePersisted()
    {
        var block = CreateBlock();
        var operation = new IOperationWrapperSonar(block.Operations[0]);
        var state = ProgramState.Empty.SetOperationValue(operation, SymbolicValue.Empty);
        var sut = new SymbolicContext(block, operation, state, true, Array.Empty<ISymbol>());
        sut.Block.Should().Be(block);
        sut.Operation.Should().Be(operation);
        sut.State.Should().Be(state);
        sut.IsInLoop.Should().BeTrue();
    }

    [TestMethod]
    public void SetOperationConstraint_WithExistingValue()
    {
        var block = CreateBlock();
        var operation = new IOperationWrapperSonar(block.Operations[0]);
        var state = ProgramState.Empty.SetOperationValue(operation, SymbolicValue.Empty);
        var sut = new SymbolicContext(block, operation, state, false, Array.Empty<ISymbol>());
        var result = sut.SetOperationConstraint(DummyConstraint.Dummy);
        result.Should().NotBe(state, "new ProgramState instance should be created");
        result[operation].HasConstraint(DummyConstraint.Dummy).Should().BeTrue();
    }

    [TestMethod]
    public void SetOperationConstraint_WithNewValue()
    {
        var block = CreateBlock();
        var operation = new IOperationWrapperSonar(block.Operations[0]);
        var state = ProgramState.Empty;
        var sut = new SymbolicContext(block, operation, state, false, Array.Empty<ISymbol>());
        var result = sut.SetOperationConstraint(DummyConstraint.Dummy);
        result.Should().NotBe(state, "new ProgramState instance should be created");
        result[operation].HasConstraint(DummyConstraint.Dummy).Should().BeTrue();
    }

    [TestMethod]
    public void SetSymbolConstraint_WithExistingValue()
    {
        var block = CreateBlock();
        var operation = new IOperationWrapperSonar(block.Operations[0]);
        var symbol = operation.Children.First().TrackedSymbol(ProgramState.Empty);
        var state = ProgramState.Empty.SetSymbolValue(symbol, SymbolicValue.Empty);
        var sut = new SymbolicContext(block, operation, state, false, Array.Empty<ISymbol>());
        var result = sut.SetSymbolConstraint(symbol, DummyConstraint.Dummy);
        result.Should().NotBe(state, "new ProgramState instance should be created");
        result[symbol].HasConstraint(DummyConstraint.Dummy).Should().BeTrue();
    }

    [TestMethod]
    public void SetSymbolConstraint_WithNewValue()
    {
        var block = CreateBlock();
        var operation = new IOperationWrapperSonar(block.Operations[0]);
        var symbol = operation.Children.First().TrackedSymbol(ProgramState.Empty);
        var state = ProgramState.Empty;
        var sut = new SymbolicContext(block, operation, state, false, Array.Empty<ISymbol>());
        var result = sut.SetSymbolConstraint(symbol, DummyConstraint.Dummy);
        result.Should().NotBe(state, "new ProgramState instance should be created");
        result[symbol].HasConstraint(DummyConstraint.Dummy).Should().BeTrue();
    }

    [TestMethod]
    public void SetOperationValue_PropagatesValue()
    {
        var block = CreateBlock();
        var operation = new IOperationWrapperSonar(block.Operations[0]);
        var state = ProgramState.Empty;
        var sut = new SymbolicContext(block, operation, state, false, Array.Empty<ISymbol>());
        var result = sut.SetOperationValue(SymbolicValue.True);
        result.Should().NotBe(state, "new ProgramState instance should be created");
        result[operation].Should().HaveOnlyConstraints(ObjectConstraint.NotNull, BoolConstraint.True);
    }

    [TestMethod]
    public void WithState_SameState_ReturnsThis()
    {
        var state = ProgramState.Empty;
        var sut = new SymbolicContext(null, default, state, false, Array.Empty<ISymbol>());
        sut.WithState(state).Should().Be(sut);
    }

    [TestMethod]
    public void WithState_DifferentState_ReturnsNew()
    {
        var state = ProgramState.Empty;
        var sut = new SymbolicContext(null, default, state, false, Array.Empty<ISymbol>());
        var newState = state.SetOperationValue(CreateOperation(), SymbolicValue.Empty);
        sut.WithState(newState).Should().NotBe(sut);
    }

    [TestMethod]
    public void ApplyOpposite_RespectsArgument()
    {
        BoolConstraint.True.ApplyOpposite(false).Should().Be(BoolConstraint.True);
        BoolConstraint.True.ApplyOpposite(true).Should().Be(BoolConstraint.False);
        BoolConstraint.False.ApplyOpposite(false).Should().Be(BoolConstraint.False);
        BoolConstraint.False.ApplyOpposite(true).Should().Be(BoolConstraint.True);
        // Special constraint behavior
        ObjectConstraint.Null.ApplyOpposite(true).Should().Be(ObjectConstraint.NotNull);
        ObjectConstraint.NotNull.ApplyOpposite(true).Should().BeNull("NotNull can be Null or any other NotNull");
    }

    private static IOperationWrapperSonar CreateOperation() =>
        new(CreateBlock().Operations[0]);

    private static BasicBlock CreateBlock() =>
        TestHelper.CompileCfgBodyCS("var value = 42;").Blocks[1];
}
