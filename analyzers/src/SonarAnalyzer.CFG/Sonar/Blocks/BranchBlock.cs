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
    public class BranchBlock : Block
    {
        internal BranchBlock(SyntaxNode branchingNode, params Block[] successors)
        {
            this.successors = successors ?? throw new ArgumentNullException(nameof(successors));
            BranchingNode = branchingNode ?? throw new ArgumentNullException(nameof(branchingNode));
        }

        public SyntaxNode BranchingNode { get; }

        protected readonly Block[] successors;

        public override IReadOnlyList<Block> SuccessorBlocks => ImmutableArray.Create(this.successors);

        internal override void ReplaceSuccessors(Dictionary<Block, Block> replacementMapping)
        {
            for (var i = 0; i < this.successors.Length; i++)
            {
                if (replacementMapping.ContainsKey(this.successors[i]))
                {
                    this.successors[i] = replacementMapping[this.successors[i]];
                }
            }
        }
    }
}
