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
    public class BinaryBranchBlock : BranchBlock
    {
        internal BinaryBranchBlock(SyntaxNode branchingNode, Block trueSuccessor, Block falseSuccessor)
            : base(branchingNode, trueSuccessor, falseSuccessor)
        {
            if (trueSuccessor == null)
            {
                throw new ArgumentNullException(nameof(trueSuccessor));
            }

            if (falseSuccessor == null)
            {
                throw new ArgumentNullException(nameof(falseSuccessor));
            }
        }

        public Block TrueSuccessorBlock => this.successors[0];

        public Block FalseSuccessorBlock => this.successors[1];

        public SyntaxNode Parent => BranchingNode.Parent;
    }
}
