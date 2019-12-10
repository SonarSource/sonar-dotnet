/*
 * Copyright (C) 2018-2019 SonarSource SA
 * All rights reserved
 * mailto:info AT sonarsource DOT com
 */

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace SonarAnalyzer.ControlFlowGraph
{
    public class SimpleBlock : Block
    {
        internal SimpleBlock(Block successor)
        {
            SuccessorBlock = successor ?? throw new ArgumentNullException(nameof(successor));
        }

        public Block SuccessorBlock { get; internal set; }

        public override IReadOnlyList<Block> SuccessorBlocks => ImmutableArray.Create(SuccessorBlock);

        internal override void ReplaceSuccessors(Dictionary<Block, Block> replacementMapping)
        {
            if (replacementMapping.ContainsKey(SuccessorBlock))
            {
                SuccessorBlock = replacementMapping[SuccessorBlock];
            }
        }

        internal override Block GetPossibleNonEmptySuccessorBlock()
        {
            if (ReversedInstructions.Any())
            {
                return this;
            }

            return SuccessorBlock.GetPossibleNonEmptySuccessorBlock();
        }
    }
}
