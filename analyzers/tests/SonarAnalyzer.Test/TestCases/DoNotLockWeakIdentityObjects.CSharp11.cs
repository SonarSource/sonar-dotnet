using System;

namespace Tests.Diagnostics
{
    class Program
    {
        private string rawStringLiterals = """some value""";

        private void Test()
        {
            lock (rawStringLiterals) { } // Noncompliant
        }
    }
}
