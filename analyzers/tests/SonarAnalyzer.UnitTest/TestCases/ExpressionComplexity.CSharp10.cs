using System;

object x = new SomeClass();

if (x is SomeClass { SomeProperty.Length: 1 } or SomeClass { SomeProperty.Length: 2 } or SomeClass { SomeProperty.Length: 3 } or SomeClass { SomeProperty.Length: 4 } or SomeClass { SomeProperty.Length: 5 }) { } // Noncompliant

if (x is SomeClass { SomeProperty.Length: 1 } and SomeClass { SomeProperty.Length: 1 } and SomeClass { SomeProperty.Length: 1 } and SomeClass { SomeProperty.Length: 1 } and SomeClass { SomeProperty.Length: 1 }) { } // Noncompliant

switch (x)
{
    case SomeClass { SomeProperty.Length: 1 } and SomeClass { SomeProperty.Length: 1 } and SomeClass { SomeProperty.Length: 1 } and SomeClass { SomeProperty.Length: 1 } and SomeClass { SomeProperty.Length: 1 }: // Noncompliant
        break;
    case SomeClass { SomeProperty.Length: 6 }:
        break;
    default:
        break;
}

public class SomeClass
{
    public int[] SomeProperty { get; }
}
