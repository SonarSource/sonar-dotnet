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
