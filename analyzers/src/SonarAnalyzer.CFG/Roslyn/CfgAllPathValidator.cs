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
        private readonly Dictionary<BasicBlock, bool> visitedStatus = new Dictionary<BasicBlock, bool>();

        protected abstract bool IsValid(BasicBlock block);
        protected abstract bool IsInvalid(BasicBlock block);

        protected CfgAllPathValidator(ControlFlowGraph cfg) =>
            this.cfg = cfg;

        public bool CheckAllPaths()
        {
            var stack = new Stack<BasicBlock>();
            stack.Push(cfg.EntryBlock);
            return IsBlockOrAllSuccessorsValid(stack);
        }

        private bool IsBlockOrAllSuccessorsValid(Stack<BasicBlock> blocks)
        {
            while (blocks.Count > 0)
            {
                var block = blocks.Pop();
                var visitedBlockStatus = visitedStatus.ContainsKey(block) ? visitedStatus[block] : (bool?)null;
                if (IsInvalid(block) || block == cfg.ExitBlock || visitedBlockStatus == false)
                {
                    visitedStatus[block] = false;
                    return false;
                }
                visitedStatus[block] = true;
                if (visitedBlockStatus == true || IsValid(block))
                {
                    return true;
                }
                foreach (var successorBlock in block.SuccessorBlocks)
                {
                    blocks.Push(successorBlock);
                }
            }
            return true;

            //bool AreAllSuccessorsValid(BasicBlock block)
            //{
            //    visitedStatus[block] = true; // protects from loops, don't fail the computation if hits itself
            //    return block.SuccessorBlocks.Any()
            //           && block.SuccessorBlocks.All(x => x != cfg.ExitBlock && (visitedStatus.ContainsKey(x) ? visitedStatus[x] : IsBlockOrAllSuccessorsValid(x)));
            //}
        }
    }
}
