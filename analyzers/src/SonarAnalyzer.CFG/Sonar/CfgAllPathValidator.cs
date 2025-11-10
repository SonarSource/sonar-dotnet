/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2014-2025 SonarSource Sàrl
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

namespace SonarAnalyzer.CFG.Sonar
{
    public class CfgAllPathValidator
    {
        protected readonly IControlFlowGraph cfg;

        private readonly HashSet<Block> alreadyVisitedBlocks = new HashSet<Block>();

        protected CfgAllPathValidator(IControlFlowGraph cfg)
        {
            this.cfg = cfg;
        }

        public bool CheckAllPaths()
        {
            return IsBlockValidWithSuccessors(this.cfg.EntryBlock);
        }

        private bool IsBlockValidWithSuccessors(Block block)
        {
            return !IsBlockInvalid(block) && (IsBlockValid(block) || AreAllSuccessorsValid(block));
        }

        private bool AreAllSuccessorsValid(Block block)
        {
            this.alreadyVisitedBlocks.Add(block);

            if (block.SuccessorBlocks.Contains(this.cfg.ExitBlock) ||
                !block.SuccessorBlocks.Except(this.alreadyVisitedBlocks).Any())
            {
                return false;
            }

            return block.SuccessorBlocks
                .Except(this.alreadyVisitedBlocks)
                .All(b => IsBlockValidWithSuccessors(b));
        }

        protected virtual bool IsBlockValid(Block block)
        {
            return false;
        }

        protected virtual bool IsBlockInvalid(Block block)
        {
            return false;
        }
    }
}
