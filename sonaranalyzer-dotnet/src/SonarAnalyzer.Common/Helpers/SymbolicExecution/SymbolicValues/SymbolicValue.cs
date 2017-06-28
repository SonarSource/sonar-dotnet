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

namespace SonarAnalyzer.Helpers.FlowAnalysis.Common
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

        internal ProgramState SetConstraint(SymbolicValueConstraint constraint, ProgramState programState)
        {
            if (constraint == null)
            {
                return programState;
            }

            var newConstraints = programState.Constraints.SetItem(this, constraint);
            newConstraints = AddConstraintTo<EqualsRelationship>(constraint, programState, newConstraints);

            if (constraint is BoolConstraint)
            {
                newConstraints = AddConstraintTo<NotEqualsRelationship>(constraint.OppositeForLogicalNot, programState, newConstraints);
            }

            return new ProgramState(
                programState.Values,
                newConstraints,
                programState.ProgramPointVisitCounts,
                programState.ExpressionStack,
                programState.Relationships);
        }

        private ImmutableDictionary<SymbolicValue, SymbolicValueConstraint> AddConstraintTo<TRelationship>(SymbolicValueConstraint constraint,
            ProgramState programState, ImmutableDictionary<SymbolicValue, SymbolicValueConstraint> constraints)
            where TRelationship: BinaryRelationship
        {
            var newConstraints = constraints;
            var equalSymbols = programState.Relationships
                            .OfType<TRelationship>()
                            .Select(r => GetOtherOperandFromMatchingRelationship(r))
                            .Where(e => e != null);

            foreach (var equalSymbol in equalSymbols.Where(e => !e.HasConstraint(constraint, programState)))
            {
                newConstraints = newConstraints.SetItem(equalSymbol, constraint);
            }

            return newConstraints;
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
            return programState.Constraints.ContainsKey(this) &&
                programState.Constraints[this].Implies(constraint);
        }

        public bool TryGetConstraint(ProgramState programState, out SymbolicValueConstraint constraint)
        {
            return programState.Constraints.TryGetValue(this, out constraint);
        }

        public bool IsNull(ProgramState programState)
        {
            return this.HasConstraint(ObjectConstraint.Null, programState);
        }

        protected IEnumerable<ProgramState> ThrowIfTooMany(IEnumerable<ProgramState> states)
        {
            var stateList = states.ToList();
            if (stateList.Count >= ExplodedGraph.MaxInternalStateCount)
            {
                throw new TooManyInternalStatesException();
            }

            return stateList;
        }

        public virtual IEnumerable<ProgramState> TrySetConstraint(SymbolicValueConstraint constraint, ProgramState currentProgramState)
        {
            if (constraint == null)
            {
                return new[] { currentProgramState };
            }

            SymbolicValueConstraint oldConstraint;
            if (!currentProgramState.Constraints.TryGetValue(this, out oldConstraint))
            {
                return new[] { SetConstraint(constraint, currentProgramState) };
            }

            var boolConstraint = constraint as BoolConstraint;
            if (boolConstraint != null)
            {
                return TrySetConstraint(boolConstraint, oldConstraint, currentProgramState);
            }

            var objectConstraint = constraint as ObjectConstraint;
            if (objectConstraint != null)
            {
                return TrySetConstraint(objectConstraint, oldConstraint, currentProgramState);
            }

            throw new NotSupportedException($"Neither {nameof(BoolConstraint)}, nor {nameof(ObjectConstraint)}");
        }

        private IEnumerable<ProgramState> TrySetConstraint(BoolConstraint boolConstraint, SymbolicValueConstraint oldConstraint,
            ProgramState currentProgramState)
        {
            if (oldConstraint == ObjectConstraint.Null)
            {
                // It was null, and now it should be true or false
                return Enumerable.Empty<ProgramState>();
            }

            var oldBoolConstraint = oldConstraint as BoolConstraint;
            if (oldBoolConstraint != null &&
                oldBoolConstraint != boolConstraint)
            {
                return Enumerable.Empty<ProgramState>();
            }

            // Either same bool constraint, or previously not null, and now a bool constraint
            return new[] { SetConstraint(boolConstraint, currentProgramState) };
        }

        private IEnumerable<ProgramState> TrySetConstraint(ObjectConstraint objectConstraint, SymbolicValueConstraint oldConstraint,
            ProgramState currentProgramState)
        {
            var oldBoolConstraint = oldConstraint as BoolConstraint;
            if (oldBoolConstraint != null)
            {
                if (objectConstraint == ObjectConstraint.Null)
                {
                    return Enumerable.Empty<ProgramState>();
                }

                return new[] { currentProgramState };
            }

            var oldObjectConstraint = oldConstraint as ObjectConstraint;
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
