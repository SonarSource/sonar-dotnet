/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2014-2025 SonarSource Sàrl
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

namespace SonarAnalyzer.Core.Common;

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
