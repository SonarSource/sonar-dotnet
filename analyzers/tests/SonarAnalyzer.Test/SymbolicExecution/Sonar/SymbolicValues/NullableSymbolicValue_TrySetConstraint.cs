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
using SonarAnalyzer.SymbolicExecution.Constraints;
using SonarAnalyzer.SymbolicExecution.Sonar;
using SonarAnalyzer.SymbolicExecution.Sonar.Constraints;
using SonarAnalyzer.SymbolicExecution.Sonar.SymbolicValues;

namespace SonarAnalyzer.Test.SymbolicExecution.Sonar.SymbolicValues
{
    [TestClass]
    public class NullableSymbolicValue_TrySetConstraint
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
        public void TrySetConstraint_Existing_No_Set_True()
        {
            var ps = new ProgramState();

            var newProgramStates = nullableSymbolicValue.TrySetConstraint(BoolConstraint.True, ps).ToList();

            newProgramStates.Should().ContainSingle();
            ShouldHaveConstraint(newProgramStates[0], nullableSymbolicValue, NullableConstraint.HasValue);
            ShouldHaveConstraint(newProgramStates[0], symbolicValue, BoolConstraint.True);
        }

        [TestMethod]
        public void TrySetConstraint_Existing_No_Set_False()
        {
            var ps = new ProgramState();

            var newProgramStates = nullableSymbolicValue.TrySetConstraint(BoolConstraint.False, ps).ToList();

            newProgramStates.Should().ContainSingle();
            ShouldHaveConstraint(newProgramStates[0], nullableSymbolicValue, NullableConstraint.HasValue);
            ShouldHaveConstraint(newProgramStates[0], symbolicValue, BoolConstraint.False);
        }

        [TestMethod]
        public void TrySetConstraint_Existing_No_Set_NoValue()
        {
            var ps = new ProgramState();

            var newProgramStates = nullableSymbolicValue.TrySetConstraint(NullableConstraint.NoValue, ps).ToList();

            newProgramStates.Should().ContainSingle();
            ShouldHaveConstraint(newProgramStates[0], nullableSymbolicValue, NullableConstraint.NoValue);
        }

        [TestMethod]
        public void TrySetConstraint_Existing_No_Set_HasValue()
        {
            var ps = new ProgramState();

            var newProgramStates = nullableSymbolicValue.TrySetConstraint(NullableConstraint.HasValue, ps).ToList();

            newProgramStates.Should().ContainSingle();
            ShouldHaveConstraint(newProgramStates[0], nullableSymbolicValue, NullableConstraint.HasValue);
        }

        [TestMethod]
        public void TrySetConstraint_Existing_No_Set_Null()
        {
            var ps = new ProgramState();

            var newProgramStates = nullableSymbolicValue.TrySetConstraint(ObjectConstraint.Null, ps).ToList();

            newProgramStates.Should().ContainSingle();
            ShouldHaveConstraint(newProgramStates[0], nullableSymbolicValue, NullableConstraint.NoValue);
        }

        [TestMethod]
        public void TrySetConstraint_Existing_No_Set_NotNull()
        {
            var ps = new ProgramState();

            var newProgramStates = nullableSymbolicValue.TrySetConstraint(ObjectConstraint.NotNull, ps).ToList();

            newProgramStates.Should().ContainSingle();
            ShouldHaveConstraint(newProgramStates[0], nullableSymbolicValue, NullableConstraint.HasValue);
        }

        [TestMethod]
        public void TrySetConstraint_Existing_No_Set_Nothing()
        {
            var ps = new ProgramState();

            var newProgramStates = nullableSymbolicValue.TrySetConstraint(null, ps).ToList();

            newProgramStates.Should().Equal(ps);
        }

        [TestMethod]
        public void TrySetConstraint_Existing_True_Set_True()
        {
            var ps = nullableSymbolicValue.TrySetConstraint(BoolConstraint.True, new ProgramState()).Single();

            var newProgramStates = nullableSymbolicValue.TrySetConstraint(BoolConstraint.True, ps).ToList();

            newProgramStates.Should().Equal(ps);
        }

        [TestMethod]
        public void TrySetConstraint_Existing_True_Set_False()
        {
            var ps = nullableSymbolicValue.TrySetConstraint(BoolConstraint.True, new ProgramState()).Single();

            var newProgramStates = nullableSymbolicValue.TrySetConstraint(BoolConstraint.False, ps).ToList();

            newProgramStates.Should().BeEmpty();
        }

        [TestMethod]
        public void TrySetConstraint_Existing_True_Set_NoValue()
        {
            var ps = nullableSymbolicValue.TrySetConstraint(BoolConstraint.True, new ProgramState()).Single();

            var newProgramStates = nullableSymbolicValue.TrySetConstraint(NullableConstraint.NoValue, ps).ToList();

            newProgramStates.Should().BeEmpty();
        }

        [TestMethod]
        public void TrySetConstraint_Existing_True_Set_HasValue()
        {
            var ps = nullableSymbolicValue.TrySetConstraint(BoolConstraint.True, new ProgramState()).Single();

            var newProgramStates = nullableSymbolicValue.TrySetConstraint(NullableConstraint.HasValue, ps).ToList();

            newProgramStates.Should().Equal(ps);
        }

        [TestMethod]
        public void TrySetConstraint_Existing_True_Set_Null()
        {
            var ps = nullableSymbolicValue.TrySetConstraint(BoolConstraint.True, new ProgramState()).Single();

            var newProgramStates = nullableSymbolicValue.TrySetConstraint(ObjectConstraint.Null, ps).ToList();

            newProgramStates.Should().BeEmpty();
        }

        [TestMethod]
        public void TrySetConstraint_Existing_True_Set_NotNull()
        {
            var ps = nullableSymbolicValue.TrySetConstraint(BoolConstraint.True, new ProgramState()).Single();

            var newProgramStates = nullableSymbolicValue.TrySetConstraint(ObjectConstraint.NotNull, ps).ToList();

            newProgramStates.Should().Equal(ps);
        }

        [TestMethod]
        public void TrySetConstraint_Existing_True_Set_Nothing()
        {
            var ps = nullableSymbolicValue.TrySetConstraint(BoolConstraint.True, new ProgramState()).Single();

            var newProgramStates = nullableSymbolicValue.TrySetConstraint(null, ps).ToList();

            newProgramStates.Should().Equal(ps);
        }

        [TestMethod]
        public void TrySetConstraint_Existing_False_Set_True()
        {
            var ps = nullableSymbolicValue.TrySetConstraint(BoolConstraint.False, new ProgramState()).Single();

            var newProgramStates = nullableSymbolicValue.TrySetConstraint(BoolConstraint.True, ps).ToList();

            newProgramStates.Should().BeEmpty();
        }

        [TestMethod]
        public void TrySetConstraint_Existing_False_Set_False()
        {
            var ps = nullableSymbolicValue.TrySetConstraint(BoolConstraint.False, new ProgramState()).Single();

            var newProgramStates = nullableSymbolicValue.TrySetConstraint(BoolConstraint.False, ps).ToList();

            newProgramStates.Should().Equal(ps);
        }

        [TestMethod]
        public void TrySetConstraint_Existing_False_Set_NoValue()
        {
            var ps = nullableSymbolicValue.TrySetConstraint(BoolConstraint.False, new ProgramState()).Single();

            var newProgramStates = nullableSymbolicValue.TrySetConstraint(NullableConstraint.NoValue, ps).ToList();

            newProgramStates.Should().BeEmpty();
        }

        [TestMethod]
        public void TrySetConstraint_Existing_False_Set_HasValue()
        {
            var ps = nullableSymbolicValue.TrySetConstraint(BoolConstraint.False, new ProgramState()).Single();

            var newProgramStates = nullableSymbolicValue.TrySetConstraint(NullableConstraint.HasValue, ps).ToList();

            newProgramStates.Should().Equal(ps);
        }

        [TestMethod]
        public void TrySetConstraint_Existing_False_Set_Null()
        {
            var ps = nullableSymbolicValue.TrySetConstraint(BoolConstraint.False, new ProgramState()).Single();

            var newProgramStates = nullableSymbolicValue.TrySetConstraint(ObjectConstraint.Null, ps).ToList();

            newProgramStates.Should().BeEmpty();
        }

        [TestMethod]
        public void TrySetConstraint_Existing_False_Set_NotNull()
        {
            var ps = nullableSymbolicValue.TrySetConstraint(BoolConstraint.False, new ProgramState()).Single();

            var newProgramStates = nullableSymbolicValue.TrySetConstraint(ObjectConstraint.NotNull, ps).ToList();

            newProgramStates.Should().Equal(ps);
        }

        [TestMethod]
        public void TrySetConstraint_Existing_False_Set_Nothing()
        {
            var ps = nullableSymbolicValue.TrySetConstraint(BoolConstraint.False, new ProgramState()).Single();

            var newProgramStates = nullableSymbolicValue.TrySetConstraint(null, ps).ToList();

            newProgramStates.Should().Equal(ps);
        }

        [TestMethod]
        public void TrySetConstraint_Existing_NoValue_Set_True()
        {
            var ps = nullableSymbolicValue.TrySetConstraint(NullableConstraint.NoValue, new ProgramState()).Single();

            var newProgramStates = nullableSymbolicValue.TrySetConstraint(BoolConstraint.True, ps).ToList();

            newProgramStates.Should().BeEmpty();
        }

        [TestMethod]
        public void TrySetConstraint_Existing_NoValue_Set_False()
        {
            var ps = nullableSymbolicValue.TrySetConstraint(NullableConstraint.NoValue, new ProgramState()).Single();

            var newProgramStates = nullableSymbolicValue.TrySetConstraint(BoolConstraint.False, ps).ToList();

            newProgramStates.Should().BeEmpty();
        }

        [TestMethod]
        public void TrySetConstraint_Existing_NoValue_Set_NoValue()
        {
            var ps = nullableSymbolicValue.TrySetConstraint(NullableConstraint.NoValue, new ProgramState()).Single();

            var newProgramStates = nullableSymbolicValue.TrySetConstraint(NullableConstraint.NoValue, ps).ToList();

            newProgramStates.Should().Equal(ps);
        }

        [TestMethod]
        public void TrySetConstraint_Existing_NoValue_Set_HasValue()
        {
            var ps = nullableSymbolicValue.TrySetConstraint(NullableConstraint.NoValue, new ProgramState()).Single();

            var newProgramStates = nullableSymbolicValue.TrySetConstraint(NullableConstraint.HasValue, ps).ToList();

            newProgramStates.Should().BeEmpty();
        }

        [TestMethod]
        public void TrySetConstraint_Existing_NoValue_Set_Null()
        {
            var ps = nullableSymbolicValue.TrySetConstraint(NullableConstraint.NoValue, new ProgramState()).Single();

            var newProgramStates = nullableSymbolicValue.TrySetConstraint(ObjectConstraint.Null, ps).ToList();

            newProgramStates.Should().Equal(ps);
        }

        [TestMethod]
        public void TrySetConstraint_Existing_NoValue_Set_NotNull()
        {
            var ps = nullableSymbolicValue.TrySetConstraint(NullableConstraint.NoValue, new ProgramState()).Single();

            var newProgramStates = nullableSymbolicValue.TrySetConstraint(ObjectConstraint.NotNull, ps).ToList();

            newProgramStates.Should().BeEmpty();
        }

        [TestMethod]
        public void TrySetConstraint_Existing_NoValue_Set_Nothing()
        {
            var ps = nullableSymbolicValue.TrySetConstraint(NullableConstraint.NoValue, new ProgramState()).Single();

            var newProgramStates = nullableSymbolicValue.TrySetConstraint(null, ps).ToList();

            newProgramStates.Should().Equal(ps);
        }

        [TestMethod]
        public void TrySetConstraint_Existing_HasValue_Set_True()
        {
            var ps = nullableSymbolicValue.TrySetConstraint(NullableConstraint.HasValue, new ProgramState()).Single();

            var newProgramStates = nullableSymbolicValue.TrySetConstraint(BoolConstraint.True, ps).ToList();

            newProgramStates.Should().ContainSingle();
            ShouldHaveConstraint(newProgramStates[0], nullableSymbolicValue, NullableConstraint.HasValue);
            ShouldHaveConstraint(newProgramStates[0], symbolicValue, BoolConstraint.True);
        }

        [TestMethod]
        public void TrySetConstraint_Existing_HasValue_Set_False()
        {
            var ps = nullableSymbolicValue.TrySetConstraint(NullableConstraint.HasValue, new ProgramState()).Single();

            var newProgramStates = nullableSymbolicValue.TrySetConstraint(BoolConstraint.False, ps).ToList();

            newProgramStates.Should().ContainSingle();
            ShouldHaveConstraint(newProgramStates[0], nullableSymbolicValue, NullableConstraint.HasValue);
            ShouldHaveConstraint(newProgramStates[0], symbolicValue, BoolConstraint.False);
        }

        [TestMethod]
        public void TrySetConstraint_Existing_HasValue_Set_NoValue()
        {
            var ps = nullableSymbolicValue.TrySetConstraint(NullableConstraint.HasValue, new ProgramState()).Single();

            var newProgramStates = nullableSymbolicValue.TrySetConstraint(NullableConstraint.NoValue, ps).ToList();

            newProgramStates.Should().BeEmpty();
        }

        [TestMethod]
        public void TrySetConstraint_Existing_HasValue_Set_HasValue()
        {
            var ps = nullableSymbolicValue.TrySetConstraint(NullableConstraint.HasValue, new ProgramState()).Single();

            var newProgramStates = nullableSymbolicValue.TrySetConstraint(NullableConstraint.HasValue, ps).ToList();

            newProgramStates.Should().Equal(ps);
        }

        [TestMethod]
        public void TrySetConstraint_Existing_HasValue_Set_Null()
        {
            var ps = nullableSymbolicValue.TrySetConstraint(NullableConstraint.HasValue, new ProgramState()).Single();

            var newProgramStates = nullableSymbolicValue.TrySetConstraint(ObjectConstraint.Null, ps).ToList();

            newProgramStates.Should().BeEmpty();
        }

        [TestMethod]
        public void TrySetConstraint_Existing_HasValue_Set_NotNull()
        {
            var ps = nullableSymbolicValue.TrySetConstraint(NullableConstraint.HasValue, new ProgramState()).Single();

            var newProgramStates = nullableSymbolicValue.TrySetConstraint(ObjectConstraint.NotNull, ps).ToList();

            newProgramStates.Should().Equal(ps);
        }

        [TestMethod]
        public void TrySetConstraint_Existing_HasValue_Set_Nothing()
        {
            var ps = nullableSymbolicValue.TrySetConstraint(NullableConstraint.HasValue, new ProgramState()).Single();

            var newProgramStates = nullableSymbolicValue.TrySetConstraint(null, ps).ToList();

            newProgramStates.Should().Equal(ps);
        }

        [TestMethod]
        public void TrySetConstraint_Existing_Null_Set_True()
        {
            var ps = nullableSymbolicValue.TrySetConstraint(ObjectConstraint.Null, new ProgramState()).Single();

            var newProgramStates = nullableSymbolicValue.TrySetConstraint(BoolConstraint.True, ps).ToList();

            newProgramStates.Should().BeEmpty();
        }

        [TestMethod]
        public void TrySetConstraint_Existing_Null_Set_False()
        {
            var ps = nullableSymbolicValue.TrySetConstraint(ObjectConstraint.Null, new ProgramState()).Single();

            var newProgramStates = nullableSymbolicValue.TrySetConstraint(BoolConstraint.False, ps).ToList();

            newProgramStates.Should().BeEmpty();
        }

        [TestMethod]
        public void TrySetConstraint_Existing_Null_Set_NoValue()
        {
            var ps = nullableSymbolicValue.TrySetConstraint(ObjectConstraint.Null, new ProgramState()).Single();

            var newProgramStates = nullableSymbolicValue.TrySetConstraint(NullableConstraint.NoValue, ps).ToList();

            newProgramStates.Should().Equal(ps);
        }

        [TestMethod]
        public void TrySetConstraint_Existing_Null_Set_HasValue()
        {
            var ps = nullableSymbolicValue.TrySetConstraint(ObjectConstraint.Null, new ProgramState()).Single();

            var newProgramStates = nullableSymbolicValue.TrySetConstraint(NullableConstraint.HasValue, ps).ToList();

            newProgramStates.Should().BeEmpty();
        }

        [TestMethod]
        public void TrySetConstraint_Existing_Null_Set_Null()
        {
            var ps = nullableSymbolicValue.TrySetConstraint(ObjectConstraint.Null, new ProgramState()).Single();

            var newProgramStates = nullableSymbolicValue.TrySetConstraint(ObjectConstraint.Null, ps).ToList();

            newProgramStates.Should().Equal(ps);
        }

        [TestMethod]
        public void TrySetConstraint_Existing_Null_Set_NotNull()
        {
            var ps = nullableSymbolicValue.TrySetConstraint(ObjectConstraint.Null, new ProgramState()).Single();

            var newProgramStates = nullableSymbolicValue.TrySetConstraint(ObjectConstraint.NotNull, ps).ToList();

            newProgramStates.Should().BeEmpty();
        }

        [TestMethod]
        public void TrySetConstraint_Existing_Null_Set_Nothing()
        {
            var ps = nullableSymbolicValue.TrySetConstraint(ObjectConstraint.Null, new ProgramState()).Single();

            var newProgramStates = nullableSymbolicValue.TrySetConstraint(null, ps).ToList();

            newProgramStates.Should().Equal(ps);
        }

        [TestMethod]
        public void TrySetConstraint_Existing_NotNull_Set_True()
        {
            var ps = nullableSymbolicValue.TrySetConstraint(ObjectConstraint.NotNull, new ProgramState()).Single();

            var newProgramStates = nullableSymbolicValue.TrySetConstraint(BoolConstraint.True, ps).ToList();

            newProgramStates.Should().ContainSingle();
            ShouldHaveConstraint(newProgramStates[0], nullableSymbolicValue, NullableConstraint.HasValue);
            ShouldHaveConstraint(newProgramStates[0], symbolicValue, BoolConstraint.True);
        }

        [TestMethod]
        public void TrySetConstraint_Existing_NotNull_Set_False()
        {
            var ps = nullableSymbolicValue.TrySetConstraint(ObjectConstraint.NotNull, new ProgramState()).Single();

            var newProgramStates = nullableSymbolicValue.TrySetConstraint(BoolConstraint.False, ps).ToList();

            newProgramStates.Should().ContainSingle();
            ShouldHaveConstraint(newProgramStates[0], nullableSymbolicValue, NullableConstraint.HasValue);
            ShouldHaveConstraint(newProgramStates[0], symbolicValue, BoolConstraint.False);
        }

        [TestMethod]
        public void TrySetConstraint_Existing_NotNull_Set_NoValue()
        {
            var ps = nullableSymbolicValue.TrySetConstraint(ObjectConstraint.NotNull, new ProgramState()).Single();

            var newProgramStates = nullableSymbolicValue.TrySetConstraint(NullableConstraint.NoValue, ps).ToList();

            newProgramStates.Should().BeEmpty();
        }

        [TestMethod]
        public void TrySetConstraint_Existing_NotNull_Set_HasValue()
        {
            var ps = nullableSymbolicValue.TrySetConstraint(ObjectConstraint.NotNull, new ProgramState()).Single();

            var newProgramStates = nullableSymbolicValue.TrySetConstraint(NullableConstraint.HasValue, ps).ToList();

            newProgramStates.Should().Equal(ps);
        }

        [TestMethod]
        public void TrySetConstraint_Existing_NotNull_Set_Null()
        {
            var ps = nullableSymbolicValue.TrySetConstraint(ObjectConstraint.NotNull, new ProgramState()).Single();

            var newProgramStates = nullableSymbolicValue.TrySetConstraint(ObjectConstraint.Null, ps).ToList();

            newProgramStates.Should().BeEmpty();
        }

        [TestMethod]
        public void TrySetConstraint_Existing_NotNull_Set_NotNull()
        {
            var ps = nullableSymbolicValue.TrySetConstraint(ObjectConstraint.NotNull, new ProgramState()).Single();

            var newProgramStates = nullableSymbolicValue.TrySetConstraint(ObjectConstraint.NotNull, ps).ToList();

            newProgramStates.Should().Equal(ps);
        }

        [TestMethod]
        public void TrySetConstraint_Existing_NotNull_Set_Nothing()
        {
            var ps = nullableSymbolicValue.TrySetConstraint(ObjectConstraint.NotNull, new ProgramState()).Single();

            var newProgramStates = nullableSymbolicValue.TrySetConstraint(null, ps).ToList();

            newProgramStates.Should().Equal(ps);
        }

        private static void ShouldHaveConstraint(ProgramState ps, SymbolicValue sv, SymbolicConstraint constraint)
        {
            sv.TryGetConstraints(ps, out var existing).Should().BeTrue();
            existing.HasConstraint(constraint).Should().BeTrue();
        }
    }
}
