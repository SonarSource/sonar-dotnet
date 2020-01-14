using System;

namespace MyLibrary
{
    public enum Visibility : sbyte // Noncompliant {{Change this enum storage to 'Int32'.}}
//              ^^^^^^^^^^
    {
        Visible = 0,
        Invisible = 1,
    }

    public enum Test_01 { } // Compliant
    public enum Test_02 : int { } // Compliant
    public enum Test_03 : System.Int32 { } // Compliant

    public enum Test_04 : long { } // Noncompliant
    public enum Test_05 : ulong { } // Noncompliant

}

// https://github.com/SonarSource/sonar-dotnet/issues/3026
namespace Issue_3026
{
    public enum Long : long // Noncompliant FP, it has to be long due to enum values
    {
        Visible = 0,
        Invisible = 3147483647,
    }

    public enum LongWithNegative : long // Noncompliant FP, it has to be long due to enum values
    {
        Visible = 0,
        Invisible = Int64.MinValue,
    }

    public enum ULong : ulong // Noncompliant FP, it has to be ulong due to enum values
    {
        Visible = 0,
        Invisible = 18446744073709551615,
    }

    public enum UInt : uint // Noncompliant FP, it has to be uint due to enum values
    {
        Visible = 0,
        Invisible = 3147483647,
    }
}
