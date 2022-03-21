/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2022 SonarSource SA
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
using Microsoft.CodeAnalysis;
using StyleCop.Analyzers.Lightup;

namespace SonarAnalyzer.SymbolicExecution.Roslyn
{
    public class SymbolicContext
    {
        public SymbolicValueCounter SymbolicValueCounter { get; }
        public IOperationWrapperSonar Operation { get; }
        public ProgramState State { get; }

        public SymbolicContext(SymbolicValueCounter symbolicValueCounter, IOperationWrapperSonar operation, ProgramState state)
        {
            SymbolicValueCounter = symbolicValueCounter ?? throw new ArgumentNullException(nameof(symbolicValueCounter));
            Operation = operation; // Operation can be null for the branch nodes.
            State = state ?? throw new ArgumentNullException(nameof(state));
        }

        public SymbolicValue CreateSymbolicValue() =>
            new(SymbolicValueCounter);

        public ProgramState SetOperationConstraint(SymbolicConstraint constraint) =>
            State.SetOperationConstraint(Operation, SymbolicValueCounter, constraint);

        public ProgramState SetSymbolConstraint(ISymbol symbol, SymbolicConstraint constraint) =>
            State.SetSymbolConstraint(symbol, SymbolicValueCounter, constraint);

        public SymbolicContext WithState(ProgramState newState) =>
            State == newState ? this : new(SymbolicValueCounter, Operation, newState);
    }
}
