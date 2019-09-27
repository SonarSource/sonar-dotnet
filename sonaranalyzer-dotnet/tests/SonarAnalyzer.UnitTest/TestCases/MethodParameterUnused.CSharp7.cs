namespace Tests.TestCases
{
    public class BasicTests
    {
        public void Baz()
        {
            this.Foo(new[] { ("some", "thing") });
        }


        // Commented because our CFG builder does not support the following syntax:
        // (see https://github.com/SonarSource/sonar-dotnet/issues/2675)
        /*
        private void Foo((string key, string value)[] bars)
        {
            foreach (var (key, value) in bars)
            { }
        }
        */
        // Meanwhile 'Foo' method is replaced by:
        private void Foo(object arg)
        {
        }

        private void Foo2((string key, string value)[] bars)
        {
            var x = bars;
        }
    }
}
