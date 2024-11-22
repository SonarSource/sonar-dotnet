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
