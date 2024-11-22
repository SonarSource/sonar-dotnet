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
            foreach (var predecessor in lva.BlockPredecessors[block.Ordinal].Where(x => !block.Predecessors.Any(y => y.Source == x)))
            {
                writer.WriteEdge(BlockId(predecessor), BlockId(block), "LVA\" fontcolor=\"blue\" penwidth=\"2\" color=\"blue");
            }
            base.WriteEdges(block);
        }
    }
}
