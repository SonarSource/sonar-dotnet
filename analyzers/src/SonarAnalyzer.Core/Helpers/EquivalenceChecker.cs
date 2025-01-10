/*
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

namespace SonarAnalyzer.Helpers.Common;

public static class EquivalenceChecker
{
    public static bool AreEquivalent(SyntaxNode node1, SyntaxNode node2, Func<SyntaxNode, SyntaxNode, bool> nodeComparator) =>
        node1.Language == node2.Language && nodeComparator(node1, node2);

    public static bool AreEquivalent(SyntaxList<SyntaxNode> nodeList1, SyntaxList<SyntaxNode> nodeList2, Func<SyntaxNode, SyntaxNode, bool> nodeComparator)
    {
        if (nodeList1.Count != nodeList2.Count)
        {
            return false;
        }

        for (var i = 0; i < nodeList1.Count; i++)
        {
            if (!AreEquivalent(nodeList1[i], nodeList2[i], nodeComparator))
            {
                return false;
            }
        }

        return true;
    }
}
