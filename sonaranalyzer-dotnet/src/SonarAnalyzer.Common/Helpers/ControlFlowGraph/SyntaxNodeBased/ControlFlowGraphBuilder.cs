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

namespace SonarAnalyzer.Helpers.FlowAnalysis.Common
{
    internal abstract class ControlFlowGraphBuilder
    {
        private class ControlFlowGraph : IControlFlowGraph
        {
            public IEnumerable<Block> Blocks { get; set; }

            public Block EntryBlock { get; set; }

            public ExitBlock ExitBlock { get; set; }
        }

        protected readonly SyntaxNode node;
        protected readonly SemanticModel semanticModel;

        protected Block currentBlock;
        protected readonly List<Block> reversedBlocks = new List<Block>();

        public IEnumerable<Block> Blocks => reversedBlocks.Reverse<Block>().ToImmutableArray();
        public Block EntryBlock { get; private set; }
        public ExitBlock ExitBlock { get; }

        protected ControlFlowGraphBuilder(SyntaxNode node, SemanticModel semanticModel)
        {
            if (node == null)
            {
                throw new ArgumentNullException(nameof(node));
            }

            if (semanticModel == null)
            {
                throw new ArgumentNullException(nameof(semanticModel));
            }

            this.node = node;
            this.semanticModel = semanticModel;

            ExitBlock = CreateExitBlock();
        }

        internal IControlFlowGraph Build()
        {
            currentBlock = CreateBlock(ExitBlock);

            Build(node);

            EntryBlock = currentBlock;

            PostProcessGraph();

            if (reversedBlocks.OfType<TemporaryBlock>().Any())
            {
                throw new InvalidOperationException("Could not construct valid control flow graph" );
            }

            return new ControlFlowGraph
            {
                Blocks = Blocks,
                EntryBlock = EntryBlock,
                ExitBlock = ExitBlock
            };
        }

        protected virtual void PostProcessGraph()
        {
            RemoveEmptyBlocks();
            ComputePredecessors();
        }

        protected abstract void Build(SyntaxNode node);

        private static readonly ISet<Type> RemovableBlockTypes = ImmutableHashSet.Create(
           typeof(SimpleBlock),
           typeof(TemporaryBlock));

        private void RemoveEmptyBlocks()
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
            if (emptyBlockReplacements.ContainsKey(EntryBlock))
            {
                EntryBlock = emptyBlockReplacements[EntryBlock];
            }
        }

        private void ComputePredecessors()
        {
            foreach (var block in reversedBlocks)
            {
                foreach (var successor in block.SuccessorBlocks)
                {
                    successor.EditablePredecessorBlocks.Add(block);
                }
            }
        }

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
