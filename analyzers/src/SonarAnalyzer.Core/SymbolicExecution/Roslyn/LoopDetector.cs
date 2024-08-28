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

internal class LoopDetector
{
    private readonly HashSet<int> loopBlocks;

    public LoopDetector(ControlFlowGraph cfg) =>
        loopBlocks = DetectLoopBlockOrdinals(cfg).ToHashSet();

    public bool IsInLoop(BasicBlock block) =>
        loopBlocks.Contains(block.Ordinal);

    // Detects loops in the cfg using a modified DFS algorithm:
    // Explore the cfg depth-first, keeping track of the path from the root to the current block.
    // If the current block is already in the path, a loop is detected.
    // Otherwise, if the last block in the path is part of an already detected loop that intersects with an earlier part of the path, merge the relevant sub-path with the loop.
    // Due to the nature of DFS:
    // If a block is processed already, current path has no loop and is not intersecting with already detected loops, the block is not part of any loop.
    private static IEnumerable<int> DetectLoopBlockOrdinals(ControlFlowGraph cfg)
    {
        var processed = new HashSet<int>();
        var loops = new List<HashSet<int>>();
        var toProcess = new Stack<int[]>();
        toProcess.Push([1]);
        while (toProcess.TryPop(out var path))
        {
            var last = path[path.Length - 1];
            if (processed.Add(last))
            {
                foreach (var successor in Successors(cfg, last))
                {
                    toProcess.Push([.. path, successor]);
                }
            }
            else
            {
                var index = path.IndexOf(x => x == last);
                if (index < path.Length - 1)         // is the block in the path twice? -> loop
                {
                    loops.Add(path.Skip(index).Take(path.Length - 1 - index).ToHashSet());
                }
                else
                {
                    MergeWithIntersectingLoops(path, last);
                }
            }
        }
        return loops.SelectMany(x => x);

        // For a given path [..A..B], if we find a loop that both A and B are part of, all blocks between A and B should also be considered part of that loop.
        void MergeWithIntersectingLoops(int[] path, int last)
        {
            foreach (var loop in loops.Where(x => x.Contains(last)))    // B is part of the loop
            {
                var intersection = path.IndexOf(loop.Contains);
                if (intersection < path.Length - 1)                     // A is part of the loop
                {
                    loop.AddRange(path.Skip(intersection + 1).Take(path.Length - 1 - intersection));  // add all blocks between A and B to the loop
                }
            }
        }
    }

    private static IEnumerable<int> Successors(ControlFlowGraph cfg, int index)
    {
        var block = cfg.Blocks[index];
        var successors = block.SuccessorBlocks.Select(x => x.Ordinal);
        if (block.EnclosingRegion(ControlFlowRegionKind.Try) is { } tryRegion)
        {
            successors = successors.Concat(tryRegion.ReachableHandlers().Select(x => x.FirstBlockOrdinal));
        }
        if (block.EnclosingRegion(ControlFlowRegionKind.Finally) is { } finallyRegion
            && block.Successors.Length == 1
            && block.Successors[0].Semantics == ControlFlowBranchSemantics.StructuredExceptionHandling)
        {
            var tryFinallyRegion = finallyRegion.EnclosingRegion;
            successors = successors.Concat(
                cfg.Blocks.SelectMany(x => x.Successors)
                    .Where(x => x.Destination is not null && x.LeavingRegions.Contains(tryFinallyRegion))
                    .Select(x => x.Destination.Ordinal));
        }
        return successors.Distinct();
    }
}
