using System;
using System.Text;

namespace Tests.Diagnostics
{
    public class Program
    {
        public static readonly Guid Global = new("54972F01-2A74-4D09-AA7C-359E9C5A5B5A"); // Compliant

        public void Foo()
        {
            Guid g1 = new(); // Noncompliant

            g1 = default; // Noncompliant
//               ^^^^^^^

            StringBuilder st = new(); // Checking that the rules raises issue only for the Guid class.
            string test = default;
        }

        public Guid Get() => default; // Noncompliant

        // See: https://github.com/SonarSource/sonar-dotnet/issues/5245
        private void Test(Guid guid = default) // Compliant
        {
        }
    }
}
