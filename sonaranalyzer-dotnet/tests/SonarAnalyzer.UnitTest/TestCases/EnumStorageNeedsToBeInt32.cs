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