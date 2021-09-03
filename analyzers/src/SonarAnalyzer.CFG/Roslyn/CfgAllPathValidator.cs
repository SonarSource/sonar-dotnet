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

using System.Collections.Generic;
using System.Linq;

namespace SonarAnalyzer.CFG.Roslyn
{
    public class CfgAllPathValidator
    {
        private readonly ControlFlowGraph cfg;
        private readonly HashSet<BasicBlock> visited = new HashSet<BasicBlock>();

        protected CfgAllPathValidator(ControlFlowGraph cfg) =>
            this.cfg = cfg;

        protected virtual bool IsBlockValid(BasicBlock block) => false;

        protected virtual bool IsBlockInvalid(BasicBlock block) => false;

        protected bool CheckAllPaths() =>
            IsBlockOrAllSuccessorsValid(this.cfg.EntryBlock);

        private bool IsBlockOrAllSuccessorsValid(BasicBlock block) =>
            !IsBlockInvalid(block) && (IsBlockValid(block) || AreAllSuccessorsValid(block));

        private bool AreAllSuccessorsValid(BasicBlock block)
        {
            this.visited.Add(block);

            return !block.SuccessorBlocks.Contains(cfg.ExitBlock)
                   && block.SuccessorBlocks.Except(visited).Any()
                   && block.SuccessorBlocks.Except(visited).All(IsBlockOrAllSuccessorsValid);
        }
    }
}
