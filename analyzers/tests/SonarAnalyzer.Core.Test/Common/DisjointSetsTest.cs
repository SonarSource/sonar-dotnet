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

namespace SonarAnalyzer.Core.Test.Common;

[TestClass]
public class DisjointSetsTest
{
    private static readonly string[] FirstSixPositiveInts = Enumerable.Range(1, 6).Select(x => x.ToString()).ToArray();

    [TestMethod]
    public void FindRootAndUnion_AreConsistent()
    {
        var sets = new DisjointSets(FirstSixPositiveInts);
        foreach (var element in FirstSixPositiveInts)
        {
            sets.FindRoot(element).Should().Be(element);
        }

        sets.Union("1", "1");
        sets.FindRoot("1").Should().Be("1");                // Reflexivity
        sets.Union("1", "2");
        sets.FindRoot("1").Should().Be(sets.FindRoot("2")); // Correctness
        sets.Union("1", "2");
        sets.FindRoot("1").Should().Be(sets.FindRoot("2")); // Idempotency
        sets.Union("2", "1");
        sets.FindRoot("1").Should().Be(sets.FindRoot("2")); // Symmetry

        sets.FindRoot("3").Should().Be("3");
        sets.Union("2", "3");
        sets.FindRoot("2").Should().Be(sets.FindRoot("3"));
        sets.FindRoot("1").Should().Be(sets.FindRoot("3")); // Transitivity
        sets.Union("3", "4");
        sets.FindRoot("1").Should().Be(sets.FindRoot("4")); // Double transitivity
        sets.Union("4", "1");
        sets.FindRoot("4").Should().Be(sets.FindRoot("1")); // Idempotency after transitivity
    }

    [TestMethod]
    public void GetAllSetsAndUnion_AreConsistent()
    {
        var sets = new DisjointSets(FirstSixPositiveInts);
        AssertSets([["1"], ["2"], ["3"], ["4"], ["5"], ["6"]], sets); // Initial state
        sets.Union("1", "2");
        AssertSets([["1", "2"], ["3"], ["4"], ["5"], ["6"]], sets);   // Correctness
        sets.Union("1", "2");
        AssertSets([["1", "2"], ["3"], ["4"], ["5"], ["6"]], sets);   // Idempotency

        sets.Union("2", "3");
        AssertSets([["1", "2", "3"], ["4"], ["5"], ["6"]], sets);     // Transitivity
        sets.Union("1", "3");
        AssertSets([["1", "2", "3"], ["4"], ["5"], ["6"]], sets);     // Idempotency after transitivity

        sets.Union("4", "5");
        AssertSets([["1", "2", "3"], ["4", "5"], ["6"]], sets);       // Separated trees
        sets.Union("3", "4");
        AssertSets([["1", "2", "3", "4", "5"], ["6"]], sets);         // Merged trees
    }

    [TestMethod]
    public void GetAllSetsAndUnion_OfNestedTrees()
    {
        var sets = new DisjointSets(FirstSixPositiveInts);
        sets.Union("1", "2");
        sets.Union("3", "4");
        sets.Union("5", "6");
        AssertSets([["1", "2"], ["3", "4"], ["5", "6"]], sets); // Merge of 1-height trees
        sets.Union("2", "3");
        AssertSets([["1", "2", "3", "4"], ["5", "6"]], sets);   // Merge of 2-height trees
        sets.Union("4", "5");
        AssertSets([["1", "2", "3", "4", "5", "6"]], sets);     // Merge of 1-height tree and 2-height tree
    }

    [TestMethod]
    public void GetAllSets_ReturnsSortedSets()
    {
        var sets = new DisjointSets(["3", "2", "1"]);
        AssertSets([["1"], ["2"], ["3"]], sets);
        sets.Union("3", "1");
        AssertSets([["1", "3"], ["2"]], sets);
    }

    private static void AssertSets(List<List<string>> expected, DisjointSets sets) =>
        sets.GetAllSets().Should().BeEquivalentTo(expected, x => x.WithStrictOrdering());
}
