using System;

namespace Tests.Diagnostics
{
    [FlagsAttribute]
    public enum Color // Noncompliant {{Remove the 'FlagsAttribute' from this enum.}}
//              ^^^^^
    {
        None = 0,
        Red = 1,
        Orange = 3,
//      ^^^^^^^^^^ Secondary
        Yellow = 4
    }

    [FlagsAttribute]
    public enum Color2
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

    [Flags]
    public enum HugeValues: ulong // Noncompliant
    {
        Max = ulong.MaxValue // Secondary
    }

    [Flags]
    public enum InvalidStringEnum : string // Noncompliant
    {
        MyValue = "toto" // Secondary
    }
}
