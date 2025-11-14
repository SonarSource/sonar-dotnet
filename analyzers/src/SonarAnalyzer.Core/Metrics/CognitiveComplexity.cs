/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2014-2025 SonarSource Sàrl
 * mailto:info AT sonarsource DOT com
 * This program is free software; you can redistribute it and/or
 * modify it under the terms of the Sonar Source-Available License Version 1, as published by SonarSource Sàrl.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
 * See the Sonar Source-Available License for more details.
 *
 * You should have received a copy of the Sonar Source-Available License
 * along with this program; if not, see https://sonarsource.com/license/ssal/
 */

namespace SonarAnalyzer.Core.Metrics;

public class CognitiveComplexity
{
    public int Complexity { get; }
    public ImmutableArray<SecondaryLocation> Locations { get; }

    public CognitiveComplexity(int complexity, ImmutableArray<SecondaryLocation> locations)
    {
        Complexity = complexity;
        Locations = locations;
    }
}

public class CognitiveComplexityWalkerState<TMethodSyntax>
    where TMethodSyntax : SyntaxNode
{
    public TMethodSyntax CurrentMethod { get; set; }

    public IList<SecondaryLocation> IncrementLocations { get; } = new List<SecondaryLocation>();

    // used to track logical operations inside parentheses
    public IList<SyntaxNode> LogicalOperationsToIgnore { get; } = new List<SyntaxNode>();

    public bool HasDirectRecursiveCall { get; set; }

    public int NestingLevel { get; set; }

    public int Complexity { get; set; }

    public void VisitWithNesting<TSyntaxNode>(TSyntaxNode node, Action<TSyntaxNode> visit)
    {
        NestingLevel++;
        visit(node);
        NestingLevel--;
    }

    public void IncreaseComplexityByOne(SyntaxToken token) =>
        IncreaseComplexity(token, 1, "+1");

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
