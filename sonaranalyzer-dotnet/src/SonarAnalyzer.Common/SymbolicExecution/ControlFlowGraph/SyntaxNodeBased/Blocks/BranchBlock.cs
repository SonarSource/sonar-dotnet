/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2017 SonarSource SA
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

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;

namespace SonarAnalyzer.SymbolicExecution.CFG
{
    public class BranchBlock : Block
    {
        internal BranchBlock(SyntaxNode branchingNode, params Block[] successors)
        {
            if (branchingNode == null)
            {
                throw new ArgumentNullException(nameof(branchingNode));
            }

            if (successors == null)
            {
                throw new ArgumentNullException(nameof(successors));
            }

            this.successors = successors;
            BranchingNode = branchingNode;
        }

        public SyntaxNode BranchingNode { get; }

        protected readonly Block[] successors;

        public override IReadOnlyList<Block> SuccessorBlocks => ImmutableArray.Create(successors);

        internal override void ReplaceSuccessors(Dictionary<Block, Block> replacementMapping)
        {
            for (int i = 0; i < successors.Length; i++)
            {
                if (replacementMapping.ContainsKey(successors[i]))
                {
                    successors[i] = replacementMapping[successors[i]];
                }
            }
        }
    }
}
