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
    public class SymbolicValue_TrySetConstraint
    {
        private static IEnumerable<object[]> TrueConstraintData { get; } = new[]
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
                ProgramStateList(ConstraintList(ObjectConstraint.Null)) // Expected
            },
            new object[]
            {
                BoolConstraint.True, // constraint to set
                ConstraintList(ObjectConstraint.NotNull), // existing
                ProgramStateList(ConstraintList(BoolConstraint.True, ObjectConstraint.NotNull)) // Expected
            },
        };

        private static IEnumerable<object[]> FalseConstraintData { get; } = new[]
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
                ProgramStateList(ConstraintList(ObjectConstraint.Null)) // Expected
            },
            new object[]
            {
                BoolConstraint.False, // constraint to set
                ConstraintList(ObjectConstraint.NotNull), // existing
                ProgramStateList(ConstraintList(BoolConstraint.False, ObjectConstraint.NotNull)) // Expected
            },
        };

        private static IEnumerable<object[]> NullConstraintData { get; } = new[]
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
            new object[]
            {
                ObjectConstraint.Null, // constraint to set
                ConstraintList(StringConstraint.EmptyString), // existing
                ProgramStateList() // Expected
            },
            new object[]
            {
                ObjectConstraint.Null, // constraint to set
                ConstraintList(StringConstraint.FullString), // existing
                ProgramStateList() // Expected
            },
            new object[]
            {
                ObjectConstraint.Null, // constraint to set
                ConstraintList(StringConstraint.WhiteSpaceString), // existing
                ProgramStateList() // Expected
            },
            new object[]
            {
                ObjectConstraint.Null, // constraint to set
                ConstraintList(StringConstraint.FullNotWhiteSpaceString), // existing
                ProgramStateList() // Expected
            },
        };

        private static IEnumerable<object[]> NotNullConstraintData { get; } = new[]
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
            new object[]
            {
                ObjectConstraint.NotNull, // constraint to set
                ConstraintList(StringConstraint.EmptyString), // existing
                ProgramStateList(ConstraintList(ObjectConstraint.NotNull, StringConstraint.EmptyString)) // Expected
            },
            new object[]
            {
                ObjectConstraint.NotNull, // constraint to set
                ConstraintList(StringConstraint.FullString), // existing
                ProgramStateList(ConstraintList(ObjectConstraint.NotNull, StringConstraint.FullString)) // Expected
            },
            new object[]
            {
                ObjectConstraint.NotNull, // constraint to set
                ConstraintList(StringConstraint.WhiteSpaceString), // existing
                ProgramStateList(ConstraintList(ObjectConstraint.NotNull, StringConstraint.WhiteSpaceString)) // Expected
            },
            new object[]
            {
                ObjectConstraint.NotNull, // constraint to set
                ConstraintList(StringConstraint.FullNotWhiteSpaceString), // existing
                ProgramStateList(ConstraintList(ObjectConstraint.NotNull, StringConstraint.FullNotWhiteSpaceString)) // Expected
            },
        };

        private static IEnumerable<object[]> NoValueConstraintData { get; } = new[]
        {
            new object[]
            {
                NullableConstraint.NoValue, // constraint to set
                ConstraintList(), // existing
                ProgramStateList(ConstraintList()) // Expected, NoValue cannot be applied on non-NullableSymbolicValue
            },
            new object[]
            {
                NullableConstraint.NoValue, // constraint to set
                ConstraintList(ObjectConstraint.Null), // existing
                ProgramStateList(ConstraintList(ObjectConstraint.Null)) // Expected, NoValue cannot be applied on non-NullableSymbolicValue
            },
        };

        private static IEnumerable<object[]> HasValueConstraintData { get; } = new[]
        {
            new object[]
            {
                NullableConstraint.HasValue, // constraint to set
                ConstraintList(), // existing
                ProgramStateList(ConstraintList()) // Expected, HasValue cannot be applied on non-NullableSymbolicValue
            },
            new object[]
            {
                NullableConstraint.HasValue, // constraint to set
                ConstraintList(ObjectConstraint.Null), // existing
                ProgramStateList(ConstraintList(ObjectConstraint.Null)) // Expected, HasValue cannot be applied on non-NullableSymbolicValue
            },
        };

        private static IEnumerable<object[]> EmptyStringConstraintData { get; } = new[]
        {
            new object[]
            {
                StringConstraint.EmptyString, // constraint to set
                ConstraintList(), // existing
                ProgramStateList(ConstraintList(StringConstraint.EmptyString, ObjectConstraint.NotNull)) // Expected
            },
            new object[]
            {
                StringConstraint.EmptyString, // constraint to set
                ConstraintList(StringConstraint.EmptyString), // existing
                ProgramStateList(ConstraintList(StringConstraint.EmptyString)) // Expected
            },
            new object[]
            {
                StringConstraint.EmptyString, // constraint to set
                ConstraintList(StringConstraint.FullString), // existing
                ProgramStateList() // Expected
            },
            new object[]
            {
                StringConstraint.EmptyString, // constraint to set
                ConstraintList(ObjectConstraint.Null), // existing
                ProgramStateList() // Expected
            },
            new object[]
            {
                StringConstraint.EmptyString, // constraint to set
                ConstraintList(ObjectConstraint.NotNull), // existing
                ProgramStateList(ConstraintList(ObjectConstraint.NotNull, StringConstraint.EmptyString)) // Expected
            },
            new object[]
            {
                StringConstraint.EmptyString, // constraint to set
                ConstraintList(StringConstraint.WhiteSpaceString), // existing
                ProgramStateList() // Expected
            },
            new object[]
            {
                StringConstraint.EmptyString, // constraint to set
                ConstraintList(StringConstraint.FullNotWhiteSpaceString), // existing
                ProgramStateList() // Expected
            },
        };

        private static IEnumerable<object[]> FullStringConstraintData { get; } = new[]
        {
            new object[]
            {
                StringConstraint.FullString, // constraint to set
                ConstraintList(), // existing
                ProgramStateList(ConstraintList(StringConstraint.FullString, ObjectConstraint.NotNull)) // Expected
            },
            new object[]
            {
                StringConstraint.FullString, // constraint to set
                ConstraintList(StringConstraint.EmptyString), // existing
                ProgramStateList() // Expected
            },
            new object[]
            {
                StringConstraint.FullString, // constraint to set
                ConstraintList(StringConstraint.FullString), // existing
                ProgramStateList(ConstraintList(StringConstraint.FullString)) // Expected
            },
            new object[]
            {
                StringConstraint.FullString, // constraint to set
                ConstraintList(ObjectConstraint.Null), // existing
                ProgramStateList() // Expected
            },
            new object[]
            {
                StringConstraint.FullString, // constraint to set
                ConstraintList(ObjectConstraint.NotNull), // existing
                ProgramStateList(ConstraintList(ObjectConstraint.NotNull, StringConstraint.FullString)) // Expected
            },
            new object[]
            {
                StringConstraint.FullString, // constraint to set
                ConstraintList(StringConstraint.WhiteSpaceString), // existing
                ProgramStateList(ConstraintList(StringConstraint.WhiteSpaceString)) // Expected
            },
            new object[]
            {
                StringConstraint.FullString, // constraint to set
                ConstraintList(StringConstraint.FullNotWhiteSpaceString), // existing
                ProgramStateList(ConstraintList(StringConstraint.FullNotWhiteSpaceString)) // Expected
            },
        };

        private static IEnumerable<object[]> FullNotWhiteSpaceStringConstraintData { get; } = new[]
        {
            new object[]
            {
                StringConstraint.FullNotWhiteSpaceString, // constraint to set
                ConstraintList(), // existing
                ProgramStateList(ConstraintList(StringConstraint.FullNotWhiteSpaceString, ObjectConstraint.NotNull)) // Expected
            },
            new object[]
            {
                StringConstraint.FullNotWhiteSpaceString, // constraint to set
                ConstraintList(StringConstraint.EmptyString), // existing
                ProgramStateList() // Expected
            },
            new object[]
            {
                StringConstraint.FullNotWhiteSpaceString, // constraint to set
                ConstraintList(StringConstraint.FullString), // existing
                ProgramStateList(ConstraintList(StringConstraint.FullNotWhiteSpaceString)) // Expected
            },
            new object[]
            {
                StringConstraint.FullNotWhiteSpaceString, // constraint to set
                ConstraintList(ObjectConstraint.Null), // existing
                ProgramStateList() // Expected
            },
            new object[]
            {
                StringConstraint.FullNotWhiteSpaceString, // constraint to set
                ConstraintList(ObjectConstraint.NotNull), // existing
                ProgramStateList(ConstraintList(ObjectConstraint.NotNull, StringConstraint.FullNotWhiteSpaceString)) // Expected
            },
            new object[]
            {
                StringConstraint.FullNotWhiteSpaceString, // constraint to set
                ConstraintList(StringConstraint.WhiteSpaceString), // existing
                ProgramStateList() // Expected
            },
            new object[]
            {
                StringConstraint.FullNotWhiteSpaceString, // constraint to set
                ConstraintList(StringConstraint.FullNotWhiteSpaceString), // existing
                ProgramStateList(ConstraintList(StringConstraint.FullNotWhiteSpaceString)) // Expected
            },
        };

        private static IEnumerable<object[]> WhiteSpaceStringConstraintData { get; } = new[]
        {
            new object[]
            {
                StringConstraint.WhiteSpaceString, // constraint to set
                ConstraintList(), // existing
                ProgramStateList(ConstraintList(StringConstraint.WhiteSpaceString, ObjectConstraint.NotNull)) // Expected
            },
            new object[]
            {
                StringConstraint.WhiteSpaceString, // constraint to set
                ConstraintList(StringConstraint.EmptyString), // existing
                ProgramStateList() // Expected
            },
            new object[]
            {
                StringConstraint.WhiteSpaceString, // constraint to set
                ConstraintList(StringConstraint.FullString), // existing
                ProgramStateList(ConstraintList(StringConstraint.WhiteSpaceString)) // Expected
            },
            new object[]
            {
                StringConstraint.WhiteSpaceString, // constraint to set
                ConstraintList(ObjectConstraint.Null), // existing
                ProgramStateList() // Expected
            },
            new object[]
            {
                StringConstraint.WhiteSpaceString, // constraint to set
                ConstraintList(ObjectConstraint.NotNull), // existing
                ProgramStateList(ConstraintList(ObjectConstraint.NotNull, StringConstraint.WhiteSpaceString)) // Expected
            },
            new object[]
            {
                StringConstraint.WhiteSpaceString, // constraint to set
                ConstraintList(StringConstraint.WhiteSpaceString), // existing
                ProgramStateList(ConstraintList(StringConstraint.WhiteSpaceString)) // Expected
            },
            new object[]
            {
                StringConstraint.WhiteSpaceString, // constraint to set
                ConstraintList(StringConstraint.FullNotWhiteSpaceString), // existing
                ProgramStateList() // Expected
            },
        };

        private static IEnumerable<object[]> FullOrNullStringConstraintData { get; } = new[]
        {
            new object[]
            {
                StringConstraint.FullOrNullString, // constraint to set
                ConstraintList(), // existing
                ProgramStateList(ConstraintList()) // Expected
            },
            new object[]
            {
                StringConstraint.FullOrNullString, // constraint to set
                ConstraintList(StringConstraint.EmptyString), // existing
                ProgramStateList() // Expected
            },
            new object[]
            {
                StringConstraint.FullOrNullString, // constraint to set
                ConstraintList(StringConstraint.FullString), // existing
                ProgramStateList(ConstraintList(StringConstraint.FullString)) // Expected
            },
            new object[]
            {
                StringConstraint.FullOrNullString, // constraint to set
                ConstraintList(ObjectConstraint.Null), // existing
                ProgramStateList(ConstraintList(ObjectConstraint.Null)) // Expected
            },
            new object[]
            {
                StringConstraint.FullOrNullString, // constraint to set
                ConstraintList(ObjectConstraint.NotNull), // existing
                ProgramStateList(ConstraintList(ObjectConstraint.NotNull, StringConstraint.FullString)) // Expected
            },
            new object[]
            {
                StringConstraint.FullOrNullString, // constraint to set
                ConstraintList(StringConstraint.WhiteSpaceString), // existing
                ProgramStateList(ConstraintList(StringConstraint.WhiteSpaceString)) // Expected
            },
            new object[]
            {
                StringConstraint.FullOrNullString, // constraint to set
                ConstraintList(StringConstraint.FullNotWhiteSpaceString), // existing
                ProgramStateList(ConstraintList(StringConstraint.FullNotWhiteSpaceString)) // Expected
            },
        };

        private static IEnumerable<object[]> NotWhiteSpaceStringConstraintData { get; } = new[]
        {
            new object[]
            {
                StringConstraint.NotWhiteSpaceString, // constraint to set
                ConstraintList(), // existing
                ProgramStateList(ConstraintList()) // Expected
            },
            new object[]
            {
                StringConstraint.NotWhiteSpaceString, // constraint to set
                ConstraintList(StringConstraint.EmptyString), // existing
                ProgramStateList(ConstraintList(StringConstraint.EmptyString)) // Expected
            },
            new object[]
            {
                StringConstraint.NotWhiteSpaceString, // constraint to set
                ConstraintList(StringConstraint.FullString), // existing
                ProgramStateList(ConstraintList(StringConstraint.FullString)) // Expected
            },
            new object[]
            {
                StringConstraint.NotWhiteSpaceString, // constraint to set
                ConstraintList(ObjectConstraint.Null), // existing
                ProgramStateList(ConstraintList(ObjectConstraint.Null)) // Expected
            },
            new object[]
            {
                StringConstraint.NotWhiteSpaceString, // constraint to set
                ConstraintList(ObjectConstraint.NotNull), // existing
                ProgramStateList(ConstraintList(ObjectConstraint.NotNull, StringConstraint.FullNotWhiteSpaceString)) // Expected
            },
            new object[]
            {
                StringConstraint.NotWhiteSpaceString, // constraint to set
                ConstraintList(StringConstraint.WhiteSpaceString), // existing
                ProgramStateList() // Expected
            },
            new object[]
            {
                StringConstraint.NotWhiteSpaceString, // constraint to set
                ConstraintList(StringConstraint.FullNotWhiteSpaceString), // existing
                ProgramStateList(ConstraintList(StringConstraint.FullNotWhiteSpaceString)) // Expected
            },
        };

        private static IEnumerable<object[]> ByteCollectionConstraintData { get; } = new[]
        {
            new object[]
            {
                ByteCollectionConstraint.CryptographicallyWeak, // constraint to set
                ConstraintList(ByteCollectionConstraint.CryptographicallyStrong), // existing
                ProgramStateList(ConstraintList(ByteCollectionConstraint.CryptographicallyWeak)) // Expected
            },
            new object[]
            {
                ByteCollectionConstraint.CryptographicallyStrong, // constraint to set
                ConstraintList(ByteCollectionConstraint.CryptographicallyWeak), // existing
                ProgramStateList(ConstraintList(ByteCollectionConstraint.CryptographicallyStrong)) // Expected
            }
        };

        private static IEnumerable<object[]> InitializationVectorConstraintData { get; } = new[]
        {
            new object[]
            {
                InitializationVectorConstraint.Initialized, // constraint to set
                ConstraintList(InitializationVectorConstraint.NotInitialized), // existing
                ProgramStateList(ConstraintList(InitializationVectorConstraint.Initialized)) // Expected
            },
            new object[]
            {
                InitializationVectorConstraint.NotInitialized, // constraint to set
                ConstraintList(InitializationVectorConstraint.Initialized), // existing
                ProgramStateList(ConstraintList(InitializationVectorConstraint.NotInitialized)) // Expected
            }
        };

        [TestMethod]
        [DynamicData(nameof(TrueConstraintData))]
        [DynamicData(nameof(FalseConstraintData))]
        [DynamicData(nameof(NullConstraintData))]
        [DynamicData(nameof(NotNullConstraintData))]
        [DynamicData(nameof(NoValueConstraintData))]
        [DynamicData(nameof(HasValueConstraintData))]
        [DynamicData(nameof(EmptyStringConstraintData))]
        [DynamicData(nameof(FullStringConstraintData))]
        [DynamicData(nameof(FullOrNullStringConstraintData))]
        [DynamicData(nameof(WhiteSpaceStringConstraintData))]
        [DynamicData(nameof(FullNotWhiteSpaceStringConstraintData))]
        [DynamicData(nameof(NotWhiteSpaceStringConstraintData))]
        [DynamicData(nameof(ByteCollectionConstraintData))]
        [DynamicData(nameof(InitializationVectorConstraintData))]
        public void TrySetConstraint(SymbolicConstraint constraint,
                                     IList<SymbolicConstraint> existingConstraints,
                                     IList<IList<SymbolicConstraint>> expectedConstraintsPerProgramState)
        {
            // Arrange
            var sv = new SymbolicValue();
            var ps = SetupProgramState(sv, existingConstraints);

            // Act
            var programStates = sv.TrySetConstraint(constraint, ps).ToList();

            // Assert
            programStates.Should().HaveCount(expectedConstraintsPerProgramState.Count);

            for (var i = 0; i < programStates.Count; i++)
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

        [TestMethod]
        public void WithNonBoolConstraint_ForBoolBinarySymbolicValue_ReturnsInputProgramState()
        {
            var sv = new AndSymbolicValue(SymbolicValue.True, SymbolicValue.False);
            var inputProgramState = SetupProgramState(sv, new[] { BoolConstraint.True });
            var newProgramStates = sv.TrySetConstraint(ObjectConstraint.NotNull, inputProgramState);

            newProgramStates.Should().ContainSingle().And.Equal(inputProgramState);
        }

        private static IList<IList<SymbolicConstraint>> ProgramStateList(params IList<SymbolicConstraint>[] programStates) => programStates;

        private static IList<SymbolicConstraint> ConstraintList(params SymbolicConstraint[] constraints) => constraints;

        private static ProgramState SetupProgramState(SymbolicValue sv, IEnumerable<SymbolicConstraint> constraints) =>
            constraints.Aggregate(new ProgramState(), (ps, constraint) => ps.SetConstraint(sv, constraint));
    }
}
