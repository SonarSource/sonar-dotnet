using System;

namespace Tests.Diagnostics
{
    public class Program
    {
        public const string NameConst = "foobar"; // Noncompliant {{Define a constant instead of using this literal 'foobar' 8 times.}}
//                                      ^^^^^^^^
        public static readonly string NameReadonly = "foobar";
//                                                   ^^^^^^^^ Secondary


        private string name = "foobar";
//                            ^^^^^^^^ Secondary

        private string[] values = new[] { "something", "something", "something" }; // Compliant - repetition below threshold

        private string Name { get; } = "foobar";
//                                     ^^^^^^^^ Secondary

        public Program()
        {
            var x = "foobar";
//                  ^^^^^^^^ Secondary

            var y = "FooBar"; // Compliant - casing is different
        }

        public void Do(string s = "foobar")
//                                ^^^^^^^^ Secondary
        {
            var x = s ?? "foobar";
//                       ^^^^^^^^ Secondary

            string GetFooBar()
            {
                return "foobar";
//                     ^^^^^^^^ Secondary
            }
        }

        public void Validate(object foobar)
        {
            if (foobar == null)
            {
                throw new ArgumentNullException("foobar"); // Compliant - matches one of the parameter name
            }

            Do("foobar"); // Compliant - matches one of the parameter name
        }
    }

    public struct Foo
    {
        public const string NameConst = "foobar"; // Noncompliant {{Define a constant instead of using this literal 'foobar' 4 times.}}
//                                      ^^^^^^^^

        private string name1 = "foobar"; // Secondary
        private string name2 = "foobar"; // Secondary
        private string name3 = "foobar"; // Secondary
    }

    public class OuterClass
    {
        private string Name { get; } = "foobar"; // Noncompliant

        private class InnerClass
        {
            private string name1 = "foobar"; // Secondary - inner class count with base
            private string name2 = "foobar"; // Secondary
            private string name3 = "foobar"; // Secondary
            private string name4 = "foobar"; // Secondary
        }

        private struct InnerStruct
        {
            private string name1 = "foobar"; // Secondary - inner struct count with base
            private string name2 = "foobar"; // Secondary
            private string name3 = "foobar"; // Secondary
            private string name4 = "foobar"; // Secondary
        }
    }
}
