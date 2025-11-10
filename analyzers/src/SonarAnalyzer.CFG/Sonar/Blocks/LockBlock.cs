/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2014-2025 SonarSource Sàrl
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

using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace SonarAnalyzer.CFG.Sonar
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
