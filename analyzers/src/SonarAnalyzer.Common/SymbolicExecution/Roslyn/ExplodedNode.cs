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

public sealed class ExplodedNode : IEquatable<ExplodedNode>
{
    private readonly IOperationWrapperSonar[] operations;
    private readonly int index;
    private readonly int programPointHash;

    public ProgramState State { get; private set; }
    public BasicBlock Block { get; }
    public FinallyPoint FinallyPoint { get; }
    public IOperationWrapperSonar Operation => index < operations.Length ? operations[index] : default;
    public int VisitCount => State.GetVisitCount(programPointHash);

    public ExplodedNode(BasicBlock block, ProgramState state, FinallyPoint finallyPoint)
        : this(block, block.OperationsAndBranchValue.ToExecutionOrder().ToArray(), 0, state, finallyPoint) { }

    private ExplodedNode(BasicBlock block, IOperationWrapperSonar[] operations, int index, ProgramState state, FinallyPoint finallyPoint)
    {
        Block = block;
        State = state ?? throw new ArgumentNullException(nameof(state));
        FinallyPoint = finallyPoint;
        this.operations = operations;
        this.index = index;
        programPointHash = ProgramPoint.Hash(block, index);
        state.CheckConsistency();
    }

    public ExplodedNode CreateNext(ProgramState state) =>
        new(Block, operations, index + 1, state, FinallyPoint);

    public int AddVisit()
    {
        State = State.AddVisit(programPointHash);
        return State.GetVisitCount(programPointHash);
    }

    public override int GetHashCode() =>
        HashCode.Combine(programPointHash, State, FinallyPoint?.BlockIndex);

    public override bool Equals(object obj) =>
        Equals(obj as ExplodedNode);

    public bool Equals(ExplodedNode other) =>
        other is not null
        && other.programPointHash == programPointHash
        && other.State.Equals(State)
        && HasSameFinallyPointChain(other.FinallyPoint);

    public override string ToString() =>
        Operation.Instance is { } operation
            ? $"Block #{Block.Ordinal}, Operation #{index}, {operation.Serialize()}{Environment.NewLine}{State}"
            : $"Block #{Block.Ordinal}, Branching{Environment.NewLine}{State}";

    private bool HasSameFinallyPointChain(FinallyPoint other)
    {
        var current = FinallyPoint;
        while (current is not null && other is not null)
        {
            if (current.BlockIndex == other.BlockIndex && current.BranchDestination == other.BranchDestination)
            {
                current = current.Previous;
                other = other.Previous;
            }
            else
            {
                return false;
            }
        }
        return current is null && other is null;
    }
}
