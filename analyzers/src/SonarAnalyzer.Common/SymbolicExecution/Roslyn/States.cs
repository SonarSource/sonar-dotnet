/*
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

global using ProgramStates = SonarAnalyzer.SymbolicExecution.Roslyn.States<SonarAnalyzer.SymbolicExecution.Roslyn.ProgramState>;
global using SymbolicContexts = SonarAnalyzer.SymbolicExecution.Roslyn.States<SonarAnalyzer.SymbolicExecution.Roslyn.SymbolicContext>;

using System.Runtime.CompilerServices;

namespace SonarAnalyzer.SymbolicExecution.Roslyn;

public readonly struct States<T> where T : class
{
    private readonly T first;
    private readonly T second;
    private readonly T[] others;

    public States() : this(null, null) { }

    public States(T first) : this(first, null) { }

    public States(T first, T second) : this(first, second, Array.Empty<T>()) { }

    public States(T first, T second, params T[] others)
    {
        this.first = first;
        this.second = second;
        this.others = others ?? Array.Empty<T>();
    }

    public int Length =>
        this switch
        {
            { first: null } => 0,
            { second: null } => 1,
            { others.Length: var otherLength } => otherLength + 2,
        };

    public static States<T> operator +(States<T> left, States<T> right)
    {
        var leftLength = left.Length;
        if (leftLength == 0)
        {
            return right;
        }

        var rightLength = right.Length;
        if (rightLength == 0)
        {
            return left;
        }

        var newLength = leftLength + rightLength;
        var array = newLength > 2
            ? new T[newLength - 2]
            : null;
        T newFirst = null;
        T newSecond = null;
        var i = 0;
        foreach (var state in left)
        {
            Append(array, ref newFirst, ref newSecond, ref i, state);
        }
        foreach (var state in right)
        {
            Append(array, ref newFirst, ref newSecond, ref i, state);
        }
        return new(newFirst, newSecond, array ?? Array.Empty<T>());

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static void Append(T[] array, ref T newFirst, ref T newSecond, ref int i, T state)
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

    public Enumerator GetEnumerator() =>
        new(this);

    public struct Enumerator
    {
        private readonly States<T> states;
        private int index;

        public Enumerator(States<T> states) => this.states = states;

        public readonly T Current =>
            index switch
            {
                1 => states.first,
                2 => states.second,
                _ => states.others[index - 3],
            };

        public bool MoveNext() =>
           ++index switch
           {
               1 => states.first != null,
               2 => states.second != null,
               _ => states.others.Length > index - 3,
           };
    }
}
