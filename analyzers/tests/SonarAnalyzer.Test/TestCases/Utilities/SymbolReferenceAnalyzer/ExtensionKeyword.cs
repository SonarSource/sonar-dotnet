public static class Extensions
{
    extension(string s)
    {
        public int Length => s.Length;

        public void DoSomething()
        {
            var x = 0;
            x = s.Length;
        }
    }
}
