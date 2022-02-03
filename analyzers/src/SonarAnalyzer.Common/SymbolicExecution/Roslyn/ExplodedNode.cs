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
using System.Linq;
using SonarAnalyzer.CFG.Roslyn;
using SonarAnalyzer.Extensions;
using SonarAnalyzer.Helpers;
using StyleCop.Analyzers.Lightup;

namespace SonarAnalyzer.SymbolicExecution.Roslyn
{
    internal sealed class ExplodedNode : IEquatable<ExplodedNode>
    {
        private readonly IOperationWrapperSonar[] operations;
        private readonly int index;
        private readonly int programPointHash;

        public ProgramState State { get; }
        public BasicBlock Block { get; }
        public IOperationWrapperSonar Operation => index < operations.Length ? operations[index] : null;

        public ExplodedNode(BasicBlock block, ProgramState state)
            : this(block, block.OperationsAndBranchValue.ToExecutionOrder().ToArray(), 0, state) { }

        private ExplodedNode(BasicBlock block, IOperationWrapperSonar[] operations, int index, ProgramState state)
        {
            Block = block;
            State = state ?? throw new ArgumentNullException(nameof(state));
            this.operations = operations;
            this.index = index;
            programPointHash = ProgramPoint.Hash(block, index);
        }

        public ExplodedNode CreateNext(ProgramState state) =>
            new(Block, operations, index + 1, state);

        public override int GetHashCode() =>
            HashCode.Combine(programPointHash, State);

        public override bool Equals(object obj) =>
            Equals(obj as ExplodedNode);

        public bool Equals(ExplodedNode other) =>
            other is not null
            && other.programPointHash == programPointHash
            && other.State.Equals(State);
    }
}
