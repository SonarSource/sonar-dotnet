/*
 * Copyright (C) 2018-2019 SonarSource SA
 * All rights reserved
 * mailto:info AT sonarsource DOT com
 */

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace SonarAnalyzer.ControlFlowGraph
{
    public abstract class AbstractControlFlowGraphBuilder
    {
        private class ControlFlowGraph : IControlFlowGraph
        {
            private static readonly ISet<Type> RemovableBlockTypes = new HashSet<Type>
            {
                   typeof(SimpleBlock),
                   typeof(TemporaryBlock)
            };

            public ControlFlowGraph(List<Block> reversedBlocks, Block entryBlock, ExitBlock exitBlock)
            {
                ExitBlock = exitBlock;
                EntryBlock = RemoveEmptyBlocks(reversedBlocks, entryBlock);

                Blocks = reversedBlocks.Reverse<Block>().ToImmutableArray();

                if (Blocks.OfType<TemporaryBlock>().Any())
                {
                    throw new InvalidOperationException("Could not construct valid control flow graph");
                }

                ComputePredecessors();
            }

            public IEnumerable<Block> Blocks { get; private set; }

            public Block EntryBlock { get; private set; }

            public ExitBlock ExitBlock { get; private set; }

            private static Block RemoveEmptyBlocks(List<Block> reversedBlocks, Block entryBlock)
            {
                var emptyBlockReplacements = new Dictionary<Block, Block>();
                var emptyBlocks = reversedBlocks.Where(b =>
                    RemovableBlockTypes.Contains(b.GetType()) &&
                    !b.ReversedInstructions.Any());

                foreach (var block in emptyBlocks)
                {
                    var replacementBlock = block.GetPossibleNonEmptySuccessorBlock();
                    if (replacementBlock != block)
                    {
                        emptyBlockReplacements.Add(block, replacementBlock);
                    }
                }

                // Remove empty blocks
                reversedBlocks.RemoveAll(b => emptyBlockReplacements.Keys.Contains(b));

                // Replace successors
                foreach (var block in reversedBlocks)
                {
                    block.ReplaceSuccessors(emptyBlockReplacements);
                }

                // Fix entry block
                var newEntryBlock = entryBlock;
                if (emptyBlockReplacements.ContainsKey(entryBlock))
                {
                    newEntryBlock = emptyBlockReplacements[entryBlock];
                }

                return newEntryBlock;
            }

            private void ComputePredecessors()
            {
                foreach (var block in Blocks)
                {
                    foreach (var successor in block.SuccessorBlocks)
                    {
                        successor.EditablePredecessorBlocks.Add(block);
                    }
                }
            }
        }

        protected readonly SyntaxNode rootNode;
        protected readonly SemanticModel semanticModel;

        protected readonly List<Block> reversedBlocks = new List<Block>();

        protected readonly Stack<Block> ExitTarget = new Stack<Block>();

        protected AbstractControlFlowGraphBuilder(SyntaxNode node, SemanticModel semanticModel)
        {
            rootNode = node ?? throw new ArgumentNullException(nameof(node));
            this.semanticModel = semanticModel ?? throw new ArgumentNullException(nameof(semanticModel));

            ExitTarget.Push(CreateExitBlock());
        }

        protected abstract void PostProcessGraph();

        public IControlFlowGraph Build()
        {
            var entryBlock = Build(rootNode, CreateBlock(ExitTarget.Peek()));
            PostProcessGraph();

            return new ControlFlowGraph(reversedBlocks, entryBlock, (ExitBlock)ExitTarget.Pop());
        }

        protected abstract Block Build(SyntaxNode node, Block currentBlock);

        #region CreateBlock*

        internal BinaryBranchBlock CreateBinaryBranchBlock(SyntaxNode branchingNode, Block trueSuccessor, Block falseSuccessor) =>
            AddBlock(new BinaryBranchBlock(branchingNode, trueSuccessor, falseSuccessor));

        internal SimpleBlock CreateBlock(Block successor) =>
            AddBlock(new SimpleBlock(successor));

        internal JumpBlock CreateJumpBlock(SyntaxNode jumpStatement, Block successor, Block wouldBeSuccessor = null) =>
            AddBlock(new JumpBlock(jumpStatement, successor, wouldBeSuccessor));

        internal BranchBlock CreateBranchBlock(SyntaxNode branchingNode, IEnumerable<Block> successors) =>
            AddBlock(new BranchBlock(branchingNode, successors.ToArray()));

        private ExitBlock CreateExitBlock() => AddBlock(new ExitBlock());

        internal TemporaryBlock CreateTemporaryBlock() => AddBlock(new TemporaryBlock());

        internal T AddBlock<T>(T block)
            where T : Block
        {
            reversedBlocks.Add(block);
            return block;
        }

        #endregion CreateBlock*
    }
}
