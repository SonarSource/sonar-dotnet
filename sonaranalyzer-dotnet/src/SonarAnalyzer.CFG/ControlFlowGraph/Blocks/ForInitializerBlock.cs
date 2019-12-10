/*
 * Copyright (C) 2018-2019 SonarSource SA
 * All rights reserved
 * mailto:info AT sonarsource DOT com
 */

using System;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace SonarAnalyzer.ControlFlowGraph.CSharp
{
    public sealed class ForInitializerBlock : SimpleBlock
    {
        internal ForInitializerBlock(ForStatementSyntax forNode, Block successor)
            : base(successor)
        {
            ForNode = forNode ?? throw new ArgumentNullException(nameof(forNode));
        }

        public ForStatementSyntax ForNode { get; }

        internal override Block GetPossibleNonEmptySuccessorBlock()
        {
            // This block can't be removed by the CFG simplification, unlike the base class SimpleBlock
            return this;
        }
    }
}
