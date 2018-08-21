namespace Tests.TestCases
{
    public class BasicTests
    {
        public void Baz()
        {
            this.Foo(new[] { ("some", "thing") });
        }

        private void Foo((string key, string value)[] bars) // Noncompliant - FP - we need a higher version of Roslyn in the tests to have the right symbol
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
