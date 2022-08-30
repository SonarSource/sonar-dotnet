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

using SonarAnalyzer.SymbolicExecution.Constraints;
using SonarAnalyzer.SymbolicExecution.Roslyn;
using SonarAnalyzer.UnitTest.TestFramework.SymbolicExecution;

namespace SonarAnalyzer.UnitTest.TestFramework.Tests
{
    [TestClass]
    public class SymbolicValueAssertionsTests
    {
        [TestMethod]
        public void SymbolicValue_HaveConstraint_Pass()
        {
            var sv = new SymbolicValue().WithConstraint(BoolConstraint.True);
            sv.Invoking(x => x.Should().HaveConstraint(BoolConstraint.True)).Should().NotThrow();
        }

        [TestMethod]
        public void SymbolicValue_HaveConstraint_WrongConstraint()
        {
            var sv = new SymbolicValue().WithConstraint(BoolConstraint.True);
            sv.Invoking(x => x.Should().HaveConstraint(BoolConstraint.False, because: "{0} say so", "I")).Should()
                .Throw<AssertFailedException>()
                .WithMessage(@"Expected symbolicValue to have constraint False because I say so, but symbolicValue has {True} constraints.");
        }

        [TestMethod]
        public void SymbolicValue_HaveConstraint_MultipleConstraints()
        {
            var sv = new SymbolicValue().WithConstraint(BoolConstraint.True).WithConstraint(ObjectConstraint.Null);
            sv.Invoking(x => x.Should().HaveConstraint(BoolConstraint.True)).Should().NotThrow();
        }

        [TestMethod]
        public void SymbolicValue_HaveConstraint_NoConstraint()
        {
            var sv = new SymbolicValue();
            sv.Invoking(x => x.Should().HaveConstraint(BoolConstraint.True)).Should()
                .Throw<AssertFailedException>()
                .WithMessage("Expected symbolicValue to have constraint True, but symbolicValue has no constraints.");
        }

        [TestMethod]
        public void SymbolicValue_HaveConstraint_OnNull()
        {
            SymbolicValue sv = null;
            sv.Invoking(x => x.Should().HaveConstraint(BoolConstraint.True)).Should()
                .Throw<AssertFailedException>()
                .WithMessage("The symbolicValue is not present and can not have constraint True.");
        }

        [TestMethod]
        public void SymbolicValue_HaveOnlyConstraint_Pass()
        {
            var sv = new SymbolicValue().WithConstraint(BoolConstraint.True);
            sv.Invoking(x => x.Should().HaveOnlyConstraint(BoolConstraint.True)).Should().NotThrow();
        }

        [TestMethod]
        public void SymbolicValue_HaveOnlyConstraint_WrongConstraint()
        {
            var sv = new SymbolicValue().WithConstraint(BoolConstraint.True);
            sv.Invoking(x => x.Should().HaveOnlyConstraint(BoolConstraint.False, because: "{0} say so", "I")).Should()
                .Throw<AssertFailedException>()
                .WithMessage(@"Expected symbolicValue to have constraint False because I say so, but symbolicValue has True constraint.");
        }

        [TestMethod]
        public void SymbolicValue_HaveOnlyConstraint_MultipleConstraints()
        {
            var sv = new SymbolicValue().WithConstraint(BoolConstraint.True).WithConstraint(ObjectConstraint.Null);
            sv.Invoking(x => x.Should().HaveOnlyConstraint(BoolConstraint.True)).Should()
                .Throw<AssertFailedException>()
                .WithMessage(@"Expected symbolicValue to have only constraint True, but symbolicValue has {Null, True} constraints.");
        }

        [TestMethod]
        public void SymbolicValue_HaveOnlyConstraint_NoConstraint()
        {
            var sv = new SymbolicValue();
            sv.Invoking(x => x.Should().HaveOnlyConstraint(BoolConstraint.True)).Should()
                .Throw<AssertFailedException>()
                .WithMessage("Expected symbolicValue to have constraint True, but symbolicValue has no constraints.");
        }

        [TestMethod]
        public void SymbolicValue_HaveOnlyConstraint_OnNull()
        {
            SymbolicValue sv = null;
            sv.Invoking(x => x.Should().HaveOnlyConstraint(BoolConstraint.True)).Should()
                .Throw<AssertFailedException>()
                .WithMessage("The symbolicValue is not present and can not have constraint True.");
        }

        [TestMethod]
        public void SymbolicValue_HaveNoConstraint_OnNull()
        {
            SymbolicValue sv = null;
            sv.Invoking(x => x.Should().HaveNoConstraints()).Should().NotThrow();
        }

        [TestMethod]
        public void SymbolicValue_HaveNoConstraint_OnEmpty()
        {
            var sv = new SymbolicValue();
            sv.Invoking(x => x.Should().HaveNoConstraints()).Should().NotThrow();
        }

        [TestMethod]
        public void SymbolicValue_HaveNoConstraint_WithConstraint()
        {
            var sv = new SymbolicValue().WithConstraint(BoolConstraint.True);
            sv.Invoking(x => x.Should().HaveNoConstraints()).Should().
                Throw<AssertFailedException>()
                .WithMessage(@"Expected symbolicValue to have no constraints, but {True} constraints were found.");
        }

        [TestMethod]
        public void SymbolicValue_HaveConstraints_OnNull()
        {
            SymbolicValue sv = null;
            sv.Invoking(x => x.Should().HaveConstraints(BoolConstraint.True)).Should()
                .Throw<AssertFailedException>()
                .WithMessage(@"The symbolicValue is not present and can not have constraints {True}.");
        }

        [TestMethod]
        public void SymbolicValue_HaveConstraints_EmptyConstraints()
        {
            var sv = new SymbolicValue();
            sv.Invoking(x => x.Should().HaveConstraints(new[] { BoolConstraint.True }, because: "{0} says so", "I")).Should()
                .Throw<AssertFailedException>()
                .WithMessage(@"Expected symbolicValue to have constraints {True} because I says so, but symbolicValue has no constraints.");
        }

        [TestMethod]
        public void SymbolicValue_HaveConstraints_Empty()
        {
            SymbolicValue sv = null;
            sv.Invoking(x => x.Should().HaveConstraints()).Should()
                .Throw<AssertFailedException>()
                .WithMessage(@"Expected constraints is empty. Use HaveNoConstraints() instead.");
        }

        [TestMethod]
        public void SymbolicValue_HaveConstraints_OneMissing()
        {
            var sv = new SymbolicValue().WithConstraint(BoolConstraint.True);
            sv.Invoking(x => x.Should().HaveConstraints(BoolConstraint.True, ObjectConstraint.NotNull)).Should()
                .Throw<AssertFailedException>()
                .WithMessage(@"Expected symbolicValue to have constraints {True, NotNull}, but constraints {NotNull} are missing. Actual constraints {True}.");
        }

        [TestMethod]
        public void SymbolicValue_HaveConstraints_OneAdditional()
        {
            var sv = new SymbolicValue().WithConstraint(BoolConstraint.True).WithConstraint(ObjectConstraint.NotNull);
            sv.Invoking(x => x.Should().HaveConstraints(BoolConstraint.True)).Should().NotThrow();
        }

        [TestMethod]
        public void SymbolicValue_HaveConstraints_MatchSome()
        {
            var sv = new SymbolicValue().WithConstraint(BoolConstraint.True).WithConstraint(ObjectConstraint.NotNull);
            sv.Invoking(x => x.Should().HaveConstraints(BoolConstraint.True, ObjectConstraint.Null)).Should()
                .Throw<AssertFailedException>()
                .WithMessage(@"Expected symbolicValue to have constraints {True, Null}, but constraints {Null} are missing. Actual constraints {NotNull, True}.");
        }

        [TestMethod]
        public void SymbolicValue_HaveOnlyConstraints_OnNull()
        {
            SymbolicValue sv = null;
            sv.Invoking(x => x.Should().HaveOnlyConstraints(BoolConstraint.True)).Should()
                .Throw<AssertFailedException>()
                .WithMessage(@"The symbolicValue is not present and can not have constraints {True}.");
        }

        [TestMethod]
        public void SymbolicValue_HaveOnlyConstraints_EmptyConstraints()
        {
            var sv = new SymbolicValue();
            sv.Invoking(x => x.Should().HaveOnlyConstraints(new[] { BoolConstraint.True }, because: "{0} says so", "I")).Should()
                .Throw<AssertFailedException>()
                .WithMessage(@"Expected symbolicValue to have constraints {True} because I says so, but symbolicValue has no constraints.");
        }

        [TestMethod]
        public void SymbolicValue_HaveOnlyConstraints_Empty()
        {
            SymbolicValue sv = null;
            sv.Invoking(x => x.Should().HaveOnlyConstraints()).Should()
                .Throw<AssertFailedException>()
                .WithMessage(@"Expected constraints is empty. Use HaveNoConstraints() instead.");
        }

        [TestMethod]
        public void SymbolicValue_HaveOnlyConstraints_OneMissing()
        {
            var sv = new SymbolicValue().WithConstraint(BoolConstraint.True);
            sv.Invoking(x => x.Should().HaveOnlyConstraints(BoolConstraint.True, ObjectConstraint.NotNull)).Should()
                .Throw<AssertFailedException>()
                .WithMessage(@"Expected symbolicValue to only have constraints {True, NotNull}, but constraints {NotNull} are missing. Actual constraints {True}.");
        }

        [TestMethod]
        public void SymbolicValue_HaveOnlyConstraints_OneAdditional()
        {
            var sv = new SymbolicValue().WithConstraint(BoolConstraint.True).WithConstraint(ObjectConstraint.NotNull);
            sv.Invoking(x => x.Should().HaveOnlyConstraints(BoolConstraint.True)).Should()
                .Throw<AssertFailedException>()
                .WithMessage(@"Expected symbolicValue to only have constraints {True}, but {NotNull} additional constraints are present. Actual constraints {NotNull, True}.");
        }

        [TestMethod]
        public void SymbolicValue_HaveOnlyConstraints_MatchSome()
        {
            var sv = new SymbolicValue().WithConstraint(BoolConstraint.True).WithConstraint(ObjectConstraint.NotNull);
            sv.Invoking(x => x.Should().HaveOnlyConstraints(BoolConstraint.True, ObjectConstraint.Null)).Should()
                .Throw<AssertFailedException>()
                .WithMessage(@"Expected symbolicValue to have constraints {True, Null}, but constraints {Null} are missing and additional constraints {NotNull} are present. Actual constraints {NotNull, True}.");
        }
    }
}
