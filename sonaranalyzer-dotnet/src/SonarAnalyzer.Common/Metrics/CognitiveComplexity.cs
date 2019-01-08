/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2019 SonarSource SA
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
using Microsoft.CodeAnalysis;

namespace SonarAnalyzer.Common
{
    public class CognitiveComplexity
    {
        public CognitiveComplexity(int complexity, ImmutableArray<SecondaryLocation> locations)
        {
            Complexity = complexity;
            Locations = locations;
        }

        public int Complexity { get; }
        public ImmutableArray<SecondaryLocation> Locations { get; }
    }

    public class CognitiveComplexityWalkerState<TMethodSyntax>
        where TMethodSyntax : SyntaxNode
    {
        public TMethodSyntax CurrentMethod { get; set; }

        public List<SecondaryLocation> IncrementLocations { get; }
            = new List<SecondaryLocation>();

        // used to track logical operations inside parentheses
        public List<SyntaxNode> LogicalOperationsToIgnore { get; }
            = new List<SyntaxNode>();

        public bool HasDirectRecursiveCall { get; set; }

        public int NestingLevel { get; set; }

        public int Complexity { get; set; }

        public void VisitWithNesting<TSyntaxNode>(TSyntaxNode node, Action<TSyntaxNode> visit)
        {
            NestingLevel++;
            visit(node);
            NestingLevel--;
        }

        public void IncreaseComplexityByOne(SyntaxToken token)
        {
            IncreaseComplexity(token, 1, "+1");
        }

        public void IncreaseComplexityByNestingPlusOne(SyntaxToken token)
        {
            var increment = NestingLevel + 1;
            var message = increment == 1
                ? "+1"
                : $"+{increment} (incl {increment - 1} for nesting)";
            IncreaseComplexity(token, increment, message);
        }

        public void IncreaseComplexity(SyntaxToken token, int increment, string message)
        {
            Complexity += increment;
            IncrementLocations.Add(new SecondaryLocation(token.GetLocation(), message));
        }
    }
}
