namespace CSharpLatest.CSharp11Features;

public static class OrderByBeforeWhere
{
    public static List<int> Method()
    {
        var list = new List<int>();
        return list.OrderBy(x => x).Where(x => true).ToList();
    }
}
