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
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SonarAnalyzer.SymbolicExecution.Constraints;

namespace SonarAnalyzer.SymbolicExecution.SymbolicValues
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
            ShouldHaveConstraint(newProgramStates[0], nullableSymbolicValue, NullableValueConstraint.HasValue);
            ShouldHaveConstraint(newProgramStates[0], symbolicValue, BoolConstraint.True);
        }

        [TestMethod]
        public void TrySetConstraint_Existing_No_Set_False()
        {
            var ps = new ProgramState();

            var newProgramStates = nullableSymbolicValue.TrySetConstraint(BoolConstraint.False, ps).ToList();

            newProgramStates.Should().ContainSingle();
            ShouldHaveConstraint(newProgramStates[0], nullableSymbolicValue, NullableValueConstraint.HasValue);
            ShouldHaveConstraint(newProgramStates[0], symbolicValue, BoolConstraint.False);
        }

        [TestMethod]
        public void TrySetConstraint_Existing_No_Set_NoValue()
        {
            var ps = new ProgramState();

            var newProgramStates = nullableSymbolicValue.TrySetConstraint(NullableValueConstraint.NoValue, ps).ToList();

            newProgramStates.Should().ContainSingle();
            ShouldHaveConstraint(newProgramStates[0], nullableSymbolicValue, NullableValueConstraint.NoValue);
        }

        [TestMethod]
        public void TrySetConstraint_Existing_No_Set_HasValue()
        {
            var ps = new ProgramState();

            var newProgramStates = nullableSymbolicValue.TrySetConstraint(NullableValueConstraint.HasValue, ps).ToList();

            newProgramStates.Should().ContainSingle();
            ShouldHaveConstraint(newProgramStates[0], nullableSymbolicValue, NullableValueConstraint.HasValue);
        }

        [TestMethod]
        public void TrySetConstraint_Existing_No_Set_Null()
        {
            var ps = new ProgramState();

            var newProgramStates = nullableSymbolicValue.TrySetConstraint(ObjectConstraint.Null, ps).ToList();

            newProgramStates.Should().ContainSingle();
            ShouldHaveConstraint(newProgramStates[0], nullableSymbolicValue, NullableValueConstraint.NoValue);
        }

        [TestMethod]
        public void TrySetConstraint_Existing_No_Set_NotNull()
        {
            var ps = new ProgramState();

            var newProgramStates = nullableSymbolicValue.TrySetConstraint(ObjectConstraint.NotNull, ps).ToList();

            newProgramStates.Should().ContainSingle();
            ShouldHaveConstraint(newProgramStates[0], nullableSymbolicValue, NullableValueConstraint.HasValue);
        }

        [TestMethod]
        public void TrySetConstraint_Existing_No_Set_Nothing()
        {
            var ps = new ProgramState();

            var newProgramStates = nullableSymbolicValue.TrySetConstraint(null, ps).ToList();

            newProgramStates.Should().BeEquivalentTo(ps);
        }

        [TestMethod]
        public void TrySetConstraint_Existing_True_Set_True()
        {
            var ps = nullableSymbolicValue.TrySetConstraint(BoolConstraint.True, new ProgramState()).Single();

            var newProgramStates = nullableSymbolicValue.TrySetConstraint(BoolConstraint.True, ps).ToList();

            newProgramStates.Should().BeEquivalentTo(ps);
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

            var newProgramStates = nullableSymbolicValue.TrySetConstraint(NullableValueConstraint.NoValue, ps).ToList();

            newProgramStates.Should().BeEmpty();
        }

        [TestMethod]
        public void TrySetConstraint_Existing_True_Set_HasValue()
        {
            var ps = nullableSymbolicValue.TrySetConstraint(BoolConstraint.True, new ProgramState()).Single();

            var newProgramStates = nullableSymbolicValue.TrySetConstraint(NullableValueConstraint.HasValue, ps).ToList();

            newProgramStates.Should().BeEquivalentTo(ps);
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

            newProgramStates.Should().BeEquivalentTo(ps);
        }

        [TestMethod]
        public void TrySetConstraint_Existing_True_Set_Nothing()
        {
            var ps = nullableSymbolicValue.TrySetConstraint(BoolConstraint.True, new ProgramState()).Single();

            var newProgramStates = nullableSymbolicValue.TrySetConstraint(null, ps).ToList();

            newProgramStates.Should().BeEquivalentTo(ps);
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

            newProgramStates.Should().BeEquivalentTo(ps);
        }

        [TestMethod]
        public void TrySetConstraint_Existing_False_Set_NoValue()
        {
            var ps = nullableSymbolicValue.TrySetConstraint(BoolConstraint.False, new ProgramState()).Single();

            var newProgramStates = nullableSymbolicValue.TrySetConstraint(NullableValueConstraint.NoValue, ps).ToList();

            newProgramStates.Should().BeEmpty();
        }

        [TestMethod]
        public void TrySetConstraint_Existing_False_Set_HasValue()
        {
            var ps = nullableSymbolicValue.TrySetConstraint(BoolConstraint.False, new ProgramState()).Single();

            var newProgramStates = nullableSymbolicValue.TrySetConstraint(NullableValueConstraint.HasValue, ps).ToList();

            newProgramStates.Should().BeEquivalentTo(ps);
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

            newProgramStates.Should().BeEquivalentTo(ps);
        }

        [TestMethod]
        public void TrySetConstraint_Existing_False_Set_Nothing()
        {
            var ps = nullableSymbolicValue.TrySetConstraint(BoolConstraint.False, new ProgramState()).Single();

            var newProgramStates = nullableSymbolicValue.TrySetConstraint(null, ps).ToList();

            newProgramStates.Should().BeEquivalentTo(ps);
        }

        [TestMethod]
        public void TrySetConstraint_Existing_NoValue_Set_True()
        {
            var ps = nullableSymbolicValue.TrySetConstraint(NullableValueConstraint.NoValue, new ProgramState()).Single();

            var newProgramStates = nullableSymbolicValue.TrySetConstraint(BoolConstraint.True, ps).ToList();

            newProgramStates.Should().BeEmpty();
        }

        [TestMethod]
        public void TrySetConstraint_Existing_NoValue_Set_False()
        {
            var ps = nullableSymbolicValue.TrySetConstraint(NullableValueConstraint.NoValue, new ProgramState()).Single();

            var newProgramStates = nullableSymbolicValue.TrySetConstraint(BoolConstraint.False, ps).ToList();

            newProgramStates.Should().BeEmpty();
        }

        [TestMethod]
        public void TrySetConstraint_Existing_NoValue_Set_NoValue()
        {
            var ps = nullableSymbolicValue.TrySetConstraint(NullableValueConstraint.NoValue, new ProgramState()).Single();

            var newProgramStates = nullableSymbolicValue.TrySetConstraint(NullableValueConstraint.NoValue, ps).ToList();

            newProgramStates.Should().BeEquivalentTo(ps);
        }

        [TestMethod]
        public void TrySetConstraint_Existing_NoValue_Set_HasValue()
        {
            var ps = nullableSymbolicValue.TrySetConstraint(NullableValueConstraint.NoValue, new ProgramState()).Single();

            var newProgramStates = nullableSymbolicValue.TrySetConstraint(NullableValueConstraint.HasValue, ps).ToList();

            newProgramStates.Should().BeEmpty();
        }

        [TestMethod]
        public void TrySetConstraint_Existing_NoValue_Set_Null()
        {
            var ps = nullableSymbolicValue.TrySetConstraint(NullableValueConstraint.NoValue, new ProgramState()).Single();

            var newProgramStates = nullableSymbolicValue.TrySetConstraint(ObjectConstraint.Null, ps).ToList();

            newProgramStates.Should().BeEquivalentTo(ps);
        }

        [TestMethod]
        public void TrySetConstraint_Existing_NoValue_Set_NotNull()
        {
            var ps = nullableSymbolicValue.TrySetConstraint(NullableValueConstraint.NoValue, new ProgramState()).Single();

            var newProgramStates = nullableSymbolicValue.TrySetConstraint(ObjectConstraint.NotNull, ps).ToList();

            newProgramStates.Should().BeEmpty();
        }

        [TestMethod]
        public void TrySetConstraint_Existing_NoValue_Set_Nothing()
        {
            var ps = nullableSymbolicValue.TrySetConstraint(NullableValueConstraint.NoValue, new ProgramState()).Single();

            var newProgramStates = nullableSymbolicValue.TrySetConstraint(null, ps).ToList();

            newProgramStates.Should().BeEquivalentTo(ps);
        }

        [TestMethod]
        public void TrySetConstraint_Existing_HasValue_Set_True()
        {
            var ps = nullableSymbolicValue.TrySetConstraint(NullableValueConstraint.HasValue, new ProgramState()).Single();

            var newProgramStates = nullableSymbolicValue.TrySetConstraint(BoolConstraint.True, ps).ToList();

            newProgramStates.Should().ContainSingle();
            ShouldHaveConstraint(newProgramStates[0], nullableSymbolicValue, NullableValueConstraint.HasValue);
            ShouldHaveConstraint(newProgramStates[0], symbolicValue, BoolConstraint.True);
        }

        [TestMethod]
        public void TrySetConstraint_Existing_HasValue_Set_False()
        {
            var ps = nullableSymbolicValue.TrySetConstraint(NullableValueConstraint.HasValue, new ProgramState()).Single();

            var newProgramStates = nullableSymbolicValue.TrySetConstraint(BoolConstraint.False, ps).ToList();

            newProgramStates.Should().ContainSingle();
            ShouldHaveConstraint(newProgramStates[0], nullableSymbolicValue, NullableValueConstraint.HasValue);
            ShouldHaveConstraint(newProgramStates[0], symbolicValue, BoolConstraint.False);
        }

        [TestMethod]
        public void TrySetConstraint_Existing_HasValue_Set_NoValue()
        {
            var ps = nullableSymbolicValue.TrySetConstraint(NullableValueConstraint.HasValue, new ProgramState()).Single();

            var newProgramStates = nullableSymbolicValue.TrySetConstraint(NullableValueConstraint.NoValue, ps).ToList();

            newProgramStates.Should().BeEmpty();
        }

        [TestMethod]
        public void TrySetConstraint_Existing_HasValue_Set_HasValue()
        {
            var ps = nullableSymbolicValue.TrySetConstraint(NullableValueConstraint.HasValue, new ProgramState()).Single();

            var newProgramStates = nullableSymbolicValue.TrySetConstraint(NullableValueConstraint.HasValue, ps).ToList();

            newProgramStates.Should().BeEquivalentTo(ps);
        }

        [TestMethod]
        public void TrySetConstraint_Existing_HasValue_Set_Null()
        {
            var ps = nullableSymbolicValue.TrySetConstraint(NullableValueConstraint.HasValue, new ProgramState()).Single();

            var newProgramStates = nullableSymbolicValue.TrySetConstraint(ObjectConstraint.Null, ps).ToList();

            newProgramStates.Should().BeEmpty();
        }

        [TestMethod]
        public void TrySetConstraint_Existing_HasValue_Set_NotNull()
        {
            var ps = nullableSymbolicValue.TrySetConstraint(NullableValueConstraint.HasValue, new ProgramState()).Single();

            var newProgramStates = nullableSymbolicValue.TrySetConstraint(ObjectConstraint.NotNull, ps).ToList();

            newProgramStates.Should().BeEquivalentTo(ps);
        }

        [TestMethod]
        public void TrySetConstraint_Existing_HasValue_Set_Nothing()
        {
            var ps = nullableSymbolicValue.TrySetConstraint(NullableValueConstraint.HasValue, new ProgramState()).Single();

            var newProgramStates = nullableSymbolicValue.TrySetConstraint(null, ps).ToList();

            newProgramStates.Should().BeEquivalentTo(ps);
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

            var newProgramStates = nullableSymbolicValue.TrySetConstraint(NullableValueConstraint.NoValue, ps).ToList();

            newProgramStates.Should().BeEquivalentTo(ps);
        }

        [TestMethod]
        public void TrySetConstraint_Existing_Null_Set_HasValue()
        {
            var ps = nullableSymbolicValue.TrySetConstraint(ObjectConstraint.Null, new ProgramState()).Single();

            var newProgramStates = nullableSymbolicValue.TrySetConstraint(NullableValueConstraint.HasValue, ps).ToList();

            newProgramStates.Should().BeEmpty();
        }

        [TestMethod]
        public void TrySetConstraint_Existing_Null_Set_Null()
        {
            var ps = nullableSymbolicValue.TrySetConstraint(ObjectConstraint.Null, new ProgramState()).Single();

            var newProgramStates = nullableSymbolicValue.TrySetConstraint(ObjectConstraint.Null, ps).ToList();

            newProgramStates.Should().BeEquivalentTo(ps);
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

            newProgramStates.Should().BeEquivalentTo(ps);
        }

        [TestMethod]
        public void TrySetConstraint_Existing_NotNull_Set_True()
        {
            var ps = nullableSymbolicValue.TrySetConstraint(ObjectConstraint.NotNull, new ProgramState()).Single();

            var newProgramStates = nullableSymbolicValue.TrySetConstraint(BoolConstraint.True, ps).ToList();

            newProgramStates.Should().ContainSingle();
            ShouldHaveConstraint(newProgramStates[0], nullableSymbolicValue, NullableValueConstraint.HasValue);
            ShouldHaveConstraint(newProgramStates[0], symbolicValue, BoolConstraint.True);
        }

        [TestMethod]
        public void TrySetConstraint_Existing_NotNull_Set_False()
        {
            var ps = nullableSymbolicValue.TrySetConstraint(ObjectConstraint.NotNull, new ProgramState()).Single();

            var newProgramStates = nullableSymbolicValue.TrySetConstraint(BoolConstraint.False, ps).ToList();

            newProgramStates.Should().ContainSingle();
            ShouldHaveConstraint(newProgramStates[0], nullableSymbolicValue, NullableValueConstraint.HasValue);
            ShouldHaveConstraint(newProgramStates[0], symbolicValue, BoolConstraint.False);
        }

        [TestMethod]
        public void TrySetConstraint_Existing_NotNull_Set_NoValue()
        {
            var ps = nullableSymbolicValue.TrySetConstraint(ObjectConstraint.NotNull, new ProgramState()).Single();

            var newProgramStates = nullableSymbolicValue.TrySetConstraint(NullableValueConstraint.NoValue, ps).ToList();

            newProgramStates.Should().BeEmpty();
        }

        [TestMethod]
        public void TrySetConstraint_Existing_NotNull_Set_HasValue()
        {
            var ps = nullableSymbolicValue.TrySetConstraint(ObjectConstraint.NotNull, new ProgramState()).Single();

            var newProgramStates = nullableSymbolicValue.TrySetConstraint(NullableValueConstraint.HasValue, ps).ToList();

            newProgramStates.Should().BeEquivalentTo(ps);
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

            newProgramStates.Should().BeEquivalentTo(ps);
        }

        [TestMethod]
        public void TrySetConstraint_Existing_NotNull_Set_Nothing()
        {
            var ps = nullableSymbolicValue.TrySetConstraint(ObjectConstraint.NotNull, new ProgramState()).Single();

            var newProgramStates = nullableSymbolicValue.TrySetConstraint(null, ps).ToList();

            newProgramStates.Should().BeEquivalentTo(ps);
        }

        private static void ShouldHaveConstraint(ProgramState ps, SymbolicValue sv, SymbolicValueConstraint constraint)
        {
            sv.TryGetConstraints(ps, out var existing).Should().BeTrue();
            existing.HasConstraint(constraint).Should().BeTrue();
        }
    }
}
