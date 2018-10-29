using System;

namespace Tests.Diagnostics
{
    [FlagsAttribute]
    public enum Color // Compliant - only power of 2 values
    {
        None = 0,
        Red = 1,
        Orange = 2,
        Yellow = 4
    }

    [FlagsAttribute]
    public enum Days
    {
        None = 0,
        Monday = 1,
        Tuesday = 2,
        Wednesday = 4,
        Thursday = 8,
        Friday = 16,
        All = Monday | Tuesday | Wednesday | Thursday | Friday    // Compliant - combination of other values
    }

    [FlagsAttribute]
    public enum Color2 // Noncompliant {{Remove the 'FlagsAttribute' from this enum.}}
//              ^^^^^^
    {
        None = 0,
        Red = 1,
        Orange = 3,
//      ^^^^^^^^^^ Secondary
        Yellow = 4
    }

    [FlagsAttribute]
    public enum Color3 // Compliant - values are automatically set with power of 2 values
    {
        None,
        Red,
        Orange,
        Yellow = 4
    }

    [FlagsAttribute]
    public enum Color4
    {
        None = 4,
        Red = 8,
        Orange = 16,
        Yellow = 12
    }

    [Flags]
    public enum Color5 : int // Noncompliant
    {
        Value = -3, // Secondary
        Other = -4 // Secondary
    }

    [Flags]
    public enum HugeValues: ulong // Noncompliant
    {
        Max = ulong.MaxValue // Secondary
    }

    [Flags]
    enum EnumFoo { }

    [Flags]
    enum EnumFoo2 // Noncompliant
    {
        a = 2,
        b = 4,
        c = 4,
        d = 10 // Secondary
    }

    [Flags]
    enum EnumFoo3
    {
        N1 = 1,
        N2 = 2,
        N3 = N1 | N2,
        N4 = 4,
        N5 = N4 | N1,
        N6 = N4 | N2
    }

    [Flags]
    enum EnumFoo4 // Noncompliant
    {
        N1 = 1,
        N2 = 2,
        N3 = N1 | N2,
        N5 = 5 // Secondary
    }
}
