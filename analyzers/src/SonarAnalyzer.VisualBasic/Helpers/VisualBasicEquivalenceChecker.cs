/*
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

namespace SonarAnalyzer.Helpers
{
    internal static class VisualBasicEquivalenceChecker
    {
        public static bool AreEquivalent(SyntaxNode node1, SyntaxNode node2) =>
            Common.EquivalenceChecker.AreEquivalent(node1, node2,
                (n1, n2) => SyntaxFactory.AreEquivalent(n1, n2));

        public static bool AreEquivalent(SyntaxList<SyntaxNode> nodeList1, SyntaxList<SyntaxNode> nodeList2) =>
            Common.EquivalenceChecker.AreEquivalent(nodeList1, nodeList2,
                (n1, n2) => SyntaxFactory.AreEquivalent(n1, n2));
    }

    internal class VisualBasicSyntaxNodeEqualityComparer<T> : IEqualityComparer<T>, IEqualityComparer<SyntaxList<T>>
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
}
