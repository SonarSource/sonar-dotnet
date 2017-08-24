/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2017 SonarSource SA
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
using Microsoft.CodeAnalysis;

namespace SonarAnalyzer.SymbolicExecution.CFG
{
    internal class CfgAllPathValidator
    {
        protected readonly IControlFlowGraph cfg;

        private readonly HashSet<Block> alreadyVisitedBlocks = new HashSet<Block>();

        protected CfgAllPathValidator(IControlFlowGraph cfg)
        {
            this.cfg = cfg;
        }

        public bool CheckAllPaths()
        {
            return IsBlockValidWithSuccessors(cfg.EntryBlock);
        }

        private bool IsBlockValidWithSuccessors(Block block)
        {
            return !IsBlockInvalid(block) && (IsBlockValid(block) || AreAllSuccessorsValid(block));
        }

        private bool AreAllSuccessorsValid(Block block)
        {
            alreadyVisitedBlocks.Add(block);

            if (block.SuccessorBlocks.Contains(cfg.ExitBlock) ||
                !block.SuccessorBlocks.Except(alreadyVisitedBlocks).Any())
            {
                return false;
            }

            return block.SuccessorBlocks
                .Except(alreadyVisitedBlocks)
                .All(b => IsBlockValidWithSuccessors(b));
        }

        protected virtual bool IsBlockValid(Block block) { return false; }

        protected virtual bool IsBlockInvalid(Block block) { return false; }
    }
}
