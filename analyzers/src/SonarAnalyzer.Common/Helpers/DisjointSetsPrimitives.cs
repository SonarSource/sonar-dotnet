namespace SonarAnalyzer.Helpers;

public static class DisjointSetsPrimitives
{
    public static void Union(IDictionary<string, string> parents, string from, string to) =>
        parents[FindRoot(parents, from)] = FindRoot(parents, to);

    public static string FindRoot(IDictionary<string, string> parents, string element) =>
        parents[element] is var root && root != element ? FindRoot(parents, root) : root;

    public static List<List<string>> DisjointSets(IDictionary<string, string> parents) =>
        parents.GroupBy(x => FindRoot(parents, x.Key), x => x.Key).Select(x => x.OrderBy(x => x).ToList()).OrderBy(x => x[0]).ToList();
}
