namespace SonarAnalyzer.Test.Common;

[TestClass]
public class DisjointSetsTest
{
    [TestMethod]
    public void FindRootAndUnion_AreConsistent()
    {
        var elements = Enumerable.Range(1, 5).Select(x => x.ToString());
        var sets = new DisjointSets(elements);
        foreach (var element in elements)
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
        var sets = new DisjointSets(Enumerable.Range(1, 5).Select(x => x.ToString()));
        AssertSets([["1"], ["2"], ["3"], ["4"], ["5"]], sets); // Initial state
        sets.Union("1", "2");
        AssertSets([["1", "2"], ["3"], ["4"], ["5"]], sets);   // Correctness
        sets.Union("1", "2");
        AssertSets([["1", "2"], ["3"], ["4"], ["5"]], sets);   // Idempotency

        sets.Union("2", "3");
        AssertSets([["1", "2", "3"], ["4"], ["5"]], sets);     // Transitivity
        sets.Union("1", "3");
        AssertSets([["1", "2", "3"], ["4"], ["5"]], sets);     // Idempotency after transitivity

        sets.Union("4", "5");
        AssertSets([["1", "2", "3"], ["4", "5"]], sets);       // Separated trees
        sets.Union("3", "4");
        AssertSets([["1", "2", "3", "4", "5"]], sets);         // Merged trees
    }

    [TestMethod]
    public void GetAllSetsAndUnion_OfNestedTrees()
    {
        var sets = new DisjointSets(Enumerable.Range(1, 6).Select(x => x.ToString()));
        sets.Union("1", "2");
        sets.Union("3", "4");
        sets.Union("5", "6");
        AssertSets([["1", "2"], ["3", "4"], ["5", "6"]], sets); // Merge of 1-height trees
        sets.Union("2", "3");
        AssertSets([["1", "2", "3", "4"], ["5", "6"]], sets);   // Merge of 2-height trees
        sets.Union("4", "5");
        AssertSets([["1", "2", "3", "4", "5", "6"]], sets);     // Merge of 1-height tree and 2-height tree
    }

    private static void AssertSets(List<List<string>> expected, DisjointSets sets) =>
      sets.GetAllSets().Should().BeEquivalentTo(expected);
}
