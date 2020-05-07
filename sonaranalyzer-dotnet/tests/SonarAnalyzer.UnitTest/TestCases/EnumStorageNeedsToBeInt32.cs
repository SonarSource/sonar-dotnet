using System;

namespace MyLibrary
{
    public enum Visibility : sbyte // Noncompliant {{Change this enum storage to 'Int32'.}}
//              ^^^^^^^^^^
    {
        Visible = 0,
        Invisible = 1,
    }

    public enum Byte : byte { }  // Noncompliant
    public enum Short : short { } // Noncompliant
    public enum UShort : ushort { }  // Noncompliant

    public enum Default { } // Compliant
    public enum IntAlias : int { } // Compliant
    public enum Int : System.Int32 { } // Compliant
    public enum Long : long { } // Compliant
    public enum ULong : ulong { } // Compliant
    public enum UInt : uint { }  // Compliant
}
