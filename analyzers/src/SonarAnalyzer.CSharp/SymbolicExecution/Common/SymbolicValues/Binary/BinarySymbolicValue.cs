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

namespace SonarAnalyzer.SymbolicExecution.SymbolicValues
{
    public class BinarySymbolicValue : SymbolicValue
    {
        // NestedSize:  8, Created ProgramStates:   8, TrySetConstraint visits:    11, Time:  0.03s
        // NestedSize: 16, Created ProgramStates: 128, TrySetConstraint visits:   247, Time:  0.2s
        // NestedSize: 24, Created ProgramStates: 256, TrySetConstraint visits:  4083, Time:  3.0s
        // NestedSize: 32, Created ProgramStates: 256, TrySetConstraint visits: 65519, Time: 46.2s
        private const int NestedSizeLimit = 16;

        public SymbolicValue LeftOperand { get; }
        public SymbolicValue RightOperand { get; }

        public BinarySymbolicValue(SymbolicValue leftOperand, SymbolicValue rightOperand)
        {
            LeftOperand = leftOperand;
            RightOperand = rightOperand;
        }

        protected void ThrowIfTooNested()
        {
            if (NestedSize() > NestedSizeLimit)
            {
                throw new TooManyInternalStatesException();
            }
        }

        private int NestedSize() =>
            (LeftOperand is BinarySymbolicValue leftBinary ? leftBinary.NestedSize() : 1)
            + (RightOperand is BinarySymbolicValue rightBinary ? rightBinary.NestedSize() : 1);
    }
}
