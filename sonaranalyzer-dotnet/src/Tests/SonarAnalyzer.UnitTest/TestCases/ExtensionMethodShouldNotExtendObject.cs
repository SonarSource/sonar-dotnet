using System;

namespace Tests.Diagnostics
{
    static class Program
    {
        static void Foo(this object obj) // Noncompliant
//                  ^^^
        {
        }

        static void Bar(this int i) { }
        static System.Collections.Generic.IEnumerable<int> GetBaz() { return new[] { 0 }; }

        static void NotAnExtensionMethod(object o) { }
    }
}
