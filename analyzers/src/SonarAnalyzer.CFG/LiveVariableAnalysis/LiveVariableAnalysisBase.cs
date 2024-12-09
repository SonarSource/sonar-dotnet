/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2014-2024 SonarSource SA
 * mailto:info AT sonarsource DOT com
 * This program is free software; you can redistribute it and/or
 * modify it under the terms of the Sonar Source-Available License Version 1, as published by SonarSource SA.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
 * See the Sonar Source-Available License for more details.
 *
 * You should have received a copy of the Sonar Source-Available License
 * along with this program; if not, see https://sonarsource.com/license/ssal/
 */

using SonarAnalyzer.CFG.Common;

namespace SonarAnalyzer.CFG.LiveVariableAnalysis;

public abstract class LiveVariableAnalysisBase<TCfg, TBlock>
{
    protected readonly ISymbol originalDeclaration;
    protected readonly CancellationToken Cancel;
    private readonly Dictionary<TBlock, HashSet<ISymbol>> blockLiveOut = new();
    private readonly Dictionary<TBlock, HashSet<ISymbol>> blockLiveIn = new();
    private readonly HashSet<ISymbol> captured = [];

    public abstract bool IsLocal(ISymbol symbol);
    protected abstract TBlock ExitBlock { get; }
    protected abstract State ProcessBlock(TBlock block);
    protected abstract IEnumerable<TBlock> ReversedBlocks();
    protected abstract IEnumerable<TBlock> Successors(TBlock block);
    protected abstract IEnumerable<TBlock> Predecessors(TBlock block);

    public TCfg Cfg { get; }
    public IReadOnlyCollection<ISymbol> CapturedVariables => captured;

    protected LiveVariableAnalysisBase(TCfg cfg, ISymbol originalDeclaration, CancellationToken cancel)
    {
        Cfg = cfg;
        this.originalDeclaration = originalDeclaration;
        Cancel = cancel;
    }

    /// <summary>
    /// LiveIn variables are alive when entering block. They are read inside the block or any of it's successors.
    /// </summary>
    public IEnumerable<ISymbol> LiveIn(TBlock block) =>
        blockLiveIn[block].Except(captured);

    /// <summary>
    /// LiveOut variables are alive when exiting block. They are read in any of it's successors.
    /// </summary>
    public IEnumerable<ISymbol> LiveOut(TBlock block) =>
        blockLiveOut[block].Except(captured);

    protected void Analyze()
    {
        var states = new Dictionary<TBlock, State>();
        var queue = new UniqueQueue<TBlock>();
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
            Cancel.ThrowIfCancellationRequested();

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

    protected abstract class State
    {
        public HashSet<ISymbol> Assigned { get; } = [];            // Kill: The set of variables that are assigned a value.
        public HashSet<ISymbol> UsedBeforeAssigned { get; } = [];  // Gen:  The set of variables that are used before any assignment.
        public HashSet<ISymbol> ProcessedLocalFunctions { get; } = [];
        public HashSet<ISymbol> Captured { get; } = [];
    }
}
