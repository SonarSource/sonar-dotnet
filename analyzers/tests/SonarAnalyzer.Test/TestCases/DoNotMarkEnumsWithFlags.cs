using System;

namespace Tests.Diagnostics
{
    public enum NoFlags // Compliant - no flags attribute
    {
        None = 0,
        Something = 3
    }

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
//      ^^^^^^^^^^ Secondary {{Enum value is not a power of two.}}
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
    public enum NegativeValues
    {
        Default = 0,
        A = -2,
        B = -4,
        C = 1 << 31, // It overflows and becomes negative: https://github.com/SonarSource/sonar-dotnet/issues/7991
        D = A | B,
        E = 2,
        F = D | E
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

namespace DifferentBaseType
{
    [Flags]
    public enum EByte : byte
    {
        A = 1,
        B = 2,
        C = A | B
    }

    [Flags]
    public enum ESByte : sbyte
    {
        A = -2,
        B = 2,
        C = A | B
    }

    [Flags]
    public enum EShort : short
    {
        A = -2,
        B = 2,
        C = A | B
    }

    [Flags]
    public enum EUShort : ushort
    {
        A = 1,
        B = 2,
        C = A | B
    }

    [Flags]
    public enum EInt : int
    {
        A = -2,
        B = 2,
        C = A | B
    }

    [Flags]
    public enum EUInt : uint
    {
        A = 1,
        B = 2,
        C = A | B
    }

    [Flags]
    public enum ELong : long
    {
        A = -2,
        B = 2,
        C = A | B
    }

    [Flags]
    public enum EULong : ulong
    {
        A = 1,
        B = 2,
        C = A | B
    }
}
