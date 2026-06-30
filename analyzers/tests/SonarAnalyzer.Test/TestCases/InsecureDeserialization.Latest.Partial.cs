using System;
using System.Runtime.Serialization;

public partial class PartialConstructor
{
    public partial PartialConstructor(string name)  // Noncompliant
    {
        Name = name ?? string.Empty;
    }
}
