using System;
using System.Runtime.InteropServices;

public partial class PartialConstructor
{
    public partial PartialConstructor(__arglist)    // Noncompliant
    {

    }
}
