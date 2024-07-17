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

using SonarAnalyzer.CFG.LiveVariableAnalysis;
using SonarAnalyzer.CFG.Roslyn;

namespace SonarAnalyzer.CFG;

public static partial class CfgSerializer
{
    private sealed class RoslynLvaWalker : RoslynCfgWalker
    {
        private readonly RoslynLiveVariableAnalysis lva;

        public RoslynLvaWalker(RoslynLiveVariableAnalysis lva, DotWriter writer, RoslynCfgIdProvider cfgIdProvider) : base(writer, cfgIdProvider)
        {
            this.lva = lva;
        }

        protected override void WriteEdges(BasicBlock block)
        {
            var lvaPredecessors = lva.BlockPredecessors[block.Ordinal];
            foreach (var predecessor in block.Predecessors)
            {
                var condition = string.Empty;
                if (predecessor.Source.ConditionKind != ControlFlowConditionKind.None)
                {
                    condition = predecessor == predecessor.Source.ConditionalSuccessor ? predecessor.Source.ConditionKind.ToString() : "Else";
                }
                var semantics = predecessor.Semantics == ControlFlowBranchSemantics.Regular ? null : predecessor.Semantics.ToString();
                writer.WriteEdge(BlockId(predecessor.Source), BlockId(block), $"{semantics} {condition}".Trim());
                lvaPredecessors.Remove(predecessor.Source);
            }
            foreach (var predecessor in lvaPredecessors)
            {
                writer.WriteEdge(BlockId(predecessor), BlockId(block), "LVA");
            }
            if (block.FallThroughSuccessor is { Destination: null })
            {
                writer.WriteEdge(BlockId(block), "NoDestination_" + BlockId(block), block.FallThroughSuccessor.Semantics.ToString());
            }
        }
    }
}
