﻿lock ("a string") { }              // Noncompliant
//    ^^^^^^^^^^
lock (MethodReturningString()) { } // Noncompliant
lock (new object()) { }            // Noncompliant
lock (staticReadonlyField) { }
lock (staticReadWriteField) {}     // Noncompliant

string MethodReturningString() => "a string";

partial class Program
{
    static readonly object staticReadonlyField = new();
    static object staticReadWriteField = new();
}

class ClassNestedAtTopLevel
{
    readonly object readonlyField = new();
    object readWriteField = new();

    static readonly object staticReadonlyField = new();
    static object staticReadWriteField = new();

    void Test()
    {
        lock (readonlyField) { }
        lock (readWriteField) { }           // Noncompliant
        lock (staticReadonlyField) { }
        lock (staticReadWriteField) { }     // Noncompliant
    }

    class SecondLevelNesting
    {
        readonly object readonlyField = new();
        object readWriteField = new();

        static readonly object staticReadonlyField = new();
        static object staticReadWriteField = new();

        void Test()
        {
            lock (readonlyField) { }
            lock (readWriteField) { }       // Noncompliant
            lock (staticReadonlyField) { }
            lock (staticReadWriteField) { } // Noncompliant
        }
    }
}
