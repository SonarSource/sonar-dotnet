/*
 * Copyright (C) 2018-2019 SonarSource SA
 * All rights reserved
 * mailto:info AT sonarsource DOT com
 */

using System;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace SonarAnalyzer.ControlFlowGraph.CSharp
{
    public class LockBlock : SimpleBlock
    {
        public LockBlock(LockStatementSyntax lockNode, Block successor)
            : base(successor)
        {
            LockNode = lockNode ?? throw new ArgumentNullException(nameof(lockNode));
        }

        public LockStatementSyntax LockNode { get; }

        internal override Block GetPossibleNonEmptySuccessorBlock()
        {
            // This block can't be removed by the CFG simplification, unlike the base class SimpleBlock
            return this;
        }
    }
}
