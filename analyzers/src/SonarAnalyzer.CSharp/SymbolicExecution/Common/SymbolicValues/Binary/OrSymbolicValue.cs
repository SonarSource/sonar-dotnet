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
using System.Linq;
using SonarAnalyzer.SymbolicExecution.Constraints;

namespace SonarAnalyzer.SymbolicExecution.SymbolicValues
{
    public class OrSymbolicValue : BoolBinarySymbolicValue
    {
        public OrSymbolicValue(SymbolicValue leftOperand, SymbolicValue rightOperand) : base(leftOperand, rightOperand) { }

        protected override IEnumerable<ProgramState> TrySetBoolConstraint(BoolConstraint constraint, ProgramState programState) =>
            constraint == BoolConstraint.False
                ? LeftOperand.TrySetConstraint(BoolConstraint.False, programState).SelectMany(ps => RightOperand.TrySetConstraint(BoolConstraint.False, ps))
                : LeftOperand.TrySetConstraint(BoolConstraint.True, programState).SelectMany(ps => RightOperand.TrySetConstraint(BoolConstraint.False, ps))
                    .Union(LeftOperand.TrySetConstraint(BoolConstraint.False, programState).SelectMany(ps => RightOperand.TrySetConstraint(BoolConstraint.True, ps)))
                    .Union(LeftOperand.TrySetConstraint(BoolConstraint.True, programState).SelectMany(ps => RightOperand.TrySetConstraint(BoolConstraint.True, ps)));

        public override string ToString() =>
            LeftOperand + " | " + RightOperand;
    }
}
