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

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using SonarAnalyzer.Helpers;
using SonarAnalyzer.SymbolicExecution.Constraints;
using SonarAnalyzer.SymbolicExecution.Relationships;
using SonarAnalyzer.SymbolicExecution.SymbolicValues;

namespace SonarAnalyzer.SymbolicExecution
{
    public class SymbolicValue
    {
        public static readonly SymbolicValue True = new BoolLiteralSymbolicValue(true);
        public static readonly SymbolicValue False = new BoolLiteralSymbolicValue(false);
        public static readonly SymbolicValue Null = new NullSymbolicValue();
        public static readonly SymbolicValue This = new ThisSymbolicValue();
        public static readonly SymbolicValue Base = new BaseSymbolicValue();

        private class BoolLiteralSymbolicValue : SymbolicValue
        {
            internal BoolLiteralSymbolicValue(bool value)
                : base(value)
            {
            }
        }

        private class ThisSymbolicValue : SymbolicValue
        {
            internal ThisSymbolicValue()
                : base(new object())
            {
            }
            public override string ToString()
            {
                return "SV_THIS";
            }
        }

        private class BaseSymbolicValue : SymbolicValue
        {
            internal BaseSymbolicValue()
                : base(new object())
            {
            }
            public override string ToString()
            {
                return "SV_BASE";
            }
        }

        private class NullSymbolicValue : SymbolicValue
        {
            internal NullSymbolicValue()
                : base(new object())
            {
            }
            public override string ToString()
            {
                return "SV_NULL";
            }
        }

        protected readonly object identifier;

        private static int SymbolicValueCounter = 0;

        internal SymbolicValue()
            : this(SymbolicValueCounter++)
        {
        }

        private SymbolicValue(object identifier)
        {
            this.identifier = identifier;
        }

        internal static SymbolicValue Create(ITypeSymbol type = null)
        {
            if (type != null &&
                type.OriginalDefinition.Is(KnownType.System_Nullable_T))
            {
                return new NullableSymbolicValue(new SymbolicValue());
            }

            return new SymbolicValue();
        }

        public override string ToString()
        {
            if (identifier != null)
            {
                return "SV_" + identifier;
            }

            return base.ToString();
        }

        internal virtual ProgramState SetConstraint(SymbolicValueConstraint constraint, ProgramState programState)
        {
            if (constraint == null)
            {
                return programState;
            }

            var updatedConstraintsMap = AddConstraintForSymbolicValue(this, constraint, programState.Constraints);
            updatedConstraintsMap = AddConstraintTo<EqualsRelationship>(constraint, programState, updatedConstraintsMap);

            if (constraint is BoolConstraint)
            {
                updatedConstraintsMap = AddConstraintTo<NotEqualsRelationship>(constraint.OppositeForLogicalNot, programState, updatedConstraintsMap);
            }

            return new ProgramState(
                programState.Values,
                updatedConstraintsMap,
                programState.ProgramPointVisitCounts,
                programState.ExpressionStack,
                programState.Relationships);
        }

        internal virtual ProgramState RemoveConstraint(SymbolicValueConstraint constraint, ProgramState programState)
        {
            if (constraint == null)
            {
                return programState;
            }

            var updatedConstraintsMap = RemoveConstraintForSymbolicValue(this, constraint, programState.Constraints);

            return new ProgramState(
                programState.Values,
                updatedConstraintsMap,
                programState.ProgramPointVisitCounts,
                programState.ExpressionStack,
                programState.Relationships);
        }

        private ImmutableDictionary<SymbolicValue, SymbolicValueConstraints> AddConstraintForSymbolicValue(SymbolicValue symbolicValue,
            SymbolicValueConstraint constraint, ImmutableDictionary<SymbolicValue, SymbolicValueConstraints> constraintMap)
        {
            var constraints = ImmutableDictionary.GetValueOrDefault(constraintMap, symbolicValue);

            var updatedConstraints = constraints != null
                ? constraints.WithConstraint(constraint)
                : SymbolicValueConstraints.Create(constraint);

            return constraintMap.SetItem(symbolicValue, updatedConstraints);
        }

        private ImmutableDictionary<SymbolicValue, SymbolicValueConstraints> RemoveConstraintForSymbolicValue(SymbolicValue symbolicValue,
            SymbolicValueConstraint constraint, ImmutableDictionary<SymbolicValue, SymbolicValueConstraints> constraintMap)
        {
            var constraints = ImmutableDictionary.GetValueOrDefault(constraintMap, symbolicValue);

            if (constraints == null)
            {
                return constraintMap;
            }

            var updatedConstraints = constraints.WithoutConstraint(constraint);

            return constraintMap.SetItem(symbolicValue, updatedConstraints);
        }

        private ImmutableDictionary<SymbolicValue, SymbolicValueConstraints> AddConstraintTo<TRelationship>(
            SymbolicValueConstraint constraint, ProgramState programState,
            ImmutableDictionary<SymbolicValue, SymbolicValueConstraints> constraintsMap)
            where TRelationship : BinaryRelationship
        {
            var newConstraintsMap = constraintsMap;
            var equalSymbols = programState.Relationships
                            .OfType<TRelationship>()
                            .Select(r => GetOtherOperandFromMatchingRelationship(r))
                            .Where(e => e != null);

            foreach (var equalSymbol in equalSymbols.Where(e => !e.HasConstraint(constraint, programState)))
            {
                newConstraintsMap = AddConstraintForSymbolicValue(equalSymbol, constraint, newConstraintsMap);
            }

            return newConstraintsMap;
        }

        private SymbolicValue GetOtherOperandFromMatchingRelationship(BinaryRelationship relationship)
        {
            if (relationship.RightOperand == this)
            {
                return relationship.LeftOperand;
            }
            else if (relationship.LeftOperand == this)
            {
                return relationship.RightOperand;
            }
            else
            {
                return null;
            }
        }

        public bool HasConstraint(SymbolicValueConstraint constraint, ProgramState programState)
        {
            SymbolicValueConstraints constraints;
            return programState.Constraints.TryGetValue(this, out constraints) &&
                   constraints.HasConstraint(constraint);
        }

        public bool TryGetConstraints(ProgramState programState, out SymbolicValueConstraints constraints)
        {
            return programState.Constraints.TryGetValue(this, out constraints);
        }

        public bool IsNull(ProgramState programState)
        {
            return this.HasConstraint(ObjectConstraint.Null, programState);
        }

        protected IEnumerable<ProgramState> ThrowIfTooMany(IEnumerable<ProgramState> states)
        {
            var stateList = states.ToList();
            if (stateList.Count >= AbstractExplodedGraph.MaxInternalStateCount)
            {
                throw new TooManyInternalStatesException();
            }

            return stateList;
        }

        public virtual IEnumerable<ProgramState> TrySetConstraint(SymbolicValueConstraint constraint,
            ProgramState currentProgramState)
        {
            if (constraint == null)
            {
                return new[] { currentProgramState };
            }

            SymbolicValueConstraints oldConstraints;
            if (!currentProgramState.Constraints.TryGetValue(this, out oldConstraints))
            {
                return new[] { SetConstraint(constraint, currentProgramState) };
            }

            var boolConstraint = constraint as BoolConstraint;
            if (boolConstraint != null)
            {
                return TrySetConstraint(boolConstraint, oldConstraints, currentProgramState);
            }

            var objectConstraint = constraint as ObjectConstraint;
            if (objectConstraint != null)
            {
                return TrySetConstraint(objectConstraint, oldConstraints, currentProgramState);
            }

            if (constraint is NullableValueConstraint ||
                constraint is DisposableConstraint ||
                constraint is CollectionCapacityConstraint)
            {
                return new[] { currentProgramState };
            }

            throw new NotSupportedException($"Neither one of {nameof(BoolConstraint)}, {nameof(ObjectConstraint)}, " +
                $"{nameof(ObjectConstraint)}, {nameof(DisposableConstraint)}, {nameof(CollectionCapacityConstraint)}.");
        }

        public virtual IEnumerable<ProgramState> TrySetOppositeConstraint(SymbolicValueConstraint constraint,
            ProgramState programState)
        {
            return TrySetConstraint(constraint?.OppositeForLogicalNot, programState);
        }

        public IEnumerable<ProgramState> TrySetConstraints(SymbolicValueConstraints constraints,
            ProgramState programState)
        {
            return TrySetConstraints(constraints, programState, false);
        }

        public IEnumerable<ProgramState> TrySetOppositeConstraints(SymbolicValueConstraints constraints,
            ProgramState programState)
        {
            return TrySetConstraints(constraints, programState, true);
        }

        private IEnumerable<ProgramState> TrySetConstraints(SymbolicValueConstraints constraints,
            ProgramState programState, bool isOppositeConstraints)
        {
            IEnumerable<ProgramState> programStates = new [] { programState };

            if (constraints == null)
            {
                return programStates;
            }

            foreach (var constraint in constraints.GetConstraints())
            {
                programStates = programStates.SelectMany(ps =>
                    isOppositeConstraints
                    ? TrySetOppositeConstraint(constraint, ps)
                    : TrySetConstraint(constraint, ps));
            }

            return programStates;
        }

        private IEnumerable<ProgramState> TrySetConstraint(BoolConstraint boolConstraint,
            SymbolicValueConstraints oldConstraints, ProgramState currentProgramState)
        {
            if (oldConstraints.HasConstraint(ObjectConstraint.Null))
            {
                // It was null, and now it should be true or false
                return Enumerable.Empty<ProgramState>();
            }

            var oldBoolConstraint = oldConstraints.GetConstraintOrDefault<BoolConstraint>();
            if (oldBoolConstraint != null &&
                oldBoolConstraint != boolConstraint)
            {
                return Enumerable.Empty<ProgramState>();
            }

            // Either same bool constraint, or previously not null, and now a bool constraint
            return new[] { SetConstraint(boolConstraint, currentProgramState) };
        }

        private IEnumerable<ProgramState> TrySetConstraint(ObjectConstraint objectConstraint,
            SymbolicValueConstraints oldConstraints, ProgramState currentProgramState)
        {
            var oldBoolConstraint = oldConstraints.GetConstraintOrDefault<BoolConstraint>();
            if (oldBoolConstraint != null)
            {
                if (objectConstraint == ObjectConstraint.Null)
                {
                    return Enumerable.Empty<ProgramState>();
                }

                return new[] { currentProgramState };
            }

            var oldObjectConstraint = oldConstraints.GetConstraintOrDefault<ObjectConstraint>();
            if (oldObjectConstraint != null)
            {
                if (oldObjectConstraint != objectConstraint)
                {
                    return Enumerable.Empty<ProgramState>();
                }

                return new[] { SetConstraint(objectConstraint, currentProgramState) };
            }

            throw new NotSupportedException($"Neither {nameof(BoolConstraint)}, nor {nameof(ObjectConstraint)}");
        }
    }
}
