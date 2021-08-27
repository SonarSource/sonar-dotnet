/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2021 SonarSource SA
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

using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using SonarAnalyzer.CFG.Helpers;
using SonarAnalyzer.CFG.Sonar;

namespace SonarAnalyzer.CFG
{
    public partial class CfgSerializer
    {
        private class SonarCfgWalker
        {
            private readonly BlockIdProvider blockId = new BlockIdProvider();
            private readonly DotWriter writer;

            public SonarCfgWalker(DotWriter writer) =>
                this.writer = writer;

            public void Visit(string methodName, IControlFlowGraph cfg)
            {
                writer.WriteGraphStart(methodName, false);
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
                if (terminator != null)
                {
                    header += ":" + terminator.Kind().ToString().Replace("Syntax", string.Empty);
                }
                writer.WriteNode(blockId.Get(block), header, block.Instructions.Select(i => i.ToString()).ToArray());
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
}
