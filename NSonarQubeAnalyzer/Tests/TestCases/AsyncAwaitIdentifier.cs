using System.Collections.Generic;

namespace Tests.Diagnostics
{
    public class AsyncAwaitIdentifier
    {
        public AsyncAwaitIdentifier()
        {
            int async = 42; // Noncompliant
            int await = 42; // Noncompliant
        }

        public void Foo(int async) // Noncompliant
        {
            from await in new List<int>() // Noncompliant
            select await; // Noncompliant
        }

        public static async Task<string> Foo(string a)
        {
            return await Foo(a);
        }
    }
}
