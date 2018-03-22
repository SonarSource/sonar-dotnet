using System;

namespace Tests.Diagnostics
{
    public class Program
    {
        public const string NameConst = "foobar"; // Secondary
//                                      ^^^^^^^^
        public static readonly string NameReadonly = "foobar"; // Secondary

        private string name = "foobar"; // Secondary

        private string[] values = new[] { "something", "something", "something" }; // Compliant - repetition below threshold

        private string Name { get; } = "foobar"; // Secondary

        public Program()
        {
            var x = "foobar"; // Secondary

            var y = "FooBar"; // Compliant - casing is different
        }

        public void Do(string s = "foobar") // Secondary
        {
            var x = s ?? "foobar"; // Secondary

            string GetFooBar()
            {
                return "foobar"; // Secondary
            }
        }

        public void Validate(object foobar)
        {
            if (o == null)
            {
                throw new ArgumentNullException("foobar"); // Compliant - matches one of the parameter name
            }

            Do("foobar"); // Compliant - matches one of the parameter name
        }
    }
}
