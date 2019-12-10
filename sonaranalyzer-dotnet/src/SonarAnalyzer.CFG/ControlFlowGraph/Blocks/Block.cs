/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2019 SonarSource SA
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
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using SonarAnalyzer.Helpers;

namespace SonarAnalyzer.ControlFlowGraph
{
    /// <summary>
    /// Basic building blocks of a Control Flow Graph (<see cref="IControlFlowGraph"/>).
    /// Holds a list of instructions which have no jumps between them.
    /// </summary>
    public class Block
    {
        private readonly Lazy<IReadOnlyList<SyntaxNode>> _instructions;
        private readonly Lazy<IReadOnlyCollection<Block>> _predecessorBlocks;
        private readonly Lazy<ISet<Block>> _allSuccessors;
        private readonly Lazy<ISet<Block>> _allPredecessors;

        // Protected to allow extending and mocking
        protected Block()
        {
            _instructions = new Lazy<IReadOnlyList<SyntaxNode>>(() => ReversedInstructions.Reverse().ToImmutableArray());
            _predecessorBlocks = new Lazy<IReadOnlyCollection<Block>>(() => EditablePredecessorBlocks.ToImmutableHashSet());
            _allSuccessors = new Lazy<ISet<Block>>(() => GetAll(this, b => b.SuccessorBlocks));
            _allPredecessors = new Lazy<ISet<Block>>(() => GetAll(this, b => b.PredecessorBlocks));
        }

        public virtual IReadOnlyList<SyntaxNode> Instructions => _instructions.Value;

        public virtual IReadOnlyCollection<Block> PredecessorBlocks => _predecessorBlocks.Value;

        public virtual IReadOnlyList<Block> SuccessorBlocks { get; } = ImmutableArray.Create<Block>();

        public IList<SyntaxNode> ReversedInstructions { get; } = new List<SyntaxNode>();

        internal ISet<Block> EditablePredecessorBlocks { get; } = new HashSet<Block>();

        internal virtual Block GetPossibleNonEmptySuccessorBlock()
        {
            return this;
        }

        internal virtual void ReplaceSuccessors(Dictionary<Block, Block> replacementMapping)
        {
        }

        public ISet<Block> AllSuccessorBlocks => _allSuccessors.Value;

        public ISet<Block> AllPredecessorBlocks => _allPredecessors.Value;

        private static ISet<Block> GetAll(Block initial, Func<Block, IEnumerable<Block>> getNexts)
        {
            var toProcess = new Queue<Block>();
            var alreadyProcesses = new HashSet<Block>();
            getNexts(initial).ToList().ForEach(b => toProcess.Enqueue(b));
            while (toProcess.Count != 0)
            {
                var current = toProcess.Dequeue();
                if (alreadyProcesses.Contains(current))
                {
                    continue;
                }

                alreadyProcesses.Add(current);

                getNexts(current).ToList().ForEach(b => toProcess.Enqueue(b));
            }

            return alreadyProcesses.ToHashSet();
        }
    }
}
