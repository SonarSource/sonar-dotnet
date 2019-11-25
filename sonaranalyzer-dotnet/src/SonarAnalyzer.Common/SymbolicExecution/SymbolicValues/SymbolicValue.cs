/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2019 SonarSource SA
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

        private static int SymbolicValueCounter;

        public SymbolicValue()
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
            if (this.identifier != null)
            {
                return "SV_" + this.identifier;
            }

            return base.ToString();
        }

        public bool TryGetConstraints(ProgramState programState, out SymbolicValueConstraints constraints)
        {
            return programState.Constraints.TryGetValue(this, out constraints);
        }

        public bool IsNull(ProgramState programState)
        {
            return programState.HasConstraint(this, ObjectConstraint.Null);
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
            ProgramState programState)
        {
            if (constraint == null)
            {
                return new[] { programState };
            }

            // This condition is added first because the way of setting string constraints
            // when the value doesnt exist is different than the other types of constraints
            if (constraint is StringConstraint stringConstraint)
            {
                return TrySetStringConstraint(stringConstraint,  programState);
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
                constraint is CollectionCapacityConstraint)
            {
                return new[] { programState };
            }

            throw new NotSupportedException($"Neither one of {nameof(BoolConstraint)}, {nameof(ObjectConstraint)}, " +
                $"{nameof(ObjectConstraint)}, {nameof(DisposableConstraint)}, {nameof(CollectionCapacityConstraint)}," +
                $"{nameof(StringConstraint)}.");
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

        private IEnumerable<ProgramState> TrySetBoolConstraint(BoolConstraint constraint,
            SymbolicValueConstraints oldConstraints, ProgramState programState)
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

        private IEnumerable<ProgramState> TrySetObjectConstraint(ObjectConstraint constraint,
            SymbolicValueConstraints oldConstraints, ProgramState programState)
        {
            if (oldConstraints.HasConstraint<StringConstraint>())
            {
                if (constraint == ObjectConstraint.Null)
                {
                    return Enumerable.Empty<ProgramState>();
                }

                return new[] { programState.SetConstraint(this, constraint) };
            }

            if (oldConstraints.HasConstraint<BoolConstraint>())
            {
                if (constraint == ObjectConstraint.Null)
                {
                    return Enumerable.Empty<ProgramState>();
                }

                return new[] { programState };
            }

            var oldObjectConstraint = oldConstraints.GetConstraintOrDefault<ObjectConstraint>();
            if (oldObjectConstraint != null)
            {
                if (oldObjectConstraint != constraint)
                {
                    return Enumerable.Empty<ProgramState>();
                }

                return new[] { programState.SetConstraint(this, constraint) };
            }

            throw new NotSupportedException($"Neither one of {nameof(BoolConstraint)}, {nameof(ObjectConstraint)}," +
                $"{nameof(StringConstraint)}.");
        }

        private IEnumerable<ProgramState> TrySetStringConstraint(StringConstraint constraint,
             ProgramState programState)
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

            throw new NotSupportedException($"Neither one of {nameof(ObjectConstraint)}," +
                $"{nameof(StringConstraint)}.");
        }

        private IEnumerable<ProgramState> UpdateObjectConstraint(StringConstraint constraint, ProgramState programState, ObjectConstraint oldObjectConstraint)
        {
            if (oldObjectConstraint == ObjectConstraint.Null)
            {
                if (constraint == StringConstraint.FullString || constraint == StringConstraint.EmptyString
                    || constraint == StringConstraint.FullNotWhiteSpaceString || constraint == StringConstraint.WhiteSpaceString)
                {
                    return Enumerable.Empty<ProgramState>();
                }
                return new[] { programState };
            }
            else
            {
                if(constraint == StringConstraint.FullOrNullString)
                {
                    return new[] { programState.SetConstraint(this, StringConstraint.FullString) };
                }

                if (constraint == StringConstraint.NotWhiteSpaceString)
                {
                    return new[] { programState.SetConstraint(this, StringConstraint.FullNotWhiteSpaceString) };
                }

                return new[] { programState.SetConstraint(this, constraint) };
            }
        }

        private static IEnumerable<ProgramState> UpdateStringConstraint(StringConstraint constraint, ProgramState programState, StringConstraint oldStringConstraint, SymbolicValue sv)
        {
            var newFullStringConstraint = constraint == StringConstraint.FullString;
            var newEmptyStringConstraint = constraint == StringConstraint.EmptyString;
            var newFullOrNullStringConstraint = constraint == StringConstraint.FullOrNullString;
            var oldFullStringConstraint = oldStringConstraint == StringConstraint.FullString;
            var oldEmptyStringConstraint = oldStringConstraint == StringConstraint.EmptyString;

            // oldStringConstraint is never FullOrNullString or NotWhiteSpaceString
            // constraints are empty Related Related
            if (((newFullStringConstraint && oldEmptyStringConstraint)
                || (newEmptyStringConstraint && oldFullStringConstraint))
                || (newFullOrNullStringConstraint && oldEmptyStringConstraint))
            {
                return Enumerable.Empty<ProgramState>();
            }

            var newFullNotWhiteSpaceStringConstraint = constraint == StringConstraint.FullNotWhiteSpaceString;
            var newWhiteSpaceStringConstraint = constraint == StringConstraint.WhiteSpaceString;
            var newNotWhiteSpaceStringConstraint = constraint == StringConstraint.NotWhiteSpaceString;
            var oldFullNotWhiteSpaceStringConstraint = oldStringConstraint == StringConstraint.FullNotWhiteSpaceString;
            var oldWhiteSpaceStringConstraint = oldStringConstraint == StringConstraint.WhiteSpaceString;

            // constraints are white space Related
            if (((newFullNotWhiteSpaceStringConstraint && oldWhiteSpaceStringConstraint
                || (newWhiteSpaceStringConstraint && oldFullNotWhiteSpaceStringConstraint))
                || (newNotWhiteSpaceStringConstraint && oldWhiteSpaceStringConstraint)))
            {
                return Enumerable.Empty<ProgramState>();
            }

            // constraints are mixed space Related
            if ((newEmptyStringConstraint && oldFullNotWhiteSpaceStringConstraint)
               || (newEmptyStringConstraint && oldWhiteSpaceStringConstraint)            
               || (newFullNotWhiteSpaceStringConstraint && oldEmptyStringConstraint)
               || (newWhiteSpaceStringConstraint && oldEmptyStringConstraint))
            {
                return Enumerable.Empty<ProgramState>();
            }


            if ((newWhiteSpaceStringConstraint && oldFullStringConstraint)
                ||(newFullNotWhiteSpaceStringConstraint && oldFullStringConstraint))
            {
                return new[] { programState.SetConstraint(sv, constraint) };
            }

            return new[] { programState };
        }

        private IEnumerable<ProgramState> SetNewStringConstraint(StringConstraint constraint, ref ProgramState programState)
        {
            if (constraint == StringConstraint.FullString)
            {
                programState = programState.SetConstraint(this, StringConstraint.FullString);
                programState = programState.SetConstraint(this, ObjectConstraint.NotNull);
            }

            else if (constraint == StringConstraint.EmptyString)
            {
                programState = programState.SetConstraint(this, StringConstraint.EmptyString);
                programState = programState.SetConstraint(this, ObjectConstraint.NotNull);
            }

            else if (constraint == StringConstraint.WhiteSpaceString)
            {
                programState = programState.SetConstraint(this, StringConstraint.WhiteSpaceString);
                programState = programState.SetConstraint(this, ObjectConstraint.NotNull);
            }

            else if (constraint == StringConstraint.FullNotWhiteSpaceString)
            {
                programState = programState.SetConstraint(this, StringConstraint.FullNotWhiteSpaceString);
                programState = programState.SetConstraint(this, ObjectConstraint.NotNull);
            }

            return new[] { programState };
        }
    }
}
