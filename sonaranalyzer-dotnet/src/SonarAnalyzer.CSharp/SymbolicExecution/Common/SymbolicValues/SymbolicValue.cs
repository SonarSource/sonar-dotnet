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

        private SymbolicValue(object identifier)
        {
            this.identifier = identifier;
        }

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
            if (constraint is StringConstraint stringConstraint)
            {
                return TrySetStringConstraint(stringConstraint, programState);
            }

            if (!programState.Constraints.TryGetValue(this, out var oldConstraints))
            {
                return new[] { programState.SetConstraint(this, constraint) };
            }

            if (constraint is BoolConstraint boolConstraint)
            {
                return TrySetBoolConstraint(boolConstraint, oldConstraints, programState);
            }

            if (constraint is ObjectConstraint objectConstraint)
            {
                return TrySetObjectConstraint(objectConstraint, oldConstraints, programState);
            }

            if (constraint is NullableValueConstraint ||
                constraint is DisposableConstraint ||
                constraint is CollectionCapacityConstraint ||
                constraint is SerializationConstraint)
            {
                return new[] { programState };
            }

            throw UnexpectedConstraintException(constraint);
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

            var oldBoolConstraint = oldConstraints.GetConstraintOrDefault<BoolConstraint>();
            if (oldBoolConstraint != null &&
                oldBoolConstraint != constraint)
            {
                return Enumerable.Empty<ProgramState>();
            }

            // Either same bool constraint, or previously not null, and now a bool constraint
            return new[] { programState.SetConstraint(this, constraint) };
        }

        private IEnumerable<ProgramState> TrySetObjectConstraint(ObjectConstraint constraint, SymbolicValueConstraints oldConstraints, ProgramState programState)
        {
            if (oldConstraints.HasConstraint<StringConstraint>())
            {
                return constraint == ObjectConstraint.Null ? Enumerable.Empty<ProgramState>() : new[] { programState.SetConstraint(this, constraint) };
            }

            if (oldConstraints.HasConstraint<BoolConstraint>())
            {
                return constraint == ObjectConstraint.Null ? Enumerable.Empty<ProgramState>() : new[] { programState };
            }

            if (oldConstraints.HasConstraint<DisposableConstraint>())
            {
                return new[] { programState.SetConstraint(this, constraint) };
            }

            var oldObjectConstraint = oldConstraints.GetConstraintOrDefault<ObjectConstraint>();
            if (oldObjectConstraint != null)
            {
                return oldObjectConstraint != constraint ? Enumerable.Empty<ProgramState>() : new[] { programState.SetConstraint(this, constraint) };
            }

            throw UnexpectedConstraintException(constraint);
        }

        private IEnumerable<ProgramState> TrySetStringConstraint(StringConstraint constraint, ProgramState programState)
        {
            // Currently FullOrNullString is never set as a constraint. the combination of Fullstring + NotNull is equivalent to it.
            // it is used to express the oposite of EmptyString Constraint
            if (!programState.Constraints.TryGetValue(this, out var oldConstraints))
            {
                return SetNewStringConstraint(constraint, ref programState);
            }

            var oldStringConstraint = oldConstraints.GetConstraintOrDefault<StringConstraint>();
            if (oldStringConstraint != null)
            {
                return UpdateStringConstraint(constraint, programState, oldStringConstraint, this);
            }

            var oldObjectConstraint = oldConstraints.GetConstraintOrDefault<ObjectConstraint>();
            if (oldObjectConstraint != null)
            {
                return UpdateObjectConstraint(constraint, programState, oldObjectConstraint);
            }

            throw UnexpectedConstraintException(constraint);
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

        private static IEnumerable<ProgramState> UpdateStringConstraint(StringConstraint constraint, ProgramState programState, StringConstraint oldStringConstraint, SymbolicValue sv)
        {
            //FIXME: Redesing
            // oldStringConstraint is never FullOrNullString or NotWhiteSpaceString
            var newFullStringConstraint = constraint == StringConstraint.FullString;
            var newEmptyStringConstraint = constraint == StringConstraint.EmptyString;
            var newFullOrNullStringConstraint = constraint == StringConstraint.FullOrNullString;
            var newFullNotWhiteSpaceStringConstraint = constraint == StringConstraint.FullNotWhiteSpaceString;
            var newWhiteSpaceStringConstraint = constraint == StringConstraint.WhiteSpaceString;
            var newNotWhiteSpaceStringConstraint = constraint == StringConstraint.NotWhiteSpaceString;
            var oldFullStringConstraint = oldStringConstraint == StringConstraint.FullString;

            if (oldStringConstraint == StringConstraint.EmptyString
                && (newFullStringConstraint || newFullOrNullStringConstraint || newFullNotWhiteSpaceStringConstraint || newWhiteSpaceStringConstraint))
            {
                return Enumerable.Empty<ProgramState>();
            }

            if(oldStringConstraint == StringConstraint.WhiteSpaceString
                && (newFullNotWhiteSpaceStringConstraint || newNotWhiteSpaceStringConstraint || newEmptyStringConstraint))
            {
                return Enumerable.Empty<ProgramState>();
            }

            if(oldStringConstraint == StringConstraint.FullNotWhiteSpaceString
                && (newWhiteSpaceStringConstraint || newEmptyStringConstraint))
            {
                return Enumerable.Empty<ProgramState>();
            }

            if (newEmptyStringConstraint && oldFullStringConstraint)
            {
                return Enumerable.Empty<ProgramState>();
            }

            return (newWhiteSpaceStringConstraint && oldFullStringConstraint) || (newFullNotWhiteSpaceStringConstraint && oldFullStringConstraint)
                ? new[] { programState.SetConstraint(sv, constraint) }
                : new[] { programState };
        }

        private static Exception UnexpectedConstraintException(SymbolicValueConstraint constraint) =>
            new NotSupportedException($"Unexpected constraint type: {constraint.GetType().Name}.");

        private IEnumerable<ProgramState> SetNewStringConstraint(StringConstraint constraint, ref ProgramState programState)
        {
            if (StringConstraint.IsNotNullConstraint(constraint))
            {
                programState = programState.SetConstraint(this, constraint);
                programState = programState.SetConstraint(this, ObjectConstraint.NotNull);
            }
            return new[] { programState };
        }

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
