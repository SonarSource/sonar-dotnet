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
    public abstract class CfgAllPathValidator
    {
        enum BlockResult
        {
            INCONCLUSIVE,
            TRUE,
            FALSE,
        }

        private readonly ControlFlowGraph cfg;
        private readonly Dictionary<BasicBlock, BlockResult> lattice = new Dictionary<BasicBlock, BlockResult>();

        protected abstract bool IsValid(BasicBlock block);
        protected abstract bool IsInvalid(BasicBlock block);

        protected CfgAllPathValidator(ControlFlowGraph cfg) =>
            this.cfg = cfg;

        public bool CheckAllPaths() =>
            IsBlockOrAllSuccessorsValid(this.cfg.EntryBlock);

        private bool IsBlockOrAllSuccessorsValid(BasicBlock block)
        {
            var isValid = !IsInvalid(block) && (IsValid(block) || AreAllSuccessorsValid(block));
            lattice[block] = isValid ? BlockResult.TRUE : BlockResult.FALSE;
            return isValid;
        }

        private bool AreAllSuccessorsValid(BasicBlock block)
        {
            lattice[block] = BlockResult.INCONCLUSIVE;
            if (!block.SuccessorBlocks.Contains(cfg.ExitBlock) && block.SuccessorBlocks.Any())
            {
                foreach (var successorBlock in block.SuccessorBlocks)
                {
                    if ((lattice.ContainsKey(successorBlock) && lattice[successorBlock] == BlockResult.FALSE)
                        || (!lattice.ContainsKey(successorBlock) && !IsBlockOrAllSuccessorsValid(successorBlock)))
                    {
                        return false;
                    }
                }
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
