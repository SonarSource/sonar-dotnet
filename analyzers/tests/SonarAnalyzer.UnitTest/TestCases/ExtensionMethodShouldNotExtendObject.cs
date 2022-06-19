using System;

namespace S4225.ExtensionMethodShouldNotExtendObject
{
    static class Compliant
    {
        static void Extends(this int i) { }
        static System.Collections.Generic.IEnumerable<int> GetBaz() { return new[] { 0 }; }
        static void NotAnExtension(object o) { }
    }

    static class NonCompliant
    {
        static void ExtendsObject(this object obj) // Noncompliant {{Refactor this extension to extend a more concrete type.}}
        //          ^^^^^^^^^^^^^
        {
        }
    }
}
