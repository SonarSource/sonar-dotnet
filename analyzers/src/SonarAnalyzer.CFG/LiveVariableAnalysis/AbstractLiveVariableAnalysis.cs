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
using SonarAnalyzer.CFG.Sonar;

namespace SonarAnalyzer.LiveVariableAnalysis
{
    public abstract class AbstractLiveVariableAnalysis
    {
        private readonly IControlFlowGraph controlFlowGraph;
        private readonly List<Block> reversedBlocks;
        private readonly IDictionary<Block, HashSet<ISymbol>> liveOutStates = new Dictionary<Block, HashSet<ISymbol>>();
        private readonly IDictionary<Block, HashSet<ISymbol>> liveInStates = new Dictionary<Block, HashSet<ISymbol>>();
        private readonly ISet<ISymbol> capturedVariables = new HashSet<ISymbol>();

        protected abstract State ProcessBlock(Block block);

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
            var states = new Dictionary<Block, State>();
            foreach (var block in reversedBlocks)
            {
                var state = ProcessBlock(block);
                capturedVariables.UnionWith(state.CapturedVariables);
                states.Add(block, state);
            }

            AnalyzeCfg(states);

            if (liveOutStates[controlFlowGraph.ExitBlock].Any())
            {
                throw new InvalidOperationException("Out of exit block should be empty");
            }
        }

        private void AnalyzeCfg(Dictionary<Block, State> states)
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

                // in = usedBeforeAssigned + (out - assigned)
                var liveIn = new HashSet<ISymbol>(states[block].UsedBeforeAssigned.Concat(liveOut.Except(states[block].Assigned)));

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

        protected class State
        {
            public ISet<ISymbol> Assigned { get; } = new HashSet<ISymbol>();             // Kill: The set of variables that are assigned a value.
            public ISet<ISymbol> UsedBeforeAssigned { get; } = new HashSet<ISymbol>();   // Gen:  The set of variables that are used before any assignment.
            public ISet<ISymbol> ProcessedLocalFunctions { get; } = new HashSet<ISymbol>();
            public ISet<SyntaxNode> AssignmentLhs { get; } = new HashSet<SyntaxNode>();
            public ISet<ISymbol> CapturedVariables { get; } = new HashSet<ISymbol>();
        }
    }
}
