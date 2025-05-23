﻿/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2024 SonarSource SA
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

using SonarAnalyzer.CFG.Sonar;

namespace SonarAnalyzer.SymbolicExecution.Sonar
{
    public sealed class ProgramPoint : IEquatable<ProgramPoint>
    {
        public Block Block { get; }
        public int Offset { get; }

        private int? hash;

        internal ProgramPoint(Block block, int offset)
        {
            Block = block;
            Offset = offset;
        }

        internal ProgramPoint(Block block)
            : this(block, 0)
        { }

        public SyntaxNode CurrentInstruction =>
            Block.Instructions[Offset];

        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }

            var p = obj as ProgramPoint;
            return Equals(p);
        }

        public bool Equals(ProgramPoint other)
        {
            if (other == null)
            {
                return false;
            }

            return Block == other.Block && Offset == other.Offset;
        }

        public override int GetHashCode() =>
            hash ??= ComputeHash();

        private int ComputeHash()
        {
            var h = 19;
            h = h * 31 + Block.GetHashCode();
            h = h * 31 + Offset.GetHashCode();
            return h;
        }
    }
}
