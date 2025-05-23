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
using SonarAnalyzer.SymbolicExecution.Sonar.Relationships;

namespace SonarAnalyzer.SymbolicExecution.Sonar.SymbolicValues
{
    public class ComparisonSymbolicValue : BoolBinarySymbolicValue
    {
        private readonly SymbolicComparisonKind  comparisonKind;

        public ComparisonSymbolicValue(SymbolicComparisonKind  comparisonKind, SymbolicValue leftOperand, SymbolicValue rightOperand) : base(leftOperand, rightOperand) =>
            this.comparisonKind = comparisonKind;

        protected override IEnumerable<ProgramState> TrySetBoolConstraint(BoolConstraint constraint, ProgramState programState) =>
            programState.TrySetRelationship(GetRelationship(constraint)) is { } newProgramState
            ? new[] { newProgramState }
            : Enumerable.Empty<ProgramState>();

        private BinaryRelationship GetRelationship(BoolConstraint boolConstraint)
        {
            var relationship = new ComparisonRelationship(comparisonKind, LeftOperand, RightOperand);
            return boolConstraint == BoolConstraint.True ? relationship : relationship.Negate();
        }

        public override string ToString() =>
            $"{(comparisonKind == SymbolicComparisonKind.Less ? "<" : "<=")}({LeftOperand}, {RightOperand})";
    }
}
