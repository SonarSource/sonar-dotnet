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
using System.Collections.Immutable;
using System.Linq;

namespace SonarAnalyzer.Helpers.FlowAnalysis.Common
{
    public class NullableSymbolicValue : SymbolicValue
    {
        private SymbolicValue wrappedValue { get; }

        public NullableSymbolicValue(SymbolicValue wrappedValue)
        {
            this.wrappedValue = wrappedValue;
        }

        public override IEnumerable<ProgramState> TrySetConstraint(SymbolicValueConstraint constraint, ProgramState currentProgramState)
        {
            var oldConstraint = currentProgramState.Constraints.GetValueOrDefault(this);

            if (constraint == ObjectConstraint.Null)
            {
                if (oldConstraint == null)
                {
                    return new[] { SetConstraint(OptionalConstraint.None, currentProgramState) };
                }
                else if (oldConstraint == OptionalConstraint.None)
                {
                    return new[] { currentProgramState };
                }
                else if (oldConstraint == OptionalConstraint.Some)
                {
                    return Enumerable.Empty<ProgramState>();
                }
                else
                {
                    // TODO: What shall we do?
                }
            }
            else if (constraint == ObjectConstraint.NotNull)
            {
                if (oldConstraint == null)
                {
                    return new[] { SetConstraint(OptionalConstraint.Some, currentProgramState) };
                }
                else if (oldConstraint == OptionalConstraint.None)
                {
                    return Enumerable.Empty<ProgramState>();
                }
                else if (oldConstraint == OptionalConstraint.Some)
                {
                    return new[] { currentProgramState };
                }
                else
                {
                    // TODO: What shall we do?
                }
            }
            else if (constraint == BoolConstraint.True)
            {
                if (oldConstraint == null)
                {
                    var newProgramState = SetConstraint(OptionalConstraint.Some, currentProgramState);
                    //return this.wrappedValue.TrySetConstraint(constraint, newProgramState);
                    return new[] { SetConstraint(constraint, newProgramState) };
                }
                else if (oldConstraint == OptionalConstraint.None)
                {
                    return Enumerable.Empty<ProgramState>();
                }
                else if (oldConstraint == OptionalConstraint.Some)
                {
                    //return this.wrappedValue.TrySetConstraint(constraint, currentProgramState);
                    return new[] { SetConstraint(constraint, currentProgramState) };
                }
                else
                {
                    //return this.wrappedValue.TrySetConstraint(constraint, currentProgramState);
                    return new[] { SetConstraint(constraint, currentProgramState) };
                }
            }
            else if (constraint == BoolConstraint.False)
            {
                if (oldConstraint == null)
                {
                    var newProgramState = SetConstraint(OptionalConstraint.Some, currentProgramState);
                    //return this.wrappedValue.TrySetConstraint(constraint, newProgramState);
                    return new[] { SetConstraint(constraint, newProgramState) };
                    //return new[]
                    //    {
                    //        SetConstraint(constraint, SetConstraint(OptionalConstraint.Some, currentProgramState)),
                    //        SetConstraint(OptionalConstraint.None, currentProgramState)
                    //    };
                }
                else if (oldConstraint == OptionalConstraint.None)
                {
                    return Enumerable.Empty<ProgramState>();
                }
                else if (oldConstraint == OptionalConstraint.Some)
                {
                    //return this.wrappedValue.TrySetConstraint(constraint, currentProgramState);
                    return new[] { SetConstraint(constraint, currentProgramState) };
                }
                else
                {
                    //return this.wrappedValue.TrySetConstraint(constraint, currentProgramState);
                    return new[] { SetConstraint(constraint, currentProgramState) };
                }
            }

            return Enumerable.Empty<ProgramState>();
        }

        public override string ToString()
        {
            if (base.identifier != null)
            {
                return $"NULLABLE_SV_{base.identifier}";
            }

            return this.wrappedValue?.ToString() ?? base.ToString();
        }
    }
}
