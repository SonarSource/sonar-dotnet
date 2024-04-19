using static SonarAnalyzer.Helpers.DisjointSetsPrimitives;

namespace SonarAnalyzer.Test.Helpers;

[TestClass]
public class DisjointSetsPrimitivesTest
{
    [TestMethod]
    public void FindRootAndUnion_AreConsistent()
    {
        var parents = Enumerable.Range(1, 5).ToDictionary(i => i.ToString(), i => i.ToString());
        foreach (var item in parents.Keys)
        {
            FindRoot(parents, item).Should().Be(item);
        }

        Union(parents, "1", "1");
        FindRoot(parents, "1").Should().Be("1");                    // Reflexivity
        Union(parents, "1", "2");
        FindRoot(parents, "1").Should().Be(FindRoot(parents, "2")); // Correctness
        Union(parents, "1", "2");
        FindRoot(parents, "1").Should().Be(FindRoot(parents, "2")); // Idempotency
        Union(parents, "2", "1");
        FindRoot(parents, "1").Should().Be(FindRoot(parents, "2")); // Symmetry

        FindRoot(parents, "3").Should().Be("3");
        Union(parents, "2", "3");
        FindRoot(parents, "2").Should().Be(FindRoot(parents, "3"));
        FindRoot(parents, "1").Should().Be(FindRoot(parents, "3")); // Transitivity
        Union(parents, "3", "4");
        FindRoot(parents, "1").Should().Be(FindRoot(parents, "4")); // Double transitivity
        Union(parents, "4", "1");
        FindRoot(parents, "4").Should().Be(FindRoot(parents, "1")); // Idempotency after transitivity
    }

    [TestMethod]
    public void DisjointSetsAndUnion_AreConsistent()
    {
        var parents = Enumerable.Range(1, 5).ToDictionary(i => i.ToString(), i => i.ToString());
        DisjointSets(parents).Should().BeEquivalentTo<List<string>>([["1"], ["2"], ["3"], ["4"], ["5"]]); // Initial state
        Union(parents, "1", "2");
        DisjointSets(parents).Should().BeEquivalentTo<List<string>>([["1", "2"], ["3"], ["4"], ["5"]]);   // Correctness
        Union(parents, "1", "2");
        DisjointSets(parents).Should().BeEquivalentTo<List<string>>([["1", "2"], ["3"], ["4"], ["5"]]);   // Idempotency

        Union(parents, "2", "3");
        DisjointSets(parents).Should().BeEquivalentTo<List<string>>([["1", "2", "3"], ["4"], ["5"]]);
        Union(parents, "1", "3");
        DisjointSets(parents).Should().BeEquivalentTo<List<string>>([["1", "2", "3"], ["4"], ["5"]]);     // Idempotency after transitivity

        Union(parents, "4", "5");
        DisjointSets(parents).Should().BeEquivalentTo<List<string>>([["1", "2", "3"], ["4", "5"]]);       // Separated trees
        Union(parents, "3", "4");
        DisjointSets(parents).Should().BeEquivalentTo<List<string>>([["1", "2", "3", "4", "5"]]);         // Merged trees
    }

    [TestMethod]
    public void DisjointSetsAndUnion_OfNestedTrees()
    {
        var parents = Enumerable.Range(1, 6).ToDictionary(i => i.ToString(), i => i.ToString());
        Union(parents, "1", "2");
        Union(parents, "3", "4");
        Union(parents, "5", "6");
        DisjointSets(parents).Should().BeEquivalentTo<List<string>>([["1", "2"], ["3", "4"], ["5", "6"]]); // Merge of 1-height trees
        Union(parents, "2", "3");
        DisjointSets(parents).Should().BeEquivalentTo<List<string>>([["1", "2", "3", "4"], ["5", "6"]]);   // Merge of 2-height trees
        Union(parents, "4", "5");
        DisjointSets(parents).Should().BeEquivalentTo<List<string>>([["1", "2", "3", "4", "5", "6"]]);     // Merge of 1-height tree and 2-height tree
    }
}
