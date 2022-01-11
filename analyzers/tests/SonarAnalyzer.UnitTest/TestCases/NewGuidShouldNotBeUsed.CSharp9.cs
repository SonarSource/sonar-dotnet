using System;
using System.Text;

namespace Tests.Diagnostics
{
    public class Program
    {
        public void Foo()
        {
            Guid g1 = new(); // Noncompliant

            g1 = default; // Noncompliant
//               ^^^^^^^

            StringBuilder st = new(); // Checking that the rules raises issue only for the Guid class.
            string test = default;
        }

        // See: https://github.com/SonarSource/sonar-dotnet/issues/5245
        private void Test(Guid guid = default) // Noncompliant FP - Guid.Empty cannot be used in this case
        {
        }

        public Guid Get() => default; // Noncompliant
    }
}
