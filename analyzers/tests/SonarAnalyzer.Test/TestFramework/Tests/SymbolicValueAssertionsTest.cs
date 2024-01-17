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

using SonarAnalyzer.SymbolicExecution.Constraints;
using SonarAnalyzer.SymbolicExecution.Roslyn;
using SonarAnalyzer.Test.TestFramework.SymbolicExecution;

namespace SonarAnalyzer.Test.TestFramework.Tests
{
    [TestClass]
    public class SymbolicValueAssertionsTest
    {
        [TestMethod]
        public void SymbolicValue_HaveOnlyConstraint_Pass()
        {
            var sv = SymbolicValue.Empty.WithConstraint(BoolConstraint.True);
            sv.Invoking(x => x.Should().HaveOnlyConstraint(BoolConstraint.True)).Should().NotThrow();
        }

        [TestMethod]
        public void SymbolicValue_HaveOnlyConstraints_Pass_ExpectedAlsoNull()
        {
            var sv = SymbolicValue.Empty.WithConstraint(BoolConstraint.True);
            sv.Invoking(x => x.Should().HaveOnlyConstraints(null, BoolConstraint.True, null)).Should().NotThrow();
        }

        [TestMethod]
        public void SymbolicValue_HaveOnlyConstraint_WrongConstraint()
        {
            var sv = SymbolicValue.Empty.WithConstraint(BoolConstraint.True);
            sv.Invoking(x => x.Should().HaveOnlyConstraint(BoolConstraint.False, because: "{0} say so", "I")).Should().Throw<AssertFailedException>()
                // * = x or SymbolicValue depending on compilation
                .WithMessage(@"Expected * to have constraint False because I say so, but SymbolicValue has True constraint.");
        }

        [TestMethod]
        public void SymbolicValue_HaveOnlyConstraint_MultipleConstraints()
        {
            var sv = SymbolicValue.Empty.WithConstraint(BoolConstraint.True).WithConstraint(ObjectConstraint.Null);
            sv.Invoking(x => x.Should().HaveOnlyConstraint(BoolConstraint.True)).Should().Throw<AssertFailedException>()
                // * = x or SymbolicValue depending on compilation
                .WithMessage(@"Expected * to have only constraint True, but SymbolicValue has {Null, True} constraints.");
        }

        [TestMethod]
        public void SymbolicValue_HaveOnlyConstraint_NoConstraint()
        {
            var sv = SymbolicValue.Empty;
            sv.Invoking(x => x.Should().HaveOnlyConstraint(BoolConstraint.True)).Should().Throw<AssertFailedException>()
                // * = x or SymbolicValue depending on compilation
                .WithMessage("Expected * to have constraint True, but SymbolicValue has no constraints.");
        }

        [TestMethod]
        public void SymbolicValue_HaveOnlyConstraint_OnNull()
        {
            var assertion = () => ((SymbolicValue)null).Should().HaveOnlyConstraint(BoolConstraint.True);
            assertion.Should().Throw<AssertFailedException>()
                // * = ((SymbolicValue)null) or SymbolicValue depending on compilation
                .WithMessage("* is <null> and can not have constraint True.");
        }

        [TestMethod]
        public void SymbolicValue_HaveNoConstraint_OnNull()
        {
            var assertion = () => ((SymbolicValue)null).Should().HaveNoConstraints();
            assertion.Should().NotThrow();
        }

        [TestMethod]
        public void SymbolicValue_HaveNoConstraint_OnEmpty()
        {
            var sv = SymbolicValue.Empty;
            sv.Invoking(x => x.Should().HaveNoConstraints()).Should().NotThrow();
        }

        [TestMethod]
        public void SymbolicValue_HaveNoConstraint_WithConstraint()
        {
            var sv = SymbolicValue.Empty.WithConstraint(BoolConstraint.True);
            sv.Invoking(x => x.Should().HaveNoConstraints()).Should().Throw<AssertFailedException>()
                // * = x or SymbolicValue depending on compilation
                .WithMessage(@"Expected * to have no constraints, but {True} was found.");
        }

        [TestMethod]
        public void SymbolicValue_HaveOnlyConstraints_OnNull()
        {
            var assertion = () => ((SymbolicValue)null).Should().HaveOnlyConstraints(BoolConstraint.True);
            assertion.Should().Throw<AssertFailedException>()
                // * = ((SymbolicValue)null) or SymbolicValue depending on compilation
                .WithMessage(@"* is <null> and can not have constraints {True}.");
        }

        [TestMethod]
        public void SymbolicValue_HaveOnlyConstraints_EmptyConstraints()
        {
            var sv = SymbolicValue.Empty;
            sv.Invoking(x => x.Should().HaveOnlyConstraints(new[] { BoolConstraint.True }, because: "{0} says so", "I")).Should().Throw<AssertFailedException>()
                // * = x or SymbolicValue depending on compilation
                .WithMessage(@"Expected * to have constraints {True} because I says so, but SymbolicValue has no constraints.");
        }

        [TestMethod]
        public void SymbolicValue_HaveOnlyConstraints_Empty()
        {
            var assertion = () => ((SymbolicValue)null).Should().HaveOnlyConstraints();
            assertion.Should().Throw<AssertFailedException>()
                .WithMessage(@"Expected constraints are empty. Use HaveNoConstraints() instead.");
        }

        [TestMethod]
        public void SymbolicValue_HaveOnlyConstraints_ExpectedNull()
        {
            var assertion = () => ((SymbolicValue)null).Should().HaveOnlyConstraints(null, null, null, null);
            assertion.Should().Throw<AssertFailedException>()
                .WithMessage(@"Expected constraints are empty. Use HaveNoConstraints() instead.");
        }

        [TestMethod]
        public void SymbolicValue_HaveOnlyConstraints_OneMissing()
        {
            var sv = SymbolicValue.Empty.WithConstraint(BoolConstraint.True);
            sv.Invoking(x => x.Should().HaveOnlyConstraints(BoolConstraint.True, ObjectConstraint.NotNull)).Should().Throw<AssertFailedException>()
                // * = x or SymbolicValue depending on compilation
                .WithMessage(@"Expected * to have constraints {True, NotNull}, but {NotNull} are missing. Actual are {True}.");
        }

        [TestMethod]
        public void SymbolicValue_HaveOnlyConstraints_OneAdditional()
        {
            var sv = SymbolicValue.Empty.WithConstraint(BoolConstraint.True).WithConstraint(ObjectConstraint.NotNull);
            sv.Invoking(x => x.Should().HaveOnlyConstraints(BoolConstraint.True)).Should().Throw<AssertFailedException>()
                // * = x or SymbolicValue depending on compilation
                .WithMessage(@"Expected * to have constraints {True}, but additional {NotNull} are present. Actual are {NotNull, True}.");
        }

        [TestMethod]
        public void SymbolicValue_HaveOnlyConstraints_MatchSome()
        {
            var sv = SymbolicValue.Empty.WithConstraint(BoolConstraint.True).WithConstraint(ObjectConstraint.NotNull);
            sv.Invoking(x => x.Should().HaveOnlyConstraints(BoolConstraint.True, ObjectConstraint.Null)).Should().Throw<AssertFailedException>()
                // * = x or SymbolicValue depending on compilation
                .WithMessage(@"Expected * to have constraints {True, Null}, but {Null} are missing and additional {NotNull} are present. Actual are {NotNull, True}.");
        }
    }
}
