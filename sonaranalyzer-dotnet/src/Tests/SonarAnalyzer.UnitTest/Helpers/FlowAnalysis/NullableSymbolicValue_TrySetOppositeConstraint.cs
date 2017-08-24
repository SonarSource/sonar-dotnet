/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2017 SonarSource SA
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
using SonarAnalyzer.SymbolicExecution;
using SonarAnalyzer.SymbolicExecution.Constraints;
using SonarAnalyzer.SymbolicExecution.SymbolicValues;

namespace SonarAnalyzer.UnitTest.Helpers.FlowAnalysis
{
    [TestClass]
    public class NullableSymbolicValue_TrySetOppositeConstraint
    {
        private SymbolicValue sv_w;
        private NullableSymbolicValue sv_0;

        [TestInitialize]
        public void TestInitialize()
        {
            sv_w = SymbolicValue.Create();
            sv_0 = new NullableSymbolicValue(sv_w);
        }

        [TestMethod]
        public void TrySetOppositeConstraint_Existing_No_Set_True()
        {
            var ps = new ProgramState();

            var newProgramStates = sv_0.TrySetOppositeConstraint(BoolConstraint.True, ps).ToList();

            newProgramStates.Should().HaveCount(2);
            ShouldHaveConstraint(newProgramStates[0], sv_0, NullableValueConstraint.HasValue);
            ShouldHaveConstraint(newProgramStates[0], sv_w, BoolConstraint.False);
            ShouldHaveConstraint(newProgramStates[1], sv_0, NullableValueConstraint.NoValue);
        }

        [TestMethod]
        public void TrySetOppositeConstraint_Existing_No_Set_False()
        {
            var ps = new ProgramState();

            var newProgramStates = sv_0.TrySetOppositeConstraint(BoolConstraint.False, ps).ToList();

            newProgramStates.Should().HaveCount(2);
            ShouldHaveConstraint(newProgramStates[0], sv_0, NullableValueConstraint.HasValue);
            ShouldHaveConstraint(newProgramStates[0], sv_w, BoolConstraint.True);
            ShouldHaveConstraint(newProgramStates[1], sv_0, NullableValueConstraint.NoValue);
        }

        [TestMethod]
        public void TrySetOppositeConstraint_Existing_No_Set_None()
        {
            var ps = new ProgramState();

            var newProgramStates = sv_0.TrySetOppositeConstraint(NullableValueConstraint.NoValue, ps).ToList();

            newProgramStates.Should().HaveCount(1);
            ShouldHaveConstraint(newProgramStates[0], sv_0, NullableValueConstraint.HasValue);
        }

        [TestMethod]
        public void TrySetOppositeConstraint_Existing_No_Set_Some()
        {
            var ps = new ProgramState();

            var newProgramStates = sv_0.TrySetOppositeConstraint(NullableValueConstraint.HasValue, ps).ToList();

            newProgramStates.Should().HaveCount(1);
            ShouldHaveConstraint(newProgramStates[0], sv_0, NullableValueConstraint.NoValue);
        }

        [TestMethod]
        public void TrySetOppositeConstraint_Existing_No_Set_Null()
        {
            var ps = new ProgramState();

            var newProgramStates = sv_0.TrySetOppositeConstraint(ObjectConstraint.Null, ps).ToList();

            newProgramStates.Should().HaveCount(1);
            ShouldHaveConstraint(newProgramStates[0], sv_0, NullableValueConstraint.HasValue);
        }

        [TestMethod]
        public void TrySetOppositeConstraint_Existing_No_Set_NotNull()
        {
            var ps = new ProgramState();

            var newProgramStates = sv_0.TrySetOppositeConstraint(ObjectConstraint.NotNull, ps).ToList();

            newProgramStates.Should().BeEquivalentTo(ps);
        }

        private void ShouldHaveConstraint(ProgramState ps, SymbolicValue sv, SymbolicValueConstraint expectedConstraint)
        {
            SymbolicValueConstraints constraints;
            sv.TryGetConstraints(ps, out constraints).Should().BeTrue();
            constraints.HasConstraint(expectedConstraint).Should().BeTrue();
        }
    }
}
