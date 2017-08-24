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
using SonarAnalyzer.SymbolicExecution.Constraints;
using SonarAnalyzer.SymbolicExecution.Relationships;

namespace SonarAnalyzer.SymbolicExecution.SymbolicValues
{
    public abstract class EqualityLikeSymbolicValue : RelationalSymbolicValue
    {
        protected EqualityLikeSymbolicValue(SymbolicValue leftOperand, SymbolicValue rightOperand)
            : base(leftOperand, rightOperand)
        {
        }

        private BinaryRelationship GetNormalizedRelationship(BoolConstraint boolConstraint, SymbolicValue leftOperand, SymbolicValue rightOperand)
        {
            var invertCount = 0;

            var leftOp = leftOperand;
            var logicalNotLeftOp = leftOp as LogicalNotSymbolicValue;
            while (logicalNotLeftOp != null)
            {
                leftOp = logicalNotLeftOp.Operand;
                logicalNotLeftOp = leftOp as LogicalNotSymbolicValue;
                invertCount++;
            }

            var rightOp = rightOperand;
            var logicalNotRightOp = rightOp as LogicalNotSymbolicValue;
            while (logicalNotRightOp != null)
            {
                rightOp = logicalNotRightOp.Operand;
                logicalNotRightOp = rightOp as LogicalNotSymbolicValue;
                invertCount++;
            }

            var relationship = GetRelationship(boolConstraint, leftOp, rightOp);

            return invertCount % 2 == 0
                ? relationship
                : relationship.Negate();
        }

        protected abstract BinaryRelationship GetRelationship(SymbolicValue left, SymbolicValue right);

        private BinaryRelationship GetRelationship(BoolConstraint boolConstraint, SymbolicValue left, SymbolicValue right)
        {
            var equalsRelationship = GetRelationship(left, right);

            return boolConstraint == BoolConstraint.True
                ? equalsRelationship
                : equalsRelationship.Negate();
        }

        private BinaryRelationship GetRelationship(BoolConstraint boolConstraint)
        {
            return GetNormalizedRelationship(boolConstraint, LeftOperand, RightOperand);
        }

        public override IEnumerable<ProgramState> TrySetConstraint(SymbolicValueConstraint constraint, ProgramState currentProgramState)
        {
            var boolConstraint = constraint as BoolConstraint;
            if (boolConstraint == null)
            {
                return new[] { currentProgramState };
            }

            SymbolicValueConstraints oldConstraints;
            BoolConstraint oldBoolConstraint = null;
            if (TryGetConstraints(currentProgramState, out oldConstraints))
            {
                oldBoolConstraint = oldConstraints.GetConstraintOrDefault<BoolConstraint>();
            }

            if (oldBoolConstraint != null /* could also be ObjectConstraint.NotNull, which can be overridden */ &&
                oldBoolConstraint != boolConstraint)
            {
                return Enumerable.Empty<ProgramState>();
            }

            SymbolicValueConstraints leftConstraints;
            var leftHasConstraint = LeftOperand.TryGetConstraints(currentProgramState, out leftConstraints);
            SymbolicValueConstraints rightConstraints;
            var rightHasConstraint = RightOperand.TryGetConstraints(currentProgramState, out rightConstraints);

            var relationship = GetRelationship(boolConstraint);

            var newProgramState = currentProgramState.TrySetRelationship(relationship);
            if (newProgramState == null)
            {
                return Enumerable.Empty<ProgramState>();
            }

            if (!rightHasConstraint && !leftHasConstraint)
            {
                return new[] { newProgramState };
            }

            return SetConstraint(boolConstraint, leftConstraints, rightConstraints, newProgramState);
        }

        internal abstract IEnumerable<ProgramState> SetConstraint(BoolConstraint boolConstraint,
            SymbolicValueConstraints leftConstraints, SymbolicValueConstraints rightConstraints,
            ProgramState programState);
    }
}
