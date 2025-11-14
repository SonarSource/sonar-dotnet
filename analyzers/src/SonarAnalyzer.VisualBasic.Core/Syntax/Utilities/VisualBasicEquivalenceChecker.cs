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

namespace SonarAnalyzer.VisualBasic.Core.Syntax.Utilities;

public static class VisualBasicEquivalenceChecker
{
    public static bool AreEquivalent(SyntaxNode node1, SyntaxNode node2) =>
        EquivalenceChecker.AreEquivalent(node1, node2,
            (n1, n2) => SyntaxFactory.AreEquivalent(n1, n2));

    public static bool AreEquivalent(SyntaxList<SyntaxNode> nodeList1, SyntaxList<SyntaxNode> nodeList2) =>
        EquivalenceChecker.AreEquivalent(nodeList1, nodeList2,
            (n1, n2) => SyntaxFactory.AreEquivalent(n1, n2));
}

public class VisualBasicSyntaxNodeEqualityComparer<T> : IEqualityComparer<T>, IEqualityComparer<SyntaxList<T>>
    where T : SyntaxNode
{
    public bool Equals(T x, T y) =>
        VisualBasicEquivalenceChecker.AreEquivalent(x, y);

    public bool Equals(SyntaxList<T> x, SyntaxList<T> y) =>
        VisualBasicEquivalenceChecker.AreEquivalent(x, y);

    public int GetHashCode(T obj) =>
        obj.GetType().FullName.GetHashCode();

    public int GetHashCode(SyntaxList<T> obj) =>
        (obj.Count + string.Join(", ", obj.Select(x => x.GetType().FullName).Distinct())).GetHashCode();
}
