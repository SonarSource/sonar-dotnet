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

public class SymbolicContext
{
    public IOperationWrapperSonar Operation { get; }
    public ProgramState State { get; }
    public int VisitCount { get; }
    public bool IsLoopCondition { get; }
    public IReadOnlyCollection<ISymbol> CapturedVariables { get; }

    public SymbolicContext(ExplodedNode node, IReadOnlyCollection<ISymbol> capturedVariables, bool isLoopCondition)
        : this(node.Operation, node.State, isLoopCondition, node.VisitCount, capturedVariables) { }

    public SymbolicContext(IOperationWrapperSonar operation, ProgramState state, bool isLoopCondition, int visitCount, IReadOnlyCollection<ISymbol> capturedVariables)
    {
        Operation = operation; // Operation can be null for the branch nodes.
        State = state ?? throw new ArgumentNullException(nameof(state));
        VisitCount = visitCount;
        IsLoopCondition = isLoopCondition;
        CapturedVariables = capturedVariables ?? throw new ArgumentNullException(nameof(capturedVariables));
    }

    public ProgramState SetOperationConstraint(SymbolicConstraint constraint) =>
        State.SetOperationConstraint(Operation, constraint);

    public ProgramState SetSymbolConstraint(ISymbol symbol, SymbolicConstraint constraint) =>
        State.SetSymbolConstraint(symbol, constraint);

    public ProgramState SetOperationValue(SymbolicValue value) =>
        State.SetOperationValue(Operation, value);

    public SymbolicContext WithState(ProgramState newState) =>
        State == newState ? this : new(Operation, newState, IsLoopCondition, VisitCount, CapturedVariables);
}
