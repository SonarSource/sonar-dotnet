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
using SonarAnalyzer.CFG.Roslyn;

namespace SonarAnalyzer.SymbolicExecution.Roslyn
{
    internal sealed class FinallyPoint
    {
        private readonly ControlFlowBranch branch;
        private readonly int finallyIndex;

        public ControlFlowGraph Cfg { get; }
        public bool IsFinallyBlock => finallyIndex < branch.FinallyRegions.Length;
        public BasicBlock Block => Cfg.Blocks[IsFinallyBlock ? branch.FinallyRegions[finallyIndex].FirstBlockOrdinal : branch.Destination.Ordinal];

        public FinallyPoint(ControlFlowGraph cfg, ControlFlowBranch branch, int finallyIndex = 0)
        {
            Cfg = cfg ?? throw new ArgumentNullException(nameof(cfg));
            this.branch = branch ?? throw new ArgumentNullException(nameof(branch));
            this.finallyIndex = finallyIndex;
        }

        public FinallyPoint CreateNext() =>
            new(Cfg, branch, finallyIndex + 1);
    }
}
