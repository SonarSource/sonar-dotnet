/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2023 SonarSource SA
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

namespace SonarAnalyzer.SymbolicExecution.Roslyn;

public readonly record struct ProgramStates
{
    private readonly ProgramState first;
    private readonly ProgramState second;
    private readonly ProgramState[] others;

    public ProgramStates() : this(null, null) { }

    public ProgramStates(ProgramState first) : this(first, null)
        => this.first = first;

    public ProgramStates(ProgramState first, ProgramState second) : this(first, second, Array.Empty<ProgramState>())
    {
    }

    public ProgramStates(ProgramState first, ProgramState second, params ProgramState[] others)
    {
        this.first = first;
        this.second = second;
        this.others = others;
    }

    public int Length =>
        this switch
        {
            { first: null } => 0,
            { second: null } => 1,
            { others.Length: var otherLength } => otherLength + 2,
        };

    public static ProgramStates operator +(ProgramStates left, ProgramStates right)
    {
        if (left.Length == 0)
        {
            return right;
        }
        else if (right.Length == 0)
        {
            return left;
        }
        else
        {
            return CopyStates(left, right);
        }

        static ProgramStates CopyStates(ProgramStates left, ProgramStates right)
        {
            var newLength = left.Length + right.Length;
            ProgramState[] array = newLength > 2
                ? new ProgramState[newLength - 2]
                : null;
            ProgramState newFirst = null;
            ProgramState newSecond = null;
            var i = 0;
            foreach (var state in left)
            {
                Append(array, ref newFirst, ref newSecond, ref i, state);
            }
            foreach (var state in right)
            {
                Append(array, ref newFirst, ref newSecond, ref i, state);
            }
            return new(newFirst, newSecond, array ?? Array.Empty<ProgramState>());
        }

        static void Append(ProgramState[] array, ref ProgramState newFirst, ref ProgramState newSecond, ref int i, ProgramState state)
        {
            if (newFirst == null)
            {
                newFirst = state;
            }
            else if (newSecond == null)
            {
                newSecond = state;
            }
            else
            {
                array[i++] = state;
            }
        }
    }

    public Enumerator GetEnumerator()
        => new(this);

    public struct Enumerator
    {
        private readonly ProgramStates programStates;
        private int index = 0;

        public Enumerator(ProgramStates programStates) => this.programStates = programStates;

        public ProgramState Current
            => index switch
            {
                1 => programStates.first,
                2 => programStates.second,
                _ => programStates.others[index - 3],
            };
        public bool MoveNext()
           => ++index switch
           {
               1 => programStates.first != null,
               2 => programStates.second != null,
               _ => programStates.others.Length > index - 3,
           };
    }
}
