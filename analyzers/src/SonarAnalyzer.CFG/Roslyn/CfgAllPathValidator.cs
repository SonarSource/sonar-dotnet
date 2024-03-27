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

namespace SonarAnalyzer.CFG.Roslyn
{
    public abstract class CfgAllPathValidator
    {
        private readonly ControlFlowGraph cfg;
        private readonly HashSet<BasicBlock> visitedStatus = [];

        protected abstract bool IsValid(BasicBlock block);
        protected abstract bool IsInvalid(BasicBlock block);

        protected CfgAllPathValidator(ControlFlowGraph cfg) =>
            this.cfg = cfg;

        public bool CheckAllPaths()
        {
            var blocks = new Stack<BasicBlock>();
            blocks.Push(cfg.EntryBlock);
            while (blocks.Count > 0)
            {
                var block = blocks.Pop();
                if (IsInvalid(block))
                {
                    return false;
                }
                visitedStatus.Add(block); // protects from loops, don't fail the computation if hits itself
                if (IsValid(block))
                {
                    continue;
                }
                if (block.SuccessorBlocks.IsEmpty)
                {
                    return false;
                }
                foreach (var successorBlock in block.SuccessorBlocks)
                {
                    if (successorBlock == cfg.ExitBlock)
                    {
                        return false;
                    }
                    if (!visitedStatus.Contains(successorBlock))
                    {
                        blocks.Push(successorBlock);
                    }
                }
            }
            return true;
        }
    }
}
