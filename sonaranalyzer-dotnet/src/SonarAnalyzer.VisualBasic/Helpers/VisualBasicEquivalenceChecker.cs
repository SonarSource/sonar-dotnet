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

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.VisualBasic;

namespace SonarAnalyzer.Helpers.VisualBasic
{
    internal static class VisualBasicEquivalenceChecker
    {
        public static bool AreEquivalent(SyntaxNode node1, SyntaxNode node2)
        {
            return Common.EquivalenceChecker.AreEquivalent(node1, node2,
                (n1, n2) => SyntaxFactory.AreEquivalent(n1, n2));
        }

        public static bool AreEquivalent(SyntaxList<SyntaxNode> nodeList1, SyntaxList<SyntaxNode> nodeList2)
        {
            return Common.EquivalenceChecker.AreEquivalent(nodeList1, nodeList2,
                (n1, n2) => SyntaxFactory.AreEquivalent(n1, n2));
        }
    }
}
