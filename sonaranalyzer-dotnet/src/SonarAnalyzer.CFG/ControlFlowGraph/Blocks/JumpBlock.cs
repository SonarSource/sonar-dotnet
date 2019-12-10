/*
 * Copyright (C) 2018-2019 SonarSource SA
 * All rights reserved
 * mailto:info AT sonarsource DOT com
 */

using System;
using System.Collections.Generic;
using Microsoft.CodeAnalysis;

namespace SonarAnalyzer.ControlFlowGraph
{
    public sealed class JumpBlock : SimpleBlock
    {
        internal JumpBlock(SyntaxNode jumpNode, Block successor, Block wouldBeSuccessor)
            : base(successor)
        {
            JumpNode = jumpNode ?? throw new ArgumentNullException(nameof(jumpNode));
            WouldBeSuccessor = wouldBeSuccessor;
        }

        public SyntaxNode JumpNode { get; }

        /// <summary>
        /// If there was no jump, this block would be the successor.
        /// It can be null, when it doesn't make sense. For example in case of lock statements.
        /// </summary>
        public Block WouldBeSuccessor { get; private set; }

        internal override Block GetPossibleNonEmptySuccessorBlock()
        {
            // JumpBlock can't be removed by the CFG simplification, unlike the base class SimpleBlock
            return this;
        }

        internal override void ReplaceSuccessors(Dictionary<Block, Block> replacementMapping)
        {
            base.ReplaceSuccessors(replacementMapping);

            if (WouldBeSuccessor != null && replacementMapping.ContainsKey(WouldBeSuccessor))
            {
                WouldBeSuccessor = replacementMapping[WouldBeSuccessor];
            }
        }
    }
}
