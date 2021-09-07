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
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using SonarAnalyzer.CFG.Helpers;

namespace SonarAnalyzer.CFG.LiveVariableAnalysis
{
    public abstract class LiveVariableAnalysisBase<TCfg, TBlock>
    {
        protected readonly TCfg cfg;
        private readonly IDictionary<TBlock, HashSet<ISymbol>> blockLiveOut = new Dictionary<TBlock, HashSet<ISymbol>>();
        private readonly IDictionary<TBlock, HashSet<ISymbol>> blockLiveIn = new Dictionary<TBlock, HashSet<ISymbol>>();
        private readonly ISet<ISymbol> captured = new HashSet<ISymbol>();

        protected abstract TBlock ExitBlock { get; }
        protected abstract State ProcessBlock(TBlock block);
        protected abstract IEnumerable<TBlock> ReversedBlocks();
        protected abstract IEnumerable<TBlock> Successors(TBlock block);
        protected abstract IEnumerable<TBlock> Predecessors(TBlock block);

        public IReadOnlyList<ISymbol> CapturedVariables => captured.ToImmutableArray();

        protected LiveVariableAnalysisBase(TCfg cfg) =>
            this.cfg = cfg;

        /// <summary>
        /// LiveIn variables are alive when entering block. They are read inside the block or any of it's successors.
        /// </summary>
        public IReadOnlyList<ISymbol> LiveIn(TBlock block) =>
            blockLiveIn[block].Except(captured).ToImmutableArray();

        /// <summary>
        /// LiveOut variables are alive when exiting block. They are read in any of it's successors.
        /// </summary>
        public IReadOnlyList<ISymbol> LiveOut(TBlock block) =>
            blockLiveOut[block].Except(captured).ToImmutableArray();

        protected void Analyze()
        {
            var states = new Dictionary<TBlock, State>();
            var queue = new Queue<TBlock>();
            foreach (var block in ReversedBlocks())
            {
                var state = ProcessBlock(block);
                captured.UnionWith(state.Captured);
                states.Add(block, state);
                blockLiveIn.Add(block, new HashSet<ISymbol>());
                blockLiveOut.Add(block, new HashSet<ISymbol>());
                queue.Enqueue(block);
            }
            while (queue.Any())
            {
                var block = queue.Dequeue();
                var liveOut = blockLiveOut[block];
                // note that on the PHP LVA impl, the `liveOut` gets cleared before being updated
                foreach (var successorLiveIn in Successors(block).Select(x => blockLiveIn[x]).Where(x => x.Any()))
                {
                    liveOut.UnionWith(successorLiveIn);
                }
                // liveIn = UsedBeforeAssigned + (LiveOut - Assigned)
                var liveIn = states[block].UsedBeforeAssigned.Concat(liveOut.Except(states[block].Assigned)).ToHashSet();
                // Don't enqueue predecessors if nothing changed.
                if (!liveIn.SetEquals(blockLiveIn[block]))
                {
                    blockLiveIn[block] = liveIn;
                    foreach (var predecessor in Predecessors(block))
                    {
                        queue.Enqueue(predecessor);
                    }
                }
            }
            if (blockLiveOut[ExitBlock].Any())
            {
                throw new InvalidOperationException("Out of exit block should be empty");
            }
        }

        protected class State
        {
            public ISet<ISymbol> Assigned { get; } = new HashSet<ISymbol>();            // Kill: The set of variables that are assigned a value.
            public ISet<ISymbol> UsedBeforeAssigned { get; } = new HashSet<ISymbol>();  // Gen:  The set of variables that are used before any assignment.
            public ISet<ISymbol> ProcessedLocalFunctions { get; } = new HashSet<ISymbol>();
            public ISet<SyntaxNode> AssignmentLhs { get; } = new HashSet<SyntaxNode>(); // FIXME: Not needed in Roslyn
            public ISet<ISymbol> Captured { get; } = new HashSet<ISymbol>();
        }
    }
}
