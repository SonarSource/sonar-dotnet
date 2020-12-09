using System;

namespace MyLibrary
{
    public enum Color
    {
        None,
        Red,
        Orange,
        Yellow,
        ReservedColor  // Noncompliant {{Remove or rename this enum member.}}
//      ^^^^^^^^^^^^^
    }

    public enum Reserved
    {
        None,
        reservedcolor,
        reserved, // Noncompliant
        fooReserved // Noncompliant
    }
}
