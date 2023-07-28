/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2023 SonarSource SA
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
using SonarAnalyzer.SymbolicExecution.Constraints;
using SonarAnalyzer.SymbolicExecution.Roslyn;

namespace SonarAnalyzer.UnitTest.SymbolicExecution.Roslyn;

public partial class ProgramStateTest
{
    private static IEnumerable<object[]> HasConstraintTestData => new[]
    {
        new object[] { null, ObjectConstraint.Null, false },
        new object[] { SymbolicValue.Empty, ObjectConstraint.Null, false },
        new object[] { SymbolicValue.Null, ObjectConstraint.Null, true },
        new object[] { SymbolicValue.Null, ObjectConstraint.NotNull, false },
        new object[] { SymbolicValue.Null, BoolConstraint.True, false }
    };

    [TestMethod]
    [DynamicData(nameof(HasConstraintTestData))]
    public void HasConstraint_Operation(SymbolicValue symbolicValue, SymbolicConstraint constraint, bool hasConstraint)
    {
        var cfg = TestHelper.CompileCfgBodyCS("var s = \"\";");
        var operation = cfg.Blocks[1].Operations[0];
        var state = ProgramState.Empty;
        if (symbolicValue is not null)
        {
            state = state.SetOperationValue(operation, symbolicValue);
        }
        state.HasConstraint(operation, constraint).Should().Be(hasConstraint);
    }

    [TestMethod]
    public void HasConstraint_Operation_IsNullSafe() =>
        ProgramState.Empty.HasConstraint((IOperation)null, ObjectConstraint.Null).Should().BeFalse();

    [TestMethod]
    [DynamicData(nameof(HasConstraintTestData))]
    public void HasConstraint_Symbol(SymbolicValue symbolicValue, SymbolicConstraint constraint, bool hasConstraint)
    {
        var cfg = TestHelper.CompileCfgBodyCS("var s = \"\";");
        var symbol = cfg.Blocks[1].Operations[0].ChildOperations.First().TrackedSymbol();
        var state = ProgramState.Empty;
        if (symbolicValue is not null)
        {
            state = state.SetSymbolValue(symbol, symbolicValue);
        }
        state.HasConstraint(symbol, constraint).Should().Be(hasConstraint);
    }

    [TestMethod]
    public void HasConstraint_Symbol_IsNullSafe() =>
        ProgramState.Empty.HasConstraint((ISymbol)null, ObjectConstraint.Null).Should().BeFalse();

    private static IEnumerable<object[]> ConstraintTestData => new[]
    {
        new object[] { null, null },
        new object[] { SymbolicValue.Empty, null },
        new object[] { SymbolicValue.Null, null },
        new object[] { SymbolicValue.True, BoolConstraint.True },
        new object[] { SymbolicValue.False, BoolConstraint.False }
    };

    [TestMethod]
    [DynamicData(nameof(ConstraintTestData))]
    public void Constraint_Operation(SymbolicValue symbolicValue, SymbolicConstraint constraint)
    {
        var cfg = TestHelper.CompileCfgBodyCS("var s = \"\";");
        var operation = cfg.Blocks[1].Operations[0];
        var state = ProgramState.Empty;
        if (symbolicValue is not null)
        {
            state = state.SetOperationValue(operation, symbolicValue);
        }
        state.Constraint<BoolConstraint>(operation).Should().Be(constraint);
    }

    [TestMethod]
    public void Constraint_Operation_IsNullSafe() =>
        ProgramState.Empty.Constraint<ObjectConstraint>((IOperation)null).Should().BeNull();

    [TestMethod]
    [DynamicData(nameof(ConstraintTestData))]
    public void Constraint_Symbol(SymbolicValue symbolicValue, SymbolicConstraint constraint)
    {
        var cfg = TestHelper.CompileCfgBodyCS("var s = \"\";");
        var symbol = cfg.Blocks[1].Operations[0].ChildOperations.First().TrackedSymbol();
        var state = ProgramState.Empty;
        if (symbolicValue is not null)
        {
            state = state.SetSymbolValue(symbol, symbolicValue);
        }
        state.Constraint<BoolConstraint>(symbol).Should().Be(constraint);
    }

    [TestMethod]
    public void Constraint_Symbol_IsNullSafe() =>
        ProgramState.Empty.Constraint<ObjectConstraint>((ISymbol)null).Should().BeNull();
}
