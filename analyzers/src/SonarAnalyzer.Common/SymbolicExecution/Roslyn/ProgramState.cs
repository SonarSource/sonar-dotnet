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

using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using StyleCop.Analyzers.Lightup;

namespace SonarAnalyzer.SymbolicExecution.Roslyn
{
    public sealed class ProgramState
    {
        public static readonly ProgramState Empty = new(ImmutableDictionary<IOperation, SymbolicValue>.Empty, ImmutableDictionary<ISymbol, SymbolicValue>.Empty);

        private readonly ImmutableDictionary<IOperation, SymbolicValue> operationValue;     // Current SymbolicValue result of a given operation
        private readonly ImmutableDictionary<ISymbol, SymbolicValue> symbolValue;

        public SymbolicValue this[IOperationWrapperSonar operation] => this[operation.Instance];
        public SymbolicValue this[IOperation operation] => operationValue.TryGetValue(operation, out var value) ? value : null;
        public SymbolicValue this[ISymbol symbol] => symbolValue.TryGetValue(symbol, out var value) ? value : null;

        private ProgramState(ImmutableDictionary<IOperation, SymbolicValue> operationValue, ImmutableDictionary<ISymbol, SymbolicValue> symbolValue)
        {
            this.operationValue = operationValue;
            this.symbolValue = symbolValue;
        }

        public ProgramState SetOperationValue(IOperationWrapperSonar operation, SymbolicValue value) =>
            SetOperationValue(operation.Instance, value);

        public ProgramState SetOperationValue(IOperation operation, SymbolicValue value) =>
            new(operationValue.SetItem(operation, value), symbolValue);

        public ProgramState SetSymbolValue(ISymbol symbol, SymbolicValue value) =>
            new(operationValue, symbolValue.SetItem(symbol, value));

        public IEnumerable<ISymbol> SymbolsWith(SymbolicConstraint constraint) =>
            symbolValue.Where(x => x.Value != null && x.Value.HasConstraint(constraint)).Select(x => x.Key);
    }
}
