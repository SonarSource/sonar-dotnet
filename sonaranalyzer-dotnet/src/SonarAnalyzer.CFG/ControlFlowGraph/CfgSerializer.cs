/*
 * Copyright (C) 2018-2019 SonarSource SA
 * All rights reserved
 * mailto:info AT sonarsource DOT com
 */

using System;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using SonarAnalyzer.Helpers;

namespace SonarAnalyzer.ControlFlowGraph.CSharp
{
    public static class CfgSerializer
    {
        public static string Serialize(string methodName, IControlFlowGraph cfg)
        {
            var stringBuilder = new StringBuilder();
            using (var writer = new StringWriter(stringBuilder))
            {
                Serialize(methodName, cfg, writer);
            }
            return stringBuilder.ToString();
        }

        public static void Serialize(string methodName, IControlFlowGraph cfg, TextWriter writer)
        {
            new CfgWalker(new DotWriter(writer)).Visit(methodName, cfg);
        }

        private class CfgWalker
        {
            private readonly BlockIdProvider _blockId = new BlockIdProvider();
            private readonly DotWriter _writer;

            public CfgWalker(DotWriter writer)
            {
                _writer = writer;
            }

            public void Visit(string methodName, IControlFlowGraph cfg)
            {
                _writer.WriteGraphStart(methodName);

                foreach (var block in cfg.Blocks)
                {
                    Visit(block);
                }

                _writer.WriteGraphEnd();
            }

            private void Visit(Block block)
            {
                Func<Block, string> getLabel = b => string.Empty;

                if (block is BinaryBranchBlock binaryBranchBlock)
                {
                    WriteNode(block, binaryBranchBlock.BranchingNode);
                    // Add labels to the binary branch block successors
                    getLabel = b =>
                    {
                        if (b == binaryBranchBlock.TrueSuccessorBlock)
                        {
                            return bool.TrueString;
                        }
                        else if (b == binaryBranchBlock.FalseSuccessorBlock)
                        {
                            return bool.FalseString;
                        }
                        return string.Empty;
                    };
                }
                else if (block is BranchBlock branchBlock)
                {
                    WriteNode(block, branchBlock.BranchingNode);
                }
                else if (block is ExitBlock exitBlock)
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
                WriteEdges(block, getLabel);
            }

            private void WriteNode(Block block, SyntaxNode terminator = null)
            {
                var header = block.GetType().Name.SplitCamelCaseToWords().First().ToUpperInvariant();
                if (terminator != null)
                {
                    // shorten the text
                    var terminatorType = terminator.Kind().ToString().Replace("Syntax", string.Empty);

                    header += ":" + terminatorType;
                }
                _writer.WriteNode(_blockId.Get(block), header, block.Instructions.Select(i => i.ToString()).ToArray());
            }

            private void WriteEdges(Block block, Func<Block, string> getLabel)
            {
                foreach (var successor in block.SuccessorBlocks)
                {
                    _writer.WriteEdge(_blockId.Get(block), _blockId.Get(successor), getLabel(successor));
                }
            }
        }
    }
}
