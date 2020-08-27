/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2020 SonarSource SA
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
using System.Linq;
using Microsoft.CodeAnalysis;
using SonarAnalyzer.Helpers;
using SonarAnalyzer.SymbolicExecution.Common.Constraints;
using SonarAnalyzer.SymbolicExecution.Constraints;
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

        protected readonly object identifier;

        private static int symbolicValueCounter;

        public SymbolicValue() : this(symbolicValueCounter++) { }

        private SymbolicValue(object identifier) => this.identifier = identifier;

        internal static SymbolicValue Create(ITypeSymbol type = null) =>
            type != null && type.OriginalDefinition.Is(KnownType.System_Nullable_T)
                ? new NullableSymbolicValue(new SymbolicValue())
                : new SymbolicValue();

        public override string ToString() =>
            identifier == null ? base.ToString() : "SV_" + identifier;

        public bool TryGetConstraints(ProgramState programState, out SymbolicValueConstraints constraints) =>
            programState.Constraints.TryGetValue(this, out constraints);

        public bool IsNull(ProgramState programState) =>
            programState.HasConstraint(this, ObjectConstraint.Null);

        public virtual IEnumerable<ProgramState> TrySetConstraint(SymbolicValueConstraint constraint, ProgramState programState)
        {
            if (constraint == null)
            {
                return new[] { programState };
            }
            // This condition is added first because the way of setting string constraints
            // when the value doesnt exist is different than the other types of constraints
            else if (constraint is StringConstraint stringConstraint)
            {
                return TrySetStringConstraint(stringConstraint, programState);
            }
            else if (!programState.Constraints.TryGetValue(this, out var oldConstraints))
            {
                return new[] { programState.SetConstraint(this, constraint) };
            }
            else if (constraint is BoolConstraint boolConstraint)
            {
                return TrySetBoolConstraint(boolConstraint, oldConstraints, programState);
            }
            else if (constraint is ObjectConstraint objectConstraint)
            {
                return TrySetObjectConstraint(objectConstraint, oldConstraints, programState);
            }
            else if (constraint is NullableValueConstraint
                || constraint is DisposableConstraint
                || constraint is CollectionCapacityConstraint
                || constraint is SerializationConstraint)
            {
                return new[] { programState };
            }
            else
            {
                throw UnexpectedConstraintException(constraint);
            }
        }

        public virtual IEnumerable<ProgramState> TrySetOppositeConstraint(SymbolicValueConstraint constraint, ProgramState programState) =>
            TrySetConstraint(constraint?.OppositeForLogicalNot, programState);

        public IEnumerable<ProgramState> TrySetConstraints(SymbolicValueConstraints constraints, ProgramState programState) =>
            TrySetConstraints(constraints, programState, false);

        public IEnumerable<ProgramState> TrySetOppositeConstraints(SymbolicValueConstraints constraints, ProgramState programState) =>
            TrySetConstraints(constraints, programState, true);

        protected static IEnumerable<ProgramState> ThrowIfTooMany(IEnumerable<ProgramState> states)
        {
            var stateList = states.ToList();
            if (stateList.Count >= AbstractExplodedGraph.MaxInternalStateCount)
            {
                throw new TooManyInternalStatesException();
            }
            return stateList;
        }

        private IEnumerable<ProgramState> TrySetConstraints(SymbolicValueConstraints constraints, ProgramState programState, bool isOppositeConstraints)
        {
            IEnumerable<ProgramState> programStates = new[] { programState };
            if (constraints == null)
            {
                return programStates;
            }

            foreach (var constraint in constraints.GetConstraints())
            {
                programStates = programStates.SelectMany(ps => isOppositeConstraints ? TrySetOppositeConstraint(constraint, ps) : TrySetConstraint(constraint, ps));
            }
            return programStates;
        }

        private IEnumerable<ProgramState> TrySetBoolConstraint(BoolConstraint constraint, SymbolicValueConstraints oldConstraints, ProgramState programState)
        {
            if (oldConstraints.HasConstraint(ObjectConstraint.Null))
            {
                // It was null, and now it should be true or false
                return new[] { programState };
            }
            else if (oldConstraints.GetConstraintOrDefault<BoolConstraint>() is { }  oldBoolConstraint && oldBoolConstraint != constraint)
            {
                return Enumerable.Empty<ProgramState>();
            }
            else
            {
                // Either same bool constraint, or previously not null, and now a bool constraint
                return new[] { programState.SetConstraint(this, constraint) };
            }
        }

        private IEnumerable<ProgramState> TrySetObjectConstraint(ObjectConstraint constraint, SymbolicValueConstraints oldConstraints, ProgramState programState)
        {
            if (oldConstraints.HasConstraint<StringConstraint>())
            {
                return constraint == ObjectConstraint.Null ? Enumerable.Empty<ProgramState>() : new[] { programState.SetConstraint(this, constraint) };
            }
            else if (oldConstraints.HasConstraint<BoolConstraint>())
            {
                return constraint == ObjectConstraint.Null ? Enumerable.Empty<ProgramState>() : new[] { programState };
            }
            else if (oldConstraints.HasConstraint<DisposableConstraint>())
            {
                return new[] { programState.SetConstraint(this, constraint) };
            }
            else if (oldConstraints.GetConstraintOrDefault<ObjectConstraint>() is { } oldObjectConstraint)
            {
                return oldObjectConstraint != constraint ? Enumerable.Empty<ProgramState>() : new[] { programState.SetConstraint(this, constraint) };
            }
            else
            {
                throw UnexpectedConstraintException(constraint);
            }
        }

        private IEnumerable<ProgramState> TrySetStringConstraint(StringConstraint constraint, ProgramState programState)
        {
            // Currently FullOrNullString is never set as a constraint. the combination of Fullstring + NotNull is equivalent to it.
            // it is used to express the oposite of EmptyString Constraint
            if (!programState.Constraints.TryGetValue(this, out var oldConstraints))
            {
                return SetNewStringConstraint(constraint, ref programState);
            }
            else if (oldConstraints.GetConstraintOrDefault<StringConstraint>() is { } oldStringConstraint)
            {
                return UpdateStringConstraint(constraint, programState, oldStringConstraint, this);
            }
            else if (oldConstraints.GetConstraintOrDefault<ObjectConstraint>() is { } oldObjectConstraint)
            {
                return UpdateObjectConstraint(constraint, programState, oldObjectConstraint);
            }
            else
            {
                throw UnexpectedConstraintException(constraint);
            }
        }

        private IEnumerable<ProgramState> UpdateObjectConstraint(StringConstraint constraint, ProgramState programState, ObjectConstraint oldObjectConstraint)
        {
            if (oldObjectConstraint == ObjectConstraint.Null)
            {
                return StringConstraint.IsNotNullConstraint(constraint) ? Enumerable.Empty<ProgramState>() : new[] { programState };
            }
            else if (constraint == StringConstraint.FullOrNullString)
            {
                return new[] { programState.SetConstraint(this, StringConstraint.FullString) };
            }
            else if (constraint == StringConstraint.NotWhiteSpaceString)
            {
                return new[] { programState.SetConstraint(this, StringConstraint.FullNotWhiteSpaceString) };
            }
            else
            {
                return new[] { programState.SetConstraint(this, constraint) };
            }
        }

        private static IEnumerable<ProgramState> UpdateStringConstraint(StringConstraint constraint, ProgramState programState, StringConstraint oldConstraint, SymbolicValue sv)
        {
            // oldConstraint is never FullOrNullString nor NotWhiteSpaceString
            var newFullString = constraint == StringConstraint.FullString;
            var newEmptyString = constraint == StringConstraint.EmptyString;
            var newFullOrNullString = constraint == StringConstraint.FullOrNullString;
            var newFullNotWhiteSpaceString = constraint == StringConstraint.FullNotWhiteSpaceString;
            var newWhiteSpaceString = constraint == StringConstraint.WhiteSpaceString;
            var newNotWhiteSpaceString = constraint == StringConstraint.NotWhiteSpaceString;

            if (oldConstraint == StringConstraint.EmptyString && (newFullString || newFullOrNullString || newFullNotWhiteSpaceString || newWhiteSpaceString))
            {
                return Enumerable.Empty<ProgramState>();
            }
            else if (oldConstraint == StringConstraint.WhiteSpaceString && (newFullNotWhiteSpaceString || newNotWhiteSpaceString || newEmptyString))
            {
                return Enumerable.Empty<ProgramState>();
            }
            else if (oldConstraint == StringConstraint.FullNotWhiteSpaceString && (newWhiteSpaceString || newEmptyString))
            {
                return Enumerable.Empty<ProgramState>();
            }
            else if (oldConstraint == StringConstraint.FullString && newEmptyString)
            {
                return Enumerable.Empty<ProgramState>();
            }
            else if (oldConstraint == StringConstraint.FullString && (newWhiteSpaceString || newFullNotWhiteSpaceString))
            {
                return new[] { programState.SetConstraint(sv, constraint) };
            }
            else
            {
                return new[] { programState };
            }
        }

        private IEnumerable<ProgramState> SetNewStringConstraint(StringConstraint constraint, ref ProgramState programState)
        {
            if (StringConstraint.IsNotNullConstraint(constraint))
            {
                programState = programState.SetConstraint(this, constraint);
                programState = programState.SetConstraint(this, ObjectConstraint.NotNull);
            }
            return new[] { programState };
        }

        private static Exception UnexpectedConstraintException(SymbolicValueConstraint constraint) =>
            new NotSupportedException($"Unexpected constraint type: {constraint.GetType().Name}.");

        private class BoolLiteralSymbolicValue : SymbolicValue
        {
            internal BoolLiteralSymbolicValue(bool value) : base(value) { }
        }

        private class ThisSymbolicValue : SymbolicValue
        {
            internal ThisSymbolicValue() : base(new object()) { }

            public override string ToString() => "SV_THIS";
        }

        private class BaseSymbolicValue : SymbolicValue
        {
            internal BaseSymbolicValue() : base(new object()) { }

            public override string ToString() => "SV_BASE";
        }

        private class NullSymbolicValue : SymbolicValue
        {
            internal NullSymbolicValue() : base(new object()) { }

            public override string ToString() => "SV_NULL";
        }
    }
}
