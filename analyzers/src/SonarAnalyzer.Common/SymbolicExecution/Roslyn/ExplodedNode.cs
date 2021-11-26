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

using System;
using System.Linq;
using SonarAnalyzer.CFG.Roslyn;
using SonarAnalyzer.Extensions;
using StyleCop.Analyzers.Lightup;

namespace SonarAnalyzer.SymbolicExecution.Roslyn
{
    public sealed class ExplodedNode
    {
        private readonly IOperationWrapperSonar[] operations;      // ToDo: Validate performance and memory, compare with other options, 0) { }
        private readonly int index;

        public ProgramState State { get; }
        public IOperationWrapperSonar Operation => index < operations.Length ? operations[index] : null;

        public ExplodedNode(BasicBlock block, ProgramState state)
            : this(block.OperationsAndBranchValue.ToExecutionOrder().ToArray(), 0, state) { }

        public ExplodedNode(ExplodedNode predecessor, ProgramState state)
            : this(predecessor.operations, predecessor.index + 1, state) { }

        private ExplodedNode(IOperationWrapperSonar[] operations, int index, ProgramState state)
        {
            State = state ?? throw new ArgumentNullException(nameof(state));
            this.operations = operations;
            this.index = index;
        }
    }
}
