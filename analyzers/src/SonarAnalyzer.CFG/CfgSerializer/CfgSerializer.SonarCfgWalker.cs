/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2014-2025 SonarSource SA
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

using Microsoft.CodeAnalysis.CSharp;
using SonarAnalyzer.CFG.Sonar;

namespace SonarAnalyzer.CFG;

public static partial class CfgSerializer
{
    private sealed class SonarCfgWalker
    {
        private readonly BlockIdProvider blockId = new();
        private readonly DotWriter writer;

        public SonarCfgWalker(DotWriter writer) =>
            this.writer = writer;

        public void Visit(IControlFlowGraph cfg, string title)
        {
            writer.WriteGraphStart(title);
            foreach (var block in cfg.Blocks)
            {
                Visit(block);
            }
            writer.WriteGraphEnd();
        }

        private void Visit(Block block)
        {
            if (block is BinaryBranchBlock binaryBranchBlock)
            {
                WriteNode(block, binaryBranchBlock.BranchingNode);
            }
            else if (block is BranchBlock branchBlock)
            {
                WriteNode(block, branchBlock.BranchingNode);
            }
            else if (block is ExitBlock)
            {
                WriteNode(block);
            }
            else if (block is ForeachCollectionProducerBlock foreachBlock)
            {
                WriteNode(foreachBlock, foreachBlock.ForeachNode);
            }
            else if (block is ForInitializerBlock forBlock)
            {
                WriteNode(forBlock, forBlock.ForNode);
            }
            else if (block is JumpBlock jumpBlock)
            {
                WriteNode(jumpBlock, jumpBlock.JumpNode);
            }
            else if (block is LockBlock lockBlock)
            {
                WriteNode(lockBlock, lockBlock.LockNode);
            }
            else if (block is UsingEndBlock usingBlock)
            {
                WriteNode(usingBlock, usingBlock.UsingStatement);
            }
            else
            {
                WriteNode(block);
            }
            WriteEdges(block);
        }

        private void WriteNode(Block block, SyntaxNode terminator = null)
        {
            var header = block.GetType().Name.SplitCamelCaseToWords().First().ToUpperInvariant();
            if (terminator is not null)
            {
                header += ":" + terminator.Kind().ToString().Replace("Syntax", string.Empty);
            }
            writer.WriteRecordNode(blockId.Get(block), header, block.Instructions.Select(x => x.ToString()).ToArray());
        }

        private void WriteEdges(Block block)
        {
            foreach (var successor in block.SuccessorBlocks)
            {
                writer.WriteEdge(blockId.Get(block), blockId.Get(successor), Label());

                string Label()
                {
                    if (block is BinaryBranchBlock binary)
                    {
                        if (successor == binary.TrueSuccessorBlock)
                        {
                            return bool.TrueString;
                        }
                        else if (successor == binary.FalseSuccessorBlock)
                        {
                            return bool.FalseString;
                        }
                    }
                    return string.Empty;
                }
            }
        }
    }
}
