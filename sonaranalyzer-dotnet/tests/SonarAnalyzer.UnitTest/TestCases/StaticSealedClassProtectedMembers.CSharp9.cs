using System;

record Record
{
    public int field0; // Compliant

    protected int field1; // Compliant - FN

    internal protected int field2; // Compliant - FN

    internal readonly protected int field3; // Compliant - FN

    protected static int field4; // Compliant - FN

    internal protected static int field5; // Compliant - FN

    internal readonly protected static int field6; // Compliant - FN

    protected const int const1 = 5; // Compliant - FN

    internal protected const int const2 = 10; // Compliant - FN

    protected Record() { } // Compliant - FN

    internal protected Record(string name) { } // Compliant - FN
}
