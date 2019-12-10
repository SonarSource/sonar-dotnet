/*
 * Copyright (C) 2018-2019 SonarSource SA
 * All rights reserved
 * mailto:info AT sonarsource DOT com
 */

using System.Collections.Generic;
using System.Linq;

namespace SonarAnalyzer.ControlFlowGraph.CSharp
{
    public class CfgAllPathValidator
    {
        protected readonly IControlFlowGraph cfg;

        private readonly HashSet<Block> _alreadyVisitedBlocks = new HashSet<Block>();

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
            _alreadyVisitedBlocks.Add(block);

            if (block.SuccessorBlocks.Contains(cfg.ExitBlock) ||
                !block.SuccessorBlocks.Except(_alreadyVisitedBlocks).Any())
            {
                return false;
            }

            return block.SuccessorBlocks
                .Except(_alreadyVisitedBlocks)
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
