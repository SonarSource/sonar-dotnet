/*
 * Copyright (C) 2018-2019 SonarSource SA
 * All rights reserved
 * mailto:info AT sonarsource DOT com
 */

using System;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace SonarAnalyzer.ControlFlowGraph.CSharp
{
    public sealed class ForeachCollectionProducerBlock : SimpleBlock
    {
        internal ForeachCollectionProducerBlock(StatementSyntax foreachNode, Block successor)
            : base(successor)
        {
            ForeachNode = foreachNode ?? throw new ArgumentNullException(nameof(foreachNode));
        }

        public StatementSyntax ForeachNode { get; }

        internal override Block GetPossibleNonEmptySuccessorBlock()
        {
            // This block can't be removed by the CFG simplification, unlike the base class SimpleBlock
            return this;
        }
    }
}
