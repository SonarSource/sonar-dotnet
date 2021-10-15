using System.Collections.Generic;

SomeClass someClass = new();

if (someClass != null && someClass is { SomeProperty.Count: 42 }) { } // Noncompliant
if (!(someClass is null) && someClass is { SomeProperty.Count: 42 }) { } // Noncompliant

if (someClass != null && someClass is not { SomeProperty.Count: 42 }) { } // Compliant

sealed record SomeClass
{
    public List<int> SomeProperty;
}
