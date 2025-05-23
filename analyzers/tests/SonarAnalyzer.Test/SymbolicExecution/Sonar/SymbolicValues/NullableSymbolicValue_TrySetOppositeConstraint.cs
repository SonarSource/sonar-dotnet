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

using SonarAnalyzer.SymbolicExecution;
using SonarAnalyzer.SymbolicExecution.Constraints;
using SonarAnalyzer.SymbolicExecution.Sonar;
using SonarAnalyzer.SymbolicExecution.Sonar.Constraints;
using SonarAnalyzer.SymbolicExecution.Sonar.SymbolicValues;

namespace SonarAnalyzer.Test.SymbolicExecution.Sonar.SymbolicValues
{
    [TestClass]
    public class NullableSymbolicValue_TrySetOppositeConstraint
    {
        private SymbolicValue symbolicValue;
        private NullableSymbolicValue nullableSymbolicValue;

        [TestInitialize]
        public void TestInitialize()
        {
            symbolicValue = SymbolicValue.Create();
            nullableSymbolicValue = new NullableSymbolicValue(symbolicValue);
        }

        [TestMethod]
        public void TrySetOppositeConstraint_Existing_No_Set_True()
        {
            var ps = new ProgramState();

            var newProgramStates = nullableSymbolicValue.TrySetOppositeConstraint(BoolConstraint.True, ps).ToList();

            newProgramStates.Should().HaveCount(2);
            ShouldHaveConstraint(newProgramStates[0], nullableSymbolicValue, NullableConstraint.HasValue);
            ShouldHaveConstraint(newProgramStates[0], symbolicValue, BoolConstraint.False);
            ShouldHaveConstraint(newProgramStates[1], nullableSymbolicValue, NullableConstraint.NoValue);
        }

        [TestMethod]
        public void TrySetOppositeConstraint_Existing_No_Set_False()
        {
            var ps = new ProgramState();

            var newProgramStates = nullableSymbolicValue.TrySetOppositeConstraint(BoolConstraint.False, ps).ToList();

            newProgramStates.Should().HaveCount(2);
            ShouldHaveConstraint(newProgramStates[0], nullableSymbolicValue, NullableConstraint.HasValue);
            ShouldHaveConstraint(newProgramStates[0], symbolicValue, BoolConstraint.True);
            ShouldHaveConstraint(newProgramStates[1], nullableSymbolicValue, NullableConstraint.NoValue);
        }

        [TestMethod]
        public void TrySetOppositeConstraint_Existing_No_Set_None()
        {
            var ps = new ProgramState();

            var newProgramStates = nullableSymbolicValue.TrySetOppositeConstraint(NullableConstraint.NoValue, ps).ToList();

            newProgramStates.Should().ContainSingle();
            ShouldHaveConstraint(newProgramStates[0], nullableSymbolicValue, NullableConstraint.HasValue);
        }

        [TestMethod]
        public void TrySetOppositeConstraint_Existing_No_Set_Some()
        {
            var ps = new ProgramState();

            var newProgramStates = nullableSymbolicValue.TrySetOppositeConstraint(NullableConstraint.HasValue, ps).ToList();

            newProgramStates.Should().ContainSingle();
            ShouldHaveConstraint(newProgramStates[0], nullableSymbolicValue, NullableConstraint.NoValue);
        }

        [TestMethod]
        public void TrySetOppositeConstraint_Existing_No_Set_Null()
        {
            var ps = new ProgramState();

            var newProgramStates = nullableSymbolicValue.TrySetOppositeConstraint(ObjectConstraint.Null, ps).ToList();

            newProgramStates.Should().ContainSingle();
            ShouldHaveConstraint(newProgramStates[0], nullableSymbolicValue, NullableConstraint.HasValue);
        }

        [TestMethod]
        public void TrySetOppositeConstraint_Existing_No_Set_NotNull()
        {
            var ps = new ProgramState();

            var newProgramStates = nullableSymbolicValue.TrySetOppositeConstraint(ObjectConstraint.NotNull, ps).ToList();

            newProgramStates.Should().Equal(ps);
        }

        private static void ShouldHaveConstraint(ProgramState ps, SymbolicValue sv, SymbolicConstraint expectedConstraint)
        {
            sv.TryGetConstraints(ps, out var constraints).Should().BeTrue();
            constraints.HasConstraint(expectedConstraint).Should().BeTrue();
        }
    }
}
