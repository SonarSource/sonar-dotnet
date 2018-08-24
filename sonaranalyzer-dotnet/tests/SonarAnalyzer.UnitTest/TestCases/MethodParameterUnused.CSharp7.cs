namespace Tests.TestCases
{
    public class BasicTests
    {
        public void Baz()
        {
            this.Foo(new[] { ("some", "thing") });
        }

        private void Foo((string key, string value)[] bars)
        {
            foreach (var (key, value) in bars)
            { }
        }

        private void Foo2((string key, string value)[] bars)
        {
            var x = bars;
        }
    }
}
