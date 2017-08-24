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

using System.Collections.Immutable;
using System.Linq;
using SonarAnalyzer.SymbolicExecution.Relationships;

namespace SonarAnalyzer.SymbolicExecution.Constraints
{
    public static class CollectionConstraintExtensions
    {
        public static ImmutableDictionary<SymbolicValue, SymbolicValueConstraints> AddConstraintForSymbolicValue(
            this ImmutableDictionary<SymbolicValue, SymbolicValueConstraints> constraintMap,
            SymbolicValue symbolicValue, SymbolicValueConstraint constraint)
        {
            var constraints = ImmutableDictionary.GetValueOrDefault(constraintMap, symbolicValue);

            var updatedConstraints = constraints != null
                ? constraints.WithConstraint(constraint)
                : SymbolicValueConstraints.Create(constraint);

            return constraintMap.SetItem(symbolicValue, updatedConstraints);
        }

        public static ImmutableDictionary<SymbolicValue, SymbolicValueConstraints> RemoveConstraintForSymbolicValue(
            this ImmutableDictionary<SymbolicValue, SymbolicValueConstraints> constraintMap,
            SymbolicValue symbolicValue, SymbolicValueConstraint constraint)
        {
            var constraints = ImmutableDictionary.GetValueOrDefault(constraintMap, symbolicValue);

            if (constraints == null)
            {
                return constraintMap;
            }

            var updatedConstraints = constraints.WithoutConstraint(constraint);

            return constraintMap.SetItem(symbolicValue, updatedConstraints);
        }

        public static ImmutableDictionary<SymbolicValue, SymbolicValueConstraints> AddConstraintTo<TRelationship>(
            this ImmutableDictionary<SymbolicValue, SymbolicValueConstraints> constraintsMap,
            SymbolicValue symbolicValue, SymbolicValueConstraint constraint, ProgramState programState)
            where TRelationship : BinaryRelationship
        {
            var newConstraintsMap = constraintsMap;
            var equalSymbols = programState.Relationships
                            .OfType<TRelationship>()
                            .Select(r => GetOtherOperandFromMatchingRelationship(symbolicValue, r))
                            .Where(e => e != null);

            foreach (var equalSymbol in equalSymbols.Where(e => !programState.HasConstraint(e, constraint)))
            {
                newConstraintsMap = newConstraintsMap.AddConstraintForSymbolicValue(equalSymbol, constraint);
            }

            return newConstraintsMap;
        }

        private static SymbolicValue GetOtherOperandFromMatchingRelationship(SymbolicValue symbolicValue,
            BinaryRelationship relationship)
        {
            if (relationship.RightOperand == symbolicValue)
            {
                return relationship.LeftOperand;
            }
            else if (relationship.LeftOperand == symbolicValue)
            {
                return relationship.RightOperand;
            }
            else
            {
                return null;
            }
        }
    }

}
