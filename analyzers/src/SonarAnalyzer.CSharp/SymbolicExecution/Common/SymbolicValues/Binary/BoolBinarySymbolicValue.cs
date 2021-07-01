/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2021 SonarSource SA
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
using SonarAnalyzer.SymbolicExecution.Constraints;

namespace SonarAnalyzer.SymbolicExecution.SymbolicValues
{
    public abstract class BoolBinarySymbolicValue : BinarySymbolicValue
    {
        // NestedSize:  4, Created ProgramStates:   8, TrySetConstraint visits:    11, Time:  0.03s
        // NestedSize:  8, Created ProgramStates: 128, TrySetConstraint visits:   247, Time:  0.2s
        // NestedSize: 12, Created ProgramStates: 256, TrySetConstraint visits:  4083, Time:  3.0s
        // NestedSize: 16, Created ProgramStates: 256, TrySetConstraint visits: 65519, Time: 46.2s
        private const int NestedSizeLimit = 8;

        protected abstract IEnumerable<ProgramState> TrySetBoolConstraint(BoolConstraint constraint, ProgramState programState);

        protected BoolBinarySymbolicValue(SymbolicValue leftOperand, SymbolicValue rightOperand) : base(leftOperand, rightOperand) { }

        public sealed override IEnumerable<ProgramState> TrySetConstraint(SymbolicValueConstraint constraint, ProgramState programState)
        {
            if (constraint is BoolConstraint boolConstraint)
            {
                return NestedSize() > NestedSizeLimit
                    ? throw new TooManyInternalStatesException()
                    : ThrowIfTooMany(TrySetBoolConstraint(boolConstraint, programState));
            }
            else
            {
                return new[] { programState };
            }
        }

        private int NestedSize() =>
            (LeftOperand is BoolBinarySymbolicValue left ? left.NestedSize() : 1)
            + (RightOperand is BoolBinarySymbolicValue right ? right.NestedSize() : 1);
    }
}
