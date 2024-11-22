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
