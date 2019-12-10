/*
 * Copyright (C) 2018-2019 SonarSource SA
 * All rights reserved
 * mailto:info AT sonarsource DOT com
 */

using System;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace SonarAnalyzer.ControlFlowGraph
{
    public sealed class TemporaryBlock : Block
    {
        public TemporaryBlock()
        {
        }

        public Block SuccessorBlock { get; set; }

        public override IReadOnlyList<Block> SuccessorBlocks => ImmutableArray.Create(SuccessorBlock);

        internal override Block GetPossibleNonEmptySuccessorBlock()
        {
            if (SuccessorBlock == null)
            {
                throw new InvalidOperationException($"{nameof(SuccessorBlock)} is null");
            }

            return SuccessorBlock.GetPossibleNonEmptySuccessorBlock();
        }
    }
}
