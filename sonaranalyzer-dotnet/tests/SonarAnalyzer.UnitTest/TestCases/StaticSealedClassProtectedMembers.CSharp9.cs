using System;

sealed record Record
{
    public int field0; // Compliant

    protected int field1; // Noncompliant

    internal protected int field2; // Noncompliant

    internal readonly protected int field3; // Noncompliant

    protected static int field4; // Noncompliant

    internal protected static int field5; // Noncompliant

    internal readonly protected static int field6; // Noncompliant

    protected const int const1 = 5; // Noncompliant

    internal protected const int const2 = 10; // Noncompliant

    protected Record() { } // Noncompliant

    internal protected Record(string name) { } // Noncompliant
}
