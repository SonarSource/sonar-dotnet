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

using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using SonarAnalyzer.SymbolicExecution.Constraints;
using Xunit;

namespace SonarAnalyzer.SymbolicExecution.SymbolicValues
{
    public class SymbolicValue_TrySetConstraint
    {
        [Theory]
        [MemberData(nameof(TrueConstraintData))]
        [MemberData(nameof(FalseConstraintData))]
        [MemberData(nameof(NullConstraintData))]
        [MemberData(nameof(NotNullConstraintData))]
        [MemberData(nameof(NoValueConstraintData))]
        [MemberData(nameof(HasValueConstraintData))]
        public void TrySetConstraint(SymbolicValueConstraint constraint,
            IList<SymbolicValueConstraint> existingConstraints,
            IList<IList<SymbolicValueConstraint>> expectedConstraintsPerProgramState)
        {
            // Arrange
            var sv = new SymbolicValue();
            var ps = SetupProgramState(sv, existingConstraints);

            // Act
            var programStates = sv.TrySetConstraint(constraint, ps).ToList();

            // Assert
            programStates.Should().HaveCount(expectedConstraintsPerProgramState.Count);

            for (int i = 0; i < programStates.Count; i++)
            {
                var programState = programStates[i];
                var expectedConstraints = expectedConstraintsPerProgramState[i];

                foreach (var expectedConstraint in expectedConstraints)
                {
                    programState.HasConstraint(sv, expectedConstraint).Should().BeTrue(
                        $"{expectedConstraint} should be present in returned ProgramState.");
                }
            }
        }

        public static IEnumerable<object[]> TrueConstraintData = new[]
        {
            new object[]
            {
                BoolConstraint.True, // constraint to set
                ConstraintList(), // existing
                ProgramStateList(ConstraintList(BoolConstraint.True)) // Expected
            },
            new object[]
            {
                BoolConstraint.True, // constraint to set
                ConstraintList(BoolConstraint.True), // existing
                ProgramStateList(ConstraintList(BoolConstraint.True)) // Expected
            },
            new object[]
            {
                BoolConstraint.True, // constraint to set
                ConstraintList(BoolConstraint.False), // existing
                ProgramStateList() // Expected
            },
            new object[]
            {
                BoolConstraint.True, // constraint to set
                ConstraintList(ObjectConstraint.Null), // existing
                ProgramStateList() // Expected
            },
            new object[]
            {
                BoolConstraint.True, // constraint to set
                ConstraintList(ObjectConstraint.NotNull), // existing
                ProgramStateList(ConstraintList(BoolConstraint.True, ObjectConstraint.NotNull)) // Expected
            },
        };

        public static IEnumerable<object[]> FalseConstraintData = new[]
        {
            new object[]
            {
                BoolConstraint.False, // constraint to set
                ConstraintList(), // existing
                ProgramStateList(ConstraintList(BoolConstraint.False)) // Expected
            },
            new object[]
            {
                BoolConstraint.False, // constraint to set
                ConstraintList(BoolConstraint.True), // existing
                ProgramStateList() // Expected
            },
            new object[]
            {
                BoolConstraint.False, // constraint to set
                ConstraintList(BoolConstraint.False), // existing
                ProgramStateList(ConstraintList(BoolConstraint.False)) // Expected
            },
            new object[]
            {
                BoolConstraint.False, // constraint to set
                ConstraintList(ObjectConstraint.Null), // existing
                ProgramStateList() // Expected
            },
            new object[]
            {
                BoolConstraint.False, // constraint to set
                ConstraintList(ObjectConstraint.NotNull), // existing
                ProgramStateList(ConstraintList(BoolConstraint.False, ObjectConstraint.NotNull)) // Expected
            },
        };

        public static IEnumerable<object[]> NullConstraintData = new[]
        {
            new object[]
            {
                ObjectConstraint.Null, // constraint to set
                ConstraintList(), // existing
                ProgramStateList(ConstraintList(ObjectConstraint.Null)) // Expected
            },
            new object[]
            {
                ObjectConstraint.Null, // constraint to set
                ConstraintList(ObjectConstraint.NotNull), // existing
                ProgramStateList() // Expected
            },
            new object[]
            {
                ObjectConstraint.Null, // constraint to set
                ConstraintList(ObjectConstraint.Null), // existing
                ProgramStateList(ConstraintList(ObjectConstraint.Null)) // Expected
            },
            new object[]
            {
                ObjectConstraint.Null, // constraint to set
                ConstraintList(BoolConstraint.False), // existing
                ProgramStateList() // Expected
            },
            new object[]
            {
                ObjectConstraint.Null, // constraint to set
                ConstraintList(BoolConstraint.True), // existing
                ProgramStateList() // Expected
            },
        };

        public static IEnumerable<object[]> NotNullConstraintData = new[]
        {
            new object[]
            {
                ObjectConstraint.NotNull, // constraint to set
                ConstraintList(), // existing
                ProgramStateList(ConstraintList(ObjectConstraint.NotNull)) // Expected
            },
            new object[]
            {
                ObjectConstraint.NotNull, // constraint to set
                ConstraintList(ObjectConstraint.Null), // existing
                ProgramStateList() // Expected
            },
            new object[]
            {
                ObjectConstraint.NotNull, // constraint to set
                ConstraintList(ObjectConstraint.NotNull), // existing
                ProgramStateList(ConstraintList(ObjectConstraint.NotNull)) // Expected
            },
            new object[]
            {
                ObjectConstraint.NotNull, // constraint to set
                ConstraintList(BoolConstraint.False), // existing
                ProgramStateList(ConstraintList(ObjectConstraint.NotNull, BoolConstraint.False)) // Expected
            },
            new object[]
            {
                ObjectConstraint.NotNull, // constraint to set
                ConstraintList(BoolConstraint.True), // existing
                ProgramStateList(ConstraintList(ObjectConstraint.NotNull, BoolConstraint.True)) // Expected
            },
        };

        public static IEnumerable<object[]> NoValueConstraintData = new[]
        {
            new object[]
            {
                NullableValueConstraint.NoValue, // constraint to set
                ConstraintList(), // existing
                ProgramStateList(ConstraintList()) // Expected, NoValue cannot be applied on non-NullableSymbolicValue
            },
            new object[]
            {
                NullableValueConstraint.NoValue, // constraint to set
                ConstraintList(ObjectConstraint.Null), // existing
                ProgramStateList(ConstraintList(ObjectConstraint.Null)) // Expected, NoValue cannot be applied on non-NullableSymbolicValue
            },
        };

        public static IEnumerable<object[]> HasValueConstraintData = new[]
        {
            new object[]
            {
                NullableValueConstraint.HasValue, // constraint to set
                ConstraintList(), // existing
                ProgramStateList(ConstraintList()) // Expected, HasValue cannot be applied on non-NullableSymbolicValue
            },
            new object[]
            {
                NullableValueConstraint.HasValue, // constraint to set
                ConstraintList(ObjectConstraint.Null), // existing
                ProgramStateList(ConstraintList(ObjectConstraint.Null)) // Expected, HasValue cannot be applied on non-NullableSymbolicValue
            },
        };

        private static IList<IList<SymbolicValueConstraint>> ProgramStateList(params IList<SymbolicValueConstraint>[] programStates)
        {
            return programStates;
        }

        private static IList<SymbolicValueConstraint> ConstraintList(params SymbolicValueConstraint[] constraints)
        {
            return constraints;
        }

        private ProgramState SetupProgramState(SymbolicValue sv, IEnumerable<SymbolicValueConstraint> constraints)
        {
            return constraints.Aggregate(new ProgramState(), 
                (ps, constraint) => ps.SetConstraint(sv, constraint));
        }
    }
}
