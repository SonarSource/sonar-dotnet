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

namespace SonarAnalyzer.Helpers;

/// <summary>
/// Primitives for working with disjoint sets of strings, to perform union-find operations with equality semantics:
/// i.e. reflexivity, symmetry and transitivity. See https://en.wikipedia.org/wiki/Disjoint-set_data_structure.
///
/// An example of use is to build undirected connected components of dependencies, where each node is the identifier.
///
/// Uses a dictionary of strings as a backing store. The dictionary represents a forest of trees, where each node is
/// a string and each tree is a set of nodes.
/// </summary>
public static class DisjointSetsPrimitives
{
    public static void Union(IDictionary<string, string> parents, string from, string to) =>
        parents[FindRoot(parents, from)] = FindRoot(parents, to);

    public static string FindRoot(IDictionary<string, string> parents, string element) =>
        parents[element] is var root && root != element ? FindRoot(parents, root) : root;

    public static List<List<string>> DisjointSets(IDictionary<string, string> parents) =>
        parents.GroupBy(x => FindRoot(parents, x.Key), x => x.Key).Select(x => x.OrderBy(x => x).ToList()).OrderBy(x => x[0]).ToList();
}
