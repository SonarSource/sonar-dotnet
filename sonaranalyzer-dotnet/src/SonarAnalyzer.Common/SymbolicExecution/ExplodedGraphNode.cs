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

namespace SonarAnalyzer.SymbolicExecution
{
    internal class ExplodedGraphNode : IEquatable<ExplodedGraphNode>
    {
        public ProgramState ProgramState { get; }
        public ProgramPoint ProgramPoint { get; }

        public ExplodedGraphNode(ProgramPoint programPoint, ProgramState programState)
        {
            ProgramState = programState;
            ProgramPoint = programPoint;
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }

            ExplodedGraphNode n = obj as ExplodedGraphNode;
            return Equals(n);
        }

        public bool Equals(ExplodedGraphNode other)
        {
            if (other == null)
            {
                return false;
            }

            return ProgramState.Equals(other.ProgramState) && ProgramPoint.Equals(other.ProgramPoint);
        }

        public override int GetHashCode()
        {
            var hash = 19;
            hash = hash * 31 + ProgramState.GetHashCode();
            hash = hash * 31 + ProgramPoint.GetHashCode();
            return hash;
        }
    }
}
