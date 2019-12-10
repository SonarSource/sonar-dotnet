/*
 * Copyright (C) 2018-2019 SonarSource SA
 * All rights reserved
 * mailto:info AT sonarsource DOT com
 */

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;

namespace SonarAnalyzer.ControlFlowGraph
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

        public override IReadOnlyList<Block> SuccessorBlocks => ImmutableArray.Create(successors);

        internal override void ReplaceSuccessors(Dictionary<Block, Block> replacementMapping)
        {
            for (var i = 0; i < successors.Length; i++)
            {
                if (replacementMapping.ContainsKey(successors[i]))
                {
                    successors[i] = replacementMapping[successors[i]];
                }
            }
        }
    }
}
