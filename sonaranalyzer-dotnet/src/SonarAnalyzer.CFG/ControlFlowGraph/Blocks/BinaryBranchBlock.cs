/*
 * Copyright (C) 2018-2019 SonarSource SA
 * All rights reserved
 * mailto:info AT sonarsource DOT com
 */

using System;
using Microsoft.CodeAnalysis;

namespace SonarAnalyzer.ControlFlowGraph
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

        public Block TrueSuccessorBlock => successors[0];

        public Block FalseSuccessorBlock => successors[1];

        public SyntaxNode Parent => BranchingNode.Parent;
    }
}
