using System;
using System.Linq;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SonarAnalyzer.Helpers.FlowAnalysis.Common;

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

            newProgramStates.Should().HaveCount(1);
            ShouldHaveConstraint(newProgramStates[0], sv_0, NullableValueConstraint.NoValue);
        }

        private void ShouldHaveConstraint(ProgramState ps, SymbolicValue sv, SymbolicValueConstraint expectedConstraint)
        {
            SymbolicValueConstraint constraint;
            sv.TryGetConstraint(ps, out constraint).Should().BeTrue();
            constraint.Should().Be(expectedConstraint);
        }
    }
}
