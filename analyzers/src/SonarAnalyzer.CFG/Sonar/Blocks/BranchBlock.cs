﻿/*
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
