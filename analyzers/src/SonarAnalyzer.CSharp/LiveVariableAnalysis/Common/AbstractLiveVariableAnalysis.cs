/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2020 SonarSource SA
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
using SonarAnalyzer.ControlFlowGraph;

namespace SonarAnalyzer.LiveVariableAnalysis
{
    public abstract class AbstractLiveVariableAnalysis
    {
        private readonly IControlFlowGraph controlFlowGraph;
        private readonly List<Block> reversedBlocks;

        private readonly Dictionary<Block, HashSet<ISymbol>> liveOutStates = new Dictionary<Block, HashSet<ISymbol>>();
        private readonly Dictionary<Block, HashSet<ISymbol>> liveInStates = new Dictionary<Block, HashSet<ISymbol>>();

        private readonly Dictionary<Block, HashSet<ISymbol>> assigned = new Dictionary<Block, HashSet<ISymbol>>();
        private readonly Dictionary<Block, HashSet<ISymbol>> used = new Dictionary<Block, HashSet<ISymbol>>();

        protected readonly ISet<ISymbol> capturedVariables = new HashSet<ISymbol>();

        protected abstract void ProcessBlock(Block block, out HashSet<ISymbol> assignedInBlock, out HashSet<ISymbol> usedInBlock);

        protected AbstractLiveVariableAnalysis(IControlFlowGraph controlFlowGraph)
        {
            this.controlFlowGraph = controlFlowGraph;
            reversedBlocks = controlFlowGraph.Blocks.Reverse().ToList();
        }

        public IReadOnlyList<ISymbol> GetLiveOut(Block block) =>
            liveOutStates[block].Except(capturedVariables).ToImmutableArray();

        public IReadOnlyList<ISymbol> GetLiveIn(Block block) =>
            liveInStates[block].Except(capturedVariables).ToImmutableArray();

        public IReadOnlyList<ISymbol> CapturedVariables =>
            capturedVariables.ToImmutableArray();

        protected void PerformAnalysis()
        {
            foreach (var block in this.reversedBlocks)
            {
                ProcessBlock(block, out var assignedInBlock, out var usedInBlock);

                assigned[block] = assignedInBlock;
                used[block] = usedInBlock;
            }

            AnalyzeCfg();

            if (liveOutStates[controlFlowGraph.ExitBlock].Any())
            {
                throw new InvalidOperationException("Out of exit block should be empty");
            }
        }

        private void AnalyzeCfg()
        {
            var workList = new Queue<Block>();
            reversedBlocks.ForEach(b => workList.Enqueue(b));

            while (workList.Any())
            {
                var block = workList.Dequeue();

                if (!liveOutStates.ContainsKey(block))
                {
                    liveOutStates.Add(block, new HashSet<ISymbol>());
                }

                var liveOut = liveOutStates[block];

                // note that on the PHP LVA impl, the `liveOut` gets cleared before being updated
                foreach (var successor in block.SuccessorBlocks)
                {
                    if (liveInStates.ContainsKey(successor))
                    {
                        liveOut.UnionWith(liveInStates[successor]);
                    }
                }

                // in = used + (out - assigned)
                var liveIn = new HashSet<ISymbol>(used[block]);
                liveIn.UnionWith(liveOut.Except(assigned[block]));

                // if things have not changed, skip adding the predecessors to the workList
                if (liveInStates.ContainsKey(block) && liveIn.SetEquals(liveInStates[block]))
                {
                    continue;
                }

                liveInStates[block] = liveIn;

                foreach (var predecessor in block.PredecessorBlocks)
                {
                    workList.Enqueue(predecessor);
                }
            }
        }
    }
}
