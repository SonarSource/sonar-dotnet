using System;

namespace Tests.Diagnostics
{
    class Program
    {
        public void MyMethod(string value) { }
        public void MyMethod(string value, StringComparison comparisonType) { }

        void InvalidCalls()
        {
            string.Compare("a", "b"); // Noncompliant {{Change this call to 'string.Compare' to an overload that accepts a 'StringComparison' as a parameter.}}

            string.Equals("a", "b"); // Noncompliant

            MyMethod("a"); // Noncompliant
        }

        void ValidCalls()
        {
            string.Compare("a", "b", StringComparison.OrdinalIgnoreCase);
            string.Equals("a", "b", StringComparison.InvariantCulture);
            MyMethod("a", StringComparison.CurrentCulture);
            "a".IndexOf('#');
        }
    }
}
