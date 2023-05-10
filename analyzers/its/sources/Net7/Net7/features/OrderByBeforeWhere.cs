namespace Net7.features
{
    public static class Foo
    {
        public static List<int> Bar()
        {
            var list = new List<int>();
            return list.OrderBy(x => x).Where(x => true).ToList();
        }
    }
}
