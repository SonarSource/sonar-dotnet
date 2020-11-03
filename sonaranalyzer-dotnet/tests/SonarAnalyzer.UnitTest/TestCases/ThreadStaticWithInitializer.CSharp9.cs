using System;

record Foo
{
    [ThreadStatic]
    public static object PerThreadObject = new object(); // Noncompliant {{Remove this initialization of 'PerThreadObject' or make it lazy.}}
//                                       ^^^^^^^^^^^^^^

    [ThreadStatic]
    public static object Compliant;
}
