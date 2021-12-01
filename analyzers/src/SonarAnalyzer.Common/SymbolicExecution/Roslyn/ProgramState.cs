/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2021 SonarSource SA
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
using StyleCop.Analyzers.Lightup;

namespace SonarAnalyzer.SymbolicExecution.Roslyn
{
    public sealed class ProgramState
    {
        public static readonly ProgramState Empty = new ProgramState(ImmutableDictionary<IOperationWrapperSonar, SymbolicValue>.Empty);

        private readonly ImmutableDictionary<IOperationWrapperSonar, SymbolicValue> operationValue;

        public SymbolicValue this[IOperationWrapperSonar operation] => operationValue.TryGetValue(operation, out var value) ? value : null;

        private ProgramState(ImmutableDictionary<IOperationWrapperSonar, SymbolicValue> operationValue)
        {
            this.operationValue = operationValue;
        }

        public ProgramState AddOperationValue(IOperationWrapperSonar operation, SymbolicValue value) =>
            new ProgramState(operationValue.Add(operation, value));
    }
}
