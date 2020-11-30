using System;
using System.Collections.Generic;

List<nuint> l = new() { 1, 2 }; // Compliant - FN
List<nint> Get() => new() { 1, 2 };  // Compliant - FN
List<nint> GetEmpty() => new();  // Compliant

[MyAttribute] // Compliant
record Record
{
}

[MyAttribute()] // Noncompliant {{Remove these redundant parentheses.}}
record RecordNoncompliant
{
}

public class MyAttribute : Attribute
{
    public int MyProperty { get; set; }
}
