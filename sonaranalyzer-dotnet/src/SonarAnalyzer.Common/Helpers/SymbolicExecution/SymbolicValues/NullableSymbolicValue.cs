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
        public SymbolicValue WrappedValue { get; }

        public NullableSymbolicValue(SymbolicValue wrappedValue)
        {
            this.WrappedValue = wrappedValue;
        }

        public override IEnumerable<ProgramState> TrySetConstraint(SymbolicValueConstraint constraint,
            ProgramState currentProgramState)
        {
            var oldConstraint = currentProgramState.Constraints.GetValueOrDefault(this);

            if (constraint is ObjectConstraint)
            {
                if (oldConstraint == constraint.OppositeForLogicalNot)
                {
                    return Enumerable.Empty<ProgramState>();
                }
                if (constraint == ObjectConstraint.Null)
                {
                    return TrySetConstraint(OptionalConstraint.None, currentProgramState);
                }
                if (constraint == ObjectConstraint.NotNull)
                {
                    return TrySetConstraint(OptionalConstraint.Some, currentProgramState);
                }
            }

            if (constraint is OptionalConstraint)
            {
                return new[] { SetConstraint(constraint, currentProgramState) };
            }

            return WrappedValue.TrySetConstraint(constraint, SetConstraint(OptionalConstraint.Some, currentProgramState));
        }

        public override string ToString()
        {
            if (base.identifier != null)
            {
                return $"NULLABLE_SV_{base.identifier}";
            }

            return this.WrappedValue?.ToString() ?? base.ToString();
        }
    }
}