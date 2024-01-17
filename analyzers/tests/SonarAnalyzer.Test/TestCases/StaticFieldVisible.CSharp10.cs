using System;

public struct StaticFieldVisibleStruct
{
    public StaticFieldVisibleStruct() { }

    public static double Pi = 3.14;  // Noncompliant
//                       ^^
    public const double Pi2 = 3.14;
    public double Pi3 = 3.14;

    [ThreadStatic]
    public static int value; // Compliant, thread static field values are not shared between threads
}
