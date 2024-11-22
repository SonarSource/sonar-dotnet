/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2014-2024 SonarSource SA
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
