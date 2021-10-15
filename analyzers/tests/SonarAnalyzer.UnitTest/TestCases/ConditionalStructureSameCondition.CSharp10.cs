using System.Collections.Generic;

SomeClass f = null;

if (f is { SomeField.Count: 42 }) // Secondary [flow1]
{
}
else if (f is { SomeField.Count: 42 }) // Noncompliant [flow1]
{
}

if (f is { SomeField.Count: 16 })
{
}
else if (f is { SomeField.Count: 23 })
{
}

abstract class SomeClass
{
    public List<int> SomeField;
}
