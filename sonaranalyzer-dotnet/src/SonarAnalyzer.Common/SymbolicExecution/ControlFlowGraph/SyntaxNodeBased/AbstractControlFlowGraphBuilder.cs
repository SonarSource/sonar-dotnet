/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2017 SonarSource SA
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

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace SonarAnalyzer.SymbolicExecution.CFG
{
    internal abstract class AbstractControlFlowGraphBuilder
    {
        private class ControlFlowGraph : IControlFlowGraph
        {
            private static readonly ISet<Type> RemovableBlockTypes = ImmutableHashSet.Create(
                   typeof(SimpleBlock),
                   typeof(TemporaryBlock));

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
            if (node == null)
            {
                throw new ArgumentNullException(nameof(node));
            }

            if (semanticModel == null)
            {
                throw new ArgumentNullException(nameof(semanticModel));
            }

            this.rootNode = node;
            this.semanticModel = semanticModel;

            ExitTarget.Push(CreateExitBlock());
        }
        protected abstract void PostProcessGraph();

        internal IControlFlowGraph Build()
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

        #endregion
    }
}
