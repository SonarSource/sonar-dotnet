/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2018 SonarSource SA
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
using Microsoft.CodeAnalysis;

namespace SonarAnalyzer.Common
{
    public abstract class CognitiveComplexityWalkerBase : ICognitiveComplexityWalker
    {
        protected readonly List<SecondaryLocation> incrementLocations = new List<SecondaryLocation>();
        // used to track logical operations inside parantheses
        protected readonly List<SyntaxNode> logicalOperationsToIgnore = new List<SyntaxNode>();
        protected bool hasDirectRecursiveCall;

        public int NestingLevel { get; protected set; }

        public int Complexity { get; protected set; }

        public IEnumerable<SecondaryLocation> IncrementLocations => this.incrementLocations;

        public abstract void Visit(SyntaxNode node);

        public void Walk(SyntaxNode node)
        {
            try
            {
                this.Visit(node);
            }
            catch (InsufficientExecutionStackException)
            {
                // TODO: trace this exception

                // Roslyn walker overflows the stack when the depth of the call is around 2050.
                // See ticket #727.

                // Reset nesting level, so the problem with the walker is not reported.
                this.NestingLevel = 0;
            }
        }

        protected void VisitWithNesting<TSyntaxNode>(TSyntaxNode node, Action<TSyntaxNode> visit)
        {
            this.NestingLevel++;
            visit(node);
            this.NestingLevel--;
        }

        protected void IncreaseComplexityByOne(SyntaxToken token)
        {
            IncreaseComplexity(token, 1, "+1");
        }

        protected void IncreaseComplexityByNestingPlusOne(SyntaxToken token)
        {
            var increment = this.NestingLevel + 1;
            var message = increment == 1
                ? "+1"
                : $"+{increment} (incl {increment - 1} for nesting)";
            IncreaseComplexity(token, increment, message);
        }

        protected void IncreaseComplexity(SyntaxToken token, int increment, string message)
        {
            Complexity += increment;
            this.incrementLocations.Add(new SecondaryLocation(token.GetLocation(), message));
        }
    }

    public abstract class CognitiveComplexityWalkerBase<TMethodSyntax> : CognitiveComplexityWalkerBase
        where TMethodSyntax: class
    {
        protected TMethodSyntax currentMethod;
    }
}
