/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2022 SonarSource SA
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

using SonarAnalyzer.CFG.Roslyn;

namespace SonarAnalyzer.SymbolicExecution.Roslyn
{
    public static class ProgramPoint
    {
        private const int BlockIndexBits = 10;  // Hash is 10 bits of block-index followed by 22 bits of instruction-index
        private const int BlockCountMax = 1 << BlockIndexBits;
        private const int BitShift = 32 - BlockIndexBits;

        public static bool HasSupportedSize(ControlFlowGraph cfg) =>
            cfg.Blocks.Length <= BlockCountMax;

        public static int Hash(BasicBlock block, int index) =>
            (int)(((uint)block.Ordinal << BitShift) + (uint)index);
    }
}
