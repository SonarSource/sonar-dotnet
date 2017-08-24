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

using Microsoft.CodeAnalysis;

namespace SonarAnalyzer.SymbolicExecution
{
    internal class ExplodedGraphCheck
    {
        protected readonly AbstractExplodedGraph explodedGraph;
        protected readonly SemanticModel semanticModel;

        protected ExplodedGraphCheck(AbstractExplodedGraph explodedGraph)
        {
            this.explodedGraph = explodedGraph;
            this.semanticModel = explodedGraph.SemanticModel;
        }

        public virtual ProgramState PreProcessInstruction(ProgramPoint programPoint, ProgramState programState)
        {
            return programState;
        }

        public virtual ProgramState PreProcessUsingStatement(ProgramPoint programPoint, ProgramState programState)
        {
            return programState;
        }

        public virtual ProgramState ObjectCreated(ProgramState programState, SymbolicValue symbolicValue, SyntaxNode instruction)
        {
            return programState;
        }

        public virtual ProgramState ObjectCreating(ProgramState programState, SyntaxNode instruction)
        {
            return programState;
        }
    }
}
