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

using SonarAnalyzer.CFG.Roslyn;

namespace SonarAnalyzer.SymbolicExecution.Roslyn;

public class SymbolicContext
{
    public BasicBlock Block { get; }
    public IOperationWrapperSonar Operation { get; }
    public ProgramState State { get; }
    public bool IsInLoop { get; }
    public IReadOnlyCollection<ISymbol> CapturedVariables { get; }

    public SymbolicContext(ExplodedNode node, IReadOnlyCollection<ISymbol> capturedVariables, bool isInLoop)
        : this(node.Block, node.Operation, node.State, isInLoop, capturedVariables) { }

    public SymbolicContext(BasicBlock block, IOperationWrapperSonar operation, ProgramState state, bool isInLoop, IReadOnlyCollection<ISymbol> capturedVariables)
    {
        Block = block;
        Operation = operation; // Operation can be null for the branch nodes.
        State = state ?? throw new ArgumentNullException(nameof(state));
        IsInLoop = isInLoop;
        CapturedVariables = capturedVariables ?? throw new ArgumentNullException(nameof(capturedVariables));
    }

    public ProgramState SetOperationConstraint(SymbolicConstraint constraint) =>
        State.SetOperationConstraint(Operation, constraint);

    public ProgramState SetSymbolConstraint(ISymbol symbol, SymbolicConstraint constraint) =>
        State.SetSymbolConstraint(symbol, constraint);

    public ProgramState SetOperationValue(SymbolicValue value) =>
        State.SetOperationValue(Operation, value);

    public SymbolicContext WithState(ProgramState newState) =>
        State == newState ? this : new(Block, Operation, newState, IsInLoop, CapturedVariables);
}
