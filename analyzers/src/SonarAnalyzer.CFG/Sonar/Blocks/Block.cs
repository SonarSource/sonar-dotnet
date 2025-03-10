﻿/*
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

namespace SonarAnalyzer.CFG.Sonar
{
    /// <summary>
    /// Basic building blocks of a Control Flow Graph (<see cref="IControlFlowGraph"/>).
    /// Holds a list of instructions which have no jumps between them.
    /// </summary>
    public class Block
    {
        private readonly Lazy<IReadOnlyList<SyntaxNode>> instructions;
        private readonly Lazy<IReadOnlyCollection<Block>> predecessorBlocks;
        private readonly Lazy<ISet<Block>> allSuccessors;
        private readonly Lazy<ISet<Block>> allPredecessors;

        // Protected to allow extending and mocking
        protected Block()
        {
            this.instructions = new Lazy<IReadOnlyList<SyntaxNode>>(() => ReversedInstructions.Reverse().ToImmutableArray());
            this.predecessorBlocks = new Lazy<IReadOnlyCollection<Block>>(() => EditablePredecessorBlocks.ToImmutableHashSet());
            this.allSuccessors = new Lazy<ISet<Block>>(() => GetAll(this, b => b.SuccessorBlocks));
            this.allPredecessors = new Lazy<ISet<Block>>(() => GetAll(this, b => b.PredecessorBlocks));
        }

        public virtual IReadOnlyList<SyntaxNode> Instructions => this.instructions.Value;

        public virtual IReadOnlyCollection<Block> PredecessorBlocks => this.predecessorBlocks.Value;

        public virtual IReadOnlyList<Block> SuccessorBlocks { get; } = ImmutableArray.Create<Block>();

        internal IList<SyntaxNode> ReversedInstructions { get; } = new List<SyntaxNode>();

        internal ISet<Block> EditablePredecessorBlocks { get; } = new HashSet<Block>();

        internal virtual Block GetPossibleNonEmptySuccessorBlock()
        {
            return this;
        }

        internal virtual void ReplaceSuccessors(Dictionary<Block, Block> replacementMapping)
        {
        }

        public ISet<Block> AllSuccessorBlocks => this.allSuccessors.Value;

        public ISet<Block> AllPredecessorBlocks => this.allPredecessors.Value;

        private static ISet<Block> GetAll(Block initial, Func<Block, IEnumerable<Block>> getNexts)
        {
            var toProcess = new Queue<Block>();
            var alreadyProcesses = new HashSet<Block>();
            getNexts(initial).ToList().ForEach(b => toProcess.Enqueue(b));
            while (toProcess.Count != 0)
            {
                var current = toProcess.Dequeue();
                if (alreadyProcesses.Contains(current))
                {
                    continue;
                }

                alreadyProcesses.Add(current);

                getNexts(current).ToList().ForEach(b => toProcess.Enqueue(b));
            }

            return alreadyProcesses.ToHashSet();
        }
    }
}
