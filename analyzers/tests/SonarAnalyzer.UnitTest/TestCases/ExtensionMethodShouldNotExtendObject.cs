using System;

static class Compliant
{
    static void ExtendsValueType(this int i) { }
    static int ExtendsWithArguments(this int i, int n)
    {
        return i + n;
    }
    static void ExtendsReferenceType(this Exception i) { }

    static void NotAnExtension(object o) { }
}

static class NonCompliant
{
    static void ExtendsObject(this object obj) // Noncompliant {{Refactor this extension to extend a more concrete type.}}
    //          ^^^^^^^^^^^^^
    {
    }

    static void ExtendsWithArguments(this object obj, int other) { } // Noncompliant
}
