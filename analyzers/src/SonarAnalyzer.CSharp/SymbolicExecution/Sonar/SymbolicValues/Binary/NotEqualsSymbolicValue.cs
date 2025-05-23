﻿/*
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
    public abstract class NotEqualsSymbolicValue : EqualityLikeSymbolicValue
    {
        protected NotEqualsSymbolicValue(SymbolicValue leftOperand, SymbolicValue rightOperand)
            : base(leftOperand, rightOperand)
        {
        }

        internal override IEnumerable<ProgramState> SetConstraint(BoolConstraint boolConstraint,
            SymbolicValueConstraints leftConstraints, SymbolicValueConstraints rightConstraints,
            ProgramState programState)
        {
            if (boolConstraint == BoolConstraint.False)
            {
                return RightOperand.TrySetConstraints(leftConstraints, programState)
                    .SelectMany(ps => LeftOperand.TrySetConstraints(rightConstraints, ps));
            }

            return RightOperand.TrySetOppositeConstraints(leftConstraints, programState)
                .SelectMany(ps => LeftOperand.TrySetOppositeConstraints(rightConstraints, ps));
        }
    }
}
