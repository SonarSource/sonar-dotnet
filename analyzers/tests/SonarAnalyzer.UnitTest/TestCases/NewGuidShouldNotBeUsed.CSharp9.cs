using System;
using System.Text;

namespace Tests.Diagnostics
{
    public class Program
    {
        public void Foo()
        {
            Guid result = new(); // Noncompliant

            result = default; // Noncompliant

            StringBuilder st = new(); // Checking that the rules raises issue only for the Guid class.
        }
    }
}
