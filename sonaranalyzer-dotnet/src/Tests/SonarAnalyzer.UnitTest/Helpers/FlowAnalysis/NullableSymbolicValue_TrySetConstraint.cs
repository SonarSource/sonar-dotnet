using System;
using System.Linq;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SonarAnalyzer.Helpers.FlowAnalysis.Common;

namespace SonarAnalyzer.UnitTest.Helpers.FlowAnalysis
{
    [TestClass]
    public class NullableSymbolicValue_TrySetConstraint
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
        public void TrySetConstraint_Existing_No_Set_True()
        {
            var ps = new ProgramState();

            var newProgramStates = sv_0.TrySetConstraint(BoolConstraint.True, ps).ToList();

            newProgramStates.Should().HaveCount(1);
            ShouldHaveConstraint(newProgramStates[0], sv_0, NullableValueConstraint.HasValue);
            ShouldHaveConstraint(newProgramStates[0], sv_w, BoolConstraint.True);
        }

        [TestMethod]
        public void TrySetConstraint_Existing_No_Set_False()
        {
            var ps = new ProgramState();

            var newProgramStates = sv_0.TrySetConstraint(BoolConstraint.False, ps).ToList();

            newProgramStates.Should().HaveCount(1);
            ShouldHaveConstraint(newProgramStates[0], sv_0, NullableValueConstraint.HasValue);
            ShouldHaveConstraint(newProgramStates[0], sv_w, BoolConstraint.False);
        }

        [TestMethod]
        public void TrySetConstraint_Existing_No_Set_NoValue()
        {
            var ps = new ProgramState();

            var newProgramStates = sv_0.TrySetConstraint(NullableValueConstraint.NoValue, ps).ToList();

            newProgramStates.Should().HaveCount(1);
            ShouldHaveConstraint(newProgramStates[0], sv_0, NullableValueConstraint.NoValue);
        }

        [TestMethod]
        public void TrySetConstraint_Existing_No_Set_HasValue()
        {
            var ps = new ProgramState();

            var newProgramStates = sv_0.TrySetConstraint(NullableValueConstraint.HasValue, ps).ToList();

            newProgramStates.Should().HaveCount(1);
            ShouldHaveConstraint(newProgramStates[0], sv_0, NullableValueConstraint.HasValue);
        }

        [TestMethod]
        public void TrySetConstraint_Existing_No_Set_Null()
        {
            var ps = new ProgramState();

            var newProgramStates = sv_0.TrySetConstraint(ObjectConstraint.Null, ps).ToList();

            newProgramStates.Should().HaveCount(1);
            ShouldHaveConstraint(newProgramStates[0], sv_0, NullableValueConstraint.NoValue);
        }

        [TestMethod]
        public void TrySetConstraint_Existing_No_Set_NotNull()
        {
            var ps = new ProgramState();

            var newProgramStates = sv_0.TrySetConstraint(ObjectConstraint.NotNull, ps).ToList();

            newProgramStates.Should().HaveCount(1);
            ShouldHaveConstraint(newProgramStates[0], sv_0, NullableValueConstraint.HasValue);
        }

        [TestMethod]
        public void TrySetConstraint_Existing_No_Set_Nothing()
        {
            var ps = new ProgramState();

            var newProgramStates = sv_0.TrySetConstraint(null, ps).ToList();

            newProgramStates.Should().BeEquivalentTo(ps);
        }

        [TestMethod]
        public void TrySetConstraint_Existing_True_Set_True()
        {
            var ps = sv_0.TrySetConstraint(BoolConstraint.True, new ProgramState()).Single();

            var newProgramStates = sv_0.TrySetConstraint(BoolConstraint.True, ps).ToList();

            newProgramStates.Should().BeEquivalentTo(ps);
        }

        [TestMethod]
        public void TrySetConstraint_Existing_True_Set_False()
        {
            var ps = sv_0.TrySetConstraint(BoolConstraint.True, new ProgramState()).Single();

            var newProgramStates = sv_0.TrySetConstraint(BoolConstraint.False, ps).ToList();

            newProgramStates.Should().HaveCount(0);
        }

        [TestMethod]
        public void TrySetConstraint_Existing_True_Set_NoValue()
        {
            var ps = sv_0.TrySetConstraint(BoolConstraint.True, new ProgramState()).Single();

            var newProgramStates = sv_0.TrySetConstraint(NullableValueConstraint.NoValue, ps).ToList();

            newProgramStates.Should().HaveCount(0);
        }

        [TestMethod]
        public void TrySetConstraint_Existing_True_Set_HasValue()
        {
            var ps = sv_0.TrySetConstraint(BoolConstraint.True, new ProgramState()).Single();

            var newProgramStates = sv_0.TrySetConstraint(NullableValueConstraint.HasValue, ps).ToList();

            newProgramStates.Should().BeEquivalentTo(ps);
        }

        [TestMethod]
        public void TrySetConstraint_Existing_True_Set_Null()
        {
            var ps = sv_0.TrySetConstraint(BoolConstraint.True, new ProgramState()).Single();

            var newProgramStates = sv_0.TrySetConstraint(ObjectConstraint.Null, ps).ToList();

            newProgramStates.Should().HaveCount(0);
        }

        [TestMethod]
        public void TrySetConstraint_Existing_True_Set_NotNull()
        {
            var ps = sv_0.TrySetConstraint(BoolConstraint.True, new ProgramState()).Single();

            var newProgramStates = sv_0.TrySetConstraint(ObjectConstraint.NotNull, ps).ToList();

            newProgramStates.Should().BeEquivalentTo(ps);
        }

        [TestMethod]
        public void TrySetConstraint_Existing_True_Set_Nothing()
        {
            var ps = sv_0.TrySetConstraint(BoolConstraint.True, new ProgramState()).Single();

            var newProgramStates = sv_0.TrySetConstraint(null, ps).ToList();

            newProgramStates.Should().BeEquivalentTo(ps);
        }

        [TestMethod]
        public void TrySetConstraint_Existing_False_Set_True()
        {
            var ps = sv_0.TrySetConstraint(BoolConstraint.False, new ProgramState()).Single();

            var newProgramStates = sv_0.TrySetConstraint(BoolConstraint.True, ps).ToList();

            newProgramStates.Should().HaveCount(0);
        }

        [TestMethod]
        public void TrySetConstraint_Existing_False_Set_False()
        {
            var ps = sv_0.TrySetConstraint(BoolConstraint.False, new ProgramState()).Single();

            var newProgramStates = sv_0.TrySetConstraint(BoolConstraint.False, ps).ToList();

            newProgramStates.Should().BeEquivalentTo(ps);
        }

        [TestMethod]
        public void TrySetConstraint_Existing_False_Set_NoValue()
        {
            var ps = sv_0.TrySetConstraint(BoolConstraint.False, new ProgramState()).Single();

            var newProgramStates = sv_0.TrySetConstraint(NullableValueConstraint.NoValue, ps).ToList();

            newProgramStates.Should().HaveCount(0);
        }

        [TestMethod]
        public void TrySetConstraint_Existing_False_Set_HasValue()
        {
            var ps = sv_0.TrySetConstraint(BoolConstraint.False, new ProgramState()).Single();

            var newProgramStates = sv_0.TrySetConstraint(NullableValueConstraint.HasValue, ps).ToList();

            newProgramStates.Should().BeEquivalentTo(ps);
        }

        [TestMethod]
        public void TrySetConstraint_Existing_False_Set_Null()
        {
            var ps = sv_0.TrySetConstraint(BoolConstraint.False, new ProgramState()).Single();

            var newProgramStates = sv_0.TrySetConstraint(ObjectConstraint.Null, ps).ToList();

            newProgramStates.Should().HaveCount(0);
        }

        [TestMethod]
        public void TrySetConstraint_Existing_False_Set_NotNull()
        {
            var ps = sv_0.TrySetConstraint(BoolConstraint.False, new ProgramState()).Single();

            var newProgramStates = sv_0.TrySetConstraint(ObjectConstraint.NotNull, ps).ToList();

            newProgramStates.Should().BeEquivalentTo(ps);
        }

        [TestMethod]
        public void TrySetConstraint_Existing_False_Set_Nothing()
        {
            var ps = sv_0.TrySetConstraint(BoolConstraint.False, new ProgramState()).Single();

            var newProgramStates = sv_0.TrySetConstraint(null, ps).ToList();

            newProgramStates.Should().BeEquivalentTo(ps);
        }

        [TestMethod]
        public void TrySetConstraint_Existing_NoValue_Set_True()
        {
            var ps = sv_0.TrySetConstraint(NullableValueConstraint.NoValue, new ProgramState()).Single();

            var newProgramStates = sv_0.TrySetConstraint(BoolConstraint.True, ps).ToList();

            newProgramStates.Should().HaveCount(0);
        }

        [TestMethod]
        public void TrySetConstraint_Existing_NoValue_Set_False()
        {
            var ps = sv_0.TrySetConstraint(NullableValueConstraint.NoValue, new ProgramState()).Single();

            var newProgramStates = sv_0.TrySetConstraint(BoolConstraint.False, ps).ToList();

            newProgramStates.Should().HaveCount(0);
        }

        [TestMethod]
        public void TrySetConstraint_Existing_NoValue_Set_NoValue()
        {
            var ps = sv_0.TrySetConstraint(NullableValueConstraint.NoValue, new ProgramState()).Single();

            var newProgramStates = sv_0.TrySetConstraint(NullableValueConstraint.NoValue, ps).ToList();

            newProgramStates.Should().BeEquivalentTo(ps);
        }

        [TestMethod]
        public void TrySetConstraint_Existing_NoValue_Set_HasValue()
        {
            var ps = sv_0.TrySetConstraint(NullableValueConstraint.NoValue, new ProgramState()).Single();

            var newProgramStates = sv_0.TrySetConstraint(NullableValueConstraint.HasValue, ps).ToList();

            newProgramStates.Should().HaveCount(0);
        }

        [TestMethod]
        public void TrySetConstraint_Existing_NoValue_Set_Null()
        {
            var ps = sv_0.TrySetConstraint(NullableValueConstraint.NoValue, new ProgramState()).Single();

            var newProgramStates = sv_0.TrySetConstraint(ObjectConstraint.Null, ps).ToList();

            newProgramStates.Should().BeEquivalentTo(ps);
        }

        [TestMethod]
        public void TrySetConstraint_Existing_NoValue_Set_NotNull()
        {
            var ps = sv_0.TrySetConstraint(NullableValueConstraint.NoValue, new ProgramState()).Single();

            var newProgramStates = sv_0.TrySetConstraint(ObjectConstraint.NotNull, ps).ToList();

            newProgramStates.Should().HaveCount(0);
        }

        [TestMethod]
        public void TrySetConstraint_Existing_NoValue_Set_Nothing()
        {
            var ps = sv_0.TrySetConstraint(NullableValueConstraint.NoValue, new ProgramState()).Single();

            var newProgramStates = sv_0.TrySetConstraint(null, ps).ToList();

            newProgramStates.Should().BeEquivalentTo(ps);
        }

        [TestMethod]
        public void TrySetConstraint_Existing_HasValue_Set_True()
        {
            var ps = sv_0.TrySetConstraint(NullableValueConstraint.HasValue, new ProgramState()).Single();

            var newProgramStates = sv_0.TrySetConstraint(BoolConstraint.True, ps).ToList();

            newProgramStates.Should().HaveCount(1);
            ShouldHaveConstraint(newProgramStates[0], sv_0, NullableValueConstraint.HasValue);
            ShouldHaveConstraint(newProgramStates[0], sv_w, BoolConstraint.True);
        }

        [TestMethod]
        public void TrySetConstraint_Existing_HasValue_Set_False()
        {
            var ps = sv_0.TrySetConstraint(NullableValueConstraint.HasValue, new ProgramState()).Single();

            var newProgramStates = sv_0.TrySetConstraint(BoolConstraint.False, ps).ToList();

            newProgramStates.Should().HaveCount(1);
            ShouldHaveConstraint(newProgramStates[0], sv_0, NullableValueConstraint.HasValue);
            ShouldHaveConstraint(newProgramStates[0], sv_w, BoolConstraint.False);
        }

        [TestMethod]
        public void TrySetConstraint_Existing_HasValue_Set_NoValue()
        {
            var ps = sv_0.TrySetConstraint(NullableValueConstraint.HasValue, new ProgramState()).Single();

            var newProgramStates = sv_0.TrySetConstraint(NullableValueConstraint.NoValue, ps).ToList();

            newProgramStates.Should().HaveCount(0);
        }

        [TestMethod]
        public void TrySetConstraint_Existing_HasValue_Set_HasValue()
        {
            var ps = sv_0.TrySetConstraint(NullableValueConstraint.HasValue, new ProgramState()).Single();

            var newProgramStates = sv_0.TrySetConstraint(NullableValueConstraint.HasValue, ps).ToList();

            newProgramStates.Should().BeEquivalentTo(ps);
        }

        [TestMethod]
        public void TrySetConstraint_Existing_HasValue_Set_Null()
        {
            var ps = sv_0.TrySetConstraint(NullableValueConstraint.HasValue, new ProgramState()).Single();

            var newProgramStates = sv_0.TrySetConstraint(ObjectConstraint.Null, ps).ToList();

            newProgramStates.Should().HaveCount(0);
        }

        [TestMethod]
        public void TrySetConstraint_Existing_HasValue_Set_NotNull()
        {
            var ps = sv_0.TrySetConstraint(NullableValueConstraint.HasValue, new ProgramState()).Single();

            var newProgramStates = sv_0.TrySetConstraint(ObjectConstraint.NotNull, ps).ToList();

            newProgramStates.Should().BeEquivalentTo(ps);
        }

        [TestMethod]
        public void TrySetConstraint_Existing_HasValue_Set_Nothing()
        {
            var ps = sv_0.TrySetConstraint(NullableValueConstraint.HasValue, new ProgramState()).Single();

            var newProgramStates = sv_0.TrySetConstraint(null, ps).ToList();

            newProgramStates.Should().BeEquivalentTo(ps);
        }

        [TestMethod]
        public void TrySetConstraint_Existing_Null_Set_True()
        {
            var ps = sv_0.TrySetConstraint(ObjectConstraint.Null, new ProgramState()).Single();

            var newProgramStates = sv_0.TrySetConstraint(BoolConstraint.True, ps).ToList();

            newProgramStates.Should().HaveCount(0);
        }

        [TestMethod]
        public void TrySetConstraint_Existing_Null_Set_False()
        {
            var ps = sv_0.TrySetConstraint(ObjectConstraint.Null, new ProgramState()).Single();

            var newProgramStates = sv_0.TrySetConstraint(BoolConstraint.False, ps).ToList();

            newProgramStates.Should().HaveCount(0);
        }

        [TestMethod]
        public void TrySetConstraint_Existing_Null_Set_NoValue()
        {
            var ps = sv_0.TrySetConstraint(ObjectConstraint.Null, new ProgramState()).Single();

            var newProgramStates = sv_0.TrySetConstraint(NullableValueConstraint.NoValue, ps).ToList();

            newProgramStates.Should().BeEquivalentTo(ps);
        }

        [TestMethod]
        public void TrySetConstraint_Existing_Null_Set_HasValue()
        {
            var ps = sv_0.TrySetConstraint(ObjectConstraint.Null, new ProgramState()).Single();

            var newProgramStates = sv_0.TrySetConstraint(NullableValueConstraint.HasValue, ps).ToList();

            newProgramStates.Should().HaveCount(0);
        }

        [TestMethod]
        public void TrySetConstraint_Existing_Null_Set_Null()
        {
            var ps = sv_0.TrySetConstraint(ObjectConstraint.Null, new ProgramState()).Single();

            var newProgramStates = sv_0.TrySetConstraint(ObjectConstraint.Null, ps).ToList();

            newProgramStates.Should().BeEquivalentTo(ps);
        }

        [TestMethod]
        public void TrySetConstraint_Existing_Null_Set_NotNull()
        {
            var ps = sv_0.TrySetConstraint(ObjectConstraint.Null, new ProgramState()).Single();

            var newProgramStates = sv_0.TrySetConstraint(ObjectConstraint.NotNull, ps).ToList();

            newProgramStates.Should().HaveCount(0);
        }

        [TestMethod]
        public void TrySetConstraint_Existing_Null_Set_Nothing()
        {
            var ps = sv_0.TrySetConstraint(ObjectConstraint.Null, new ProgramState()).Single();

            var newProgramStates = sv_0.TrySetConstraint(null, ps).ToList();

            newProgramStates.Should().BeEquivalentTo(ps);
        }

        [TestMethod]
        public void TrySetConstraint_Existing_NotNull_Set_True()
        {
            var ps = sv_0.TrySetConstraint(ObjectConstraint.NotNull, new ProgramState()).Single();

            var newProgramStates = sv_0.TrySetConstraint(BoolConstraint.True, ps).ToList();

            newProgramStates.Should().HaveCount(1);
            ShouldHaveConstraint(newProgramStates[0], sv_0, NullableValueConstraint.HasValue);
            ShouldHaveConstraint(newProgramStates[0], sv_w, BoolConstraint.True);
        }

        [TestMethod]
        public void TrySetConstraint_Existing_NotNull_Set_False()
        {
            var ps = sv_0.TrySetConstraint(ObjectConstraint.NotNull, new ProgramState()).Single();

            var newProgramStates = sv_0.TrySetConstraint(BoolConstraint.False, ps).ToList();

            newProgramStates.Should().HaveCount(1);
            ShouldHaveConstraint(newProgramStates[0], sv_0, NullableValueConstraint.HasValue);
            ShouldHaveConstraint(newProgramStates[0], sv_w, BoolConstraint.False);
        }

        [TestMethod]
        public void TrySetConstraint_Existing_NotNull_Set_NoValue()
        {
            var ps = sv_0.TrySetConstraint(ObjectConstraint.NotNull, new ProgramState()).Single();

            var newProgramStates = sv_0.TrySetConstraint(NullableValueConstraint.NoValue, ps).ToList();

            newProgramStates.Should().HaveCount(0);
        }

        [TestMethod]
        public void TrySetConstraint_Existing_NotNull_Set_HasValue()
        {
            var ps = sv_0.TrySetConstraint(ObjectConstraint.NotNull, new ProgramState()).Single();

            var newProgramStates = sv_0.TrySetConstraint(NullableValueConstraint.HasValue, ps).ToList();

            newProgramStates.Should().BeEquivalentTo(ps);
        }

        [TestMethod]
        public void TrySetConstraint_Existing_NotNull_Set_Null()
        {
            var ps = sv_0.TrySetConstraint(ObjectConstraint.NotNull, new ProgramState()).Single();

            var newProgramStates = sv_0.TrySetConstraint(ObjectConstraint.Null, ps).ToList();

            newProgramStates.Should().HaveCount(0);
        }

        [TestMethod]
        public void TrySetConstraint_Existing_NotNull_Set_NotNull()
        {
            var ps = sv_0.TrySetConstraint(ObjectConstraint.NotNull, new ProgramState()).Single();

            var newProgramStates = sv_0.TrySetConstraint(ObjectConstraint.NotNull, ps).ToList();

            newProgramStates.Should().BeEquivalentTo(ps);
        }

        [TestMethod]
        public void TrySetConstraint_Existing_NotNull_Set_Nothing()
        {
            var ps = sv_0.TrySetConstraint(ObjectConstraint.NotNull, new ProgramState()).Single();

            var newProgramStates = sv_0.TrySetConstraint(null, ps).ToList();

            newProgramStates.Should().BeEquivalentTo(ps);
        }

        private void ShouldHaveConstraint(ProgramState ps, SymbolicValue sv, SymbolicValueConstraint constraint)
        {
            SymbolicValueConstraint existing;
            sv.TryGetConstraint(ps, out existing).Should().BeTrue();
            constraint.Should().Be(existing);
        }
    }
}
