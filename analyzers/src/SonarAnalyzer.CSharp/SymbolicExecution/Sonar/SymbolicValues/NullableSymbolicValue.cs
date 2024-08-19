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
using SonarAnalyzer.SymbolicExecution.Sonar.Constraints;

namespace SonarAnalyzer.SymbolicExecution.Sonar.SymbolicValues
{
    public class NullableSymbolicValue : SymbolicValue
    {
        public SymbolicValue WrappedValue { get; }

        public NullableSymbolicValue(SymbolicValue wrappedValue)
        {
            WrappedValue = wrappedValue;
        }

        public override IEnumerable<ProgramState> TrySetConstraint(SymbolicConstraint constraint, ProgramState programState)
        {
            if (constraint == null)
            {
                return new[] { programState };
            }

            if (constraint is ObjectConstraint)
            {
                var optionalConstraint = constraint == ObjectConstraint.Null
                    ? NullableConstraint.NoValue
                    : NullableConstraint.HasValue;

                return TrySetConstraint(optionalConstraint, programState);
            }

            var oldConstraint = ((IDictionary<SymbolicValue, SymbolicValueConstraints>)programState.Constraints).GetValueOrDefault(this)?.GetConstraintOrDefault<NullableConstraint>();
            if (constraint is NullableConstraint)
            {
                if (oldConstraint == null)
                {
                    return new[] { programState.SetConstraint(this, constraint) };
                }

                if (oldConstraint != constraint)
                {
                    return Enumerable.Empty<ProgramState>();
                }

                return new[] { programState };
            }

            return TrySetConstraint(NullableConstraint.HasValue, programState).SelectMany(ps => WrappedValue.TrySetConstraint(constraint, ps));
        }

        public override IEnumerable<ProgramState> TrySetOppositeConstraint(SymbolicConstraint constraint, ProgramState programState)
        {
            var negateConstraint = constraint?.Opposite;

            return constraint is BoolConstraint
                ? TrySetConstraint(negateConstraint, programState).Union(TrySetConstraint(NullableConstraint.NoValue, programState))
                : TrySetConstraint(negateConstraint, programState);
        }

        public override string ToString() => $"NULLABLE_SV_{identifier}" ;
    }
}
