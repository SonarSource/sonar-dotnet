/*
 * Copyright (C) 2018-2019 SonarSource SA
 * All rights reserved
 * mailto:info AT sonarsource DOT com
 */

using Microsoft.CodeAnalysis;

namespace SonarAnalyzer.ControlFlowGraph
{
    public sealed class BinaryBranchingSimpleBlock : SimpleBlock
    {
        internal BinaryBranchingSimpleBlock(SyntaxNode branchingInstruction, Block trueAndFalseSuccessor)
            : base(trueAndFalseSuccessor)
        {
            BranchingInstruction = branchingInstruction;
        }

        public SyntaxNode BranchingInstruction { get; }
    }
}
