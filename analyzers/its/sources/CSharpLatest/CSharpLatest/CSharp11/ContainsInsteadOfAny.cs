namespace CSharpLatest.CSharp11
{
    public static class ContainsInsteadOfAny
    {
        public static bool Bar(List<int> list) =>
            list.Any(x => x == 0);
    }
}
