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

namespace SonarAnalyzer.Common;

/// <summary>
/// Data structure for working with disjoint sets of strings, to perform union-find operations with equality semantics:
/// i.e. reflexivity, symmetry and transitivity.
///
/// See https://en.wikipedia.org/wiki/Disjoint-set_data_structure for an introduction to the data structure and
/// https://www.geeksforgeeks.org/introduction-to-disjoint-set-data-structure-or-union-find-algorithm/ for examples of
/// its use.
///
/// An example of use is to build undirected connected components of dependencies, where each node is the identifier.
///
/// Uses a dictionary of strings as a backing store. The dictionary represents a forest of trees, where each node is
/// a string and each tree is a set of nodes.
/// </summary>
public class DisjointSets
{
    private readonly Dictionary<string, string> parents;

    public DisjointSets(IEnumerable<string> elements) =>
        parents = elements.ToDictionary(x => x, x => x);

    public void Union(string from, string to) =>
        parents[FindRoot(from)] = FindRoot(to);

    public string FindRoot(string element) =>
        parents[element] is var root && root != element ? FindRoot(root) : root;

    // Set elements are sorted in ascending order. Sets are sorted in ascending order by their first element.
    public List<List<string>> GetAllSets() =>
        [.. parents.GroupBy(x => FindRoot(x.Key), x => x.Key).Select(x => x.OrderBy(x => x).ToList()).OrderBy(x => x[0])];
}
