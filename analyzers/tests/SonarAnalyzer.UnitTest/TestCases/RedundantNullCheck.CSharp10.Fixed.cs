using System.Collections.Generic;

SomeClass someClass = new();

if (someClass is { SomeProperty.Count: 42 }) { } // Fixed
if (someClass is { SomeProperty.Count: 42 }) { } // Fixed

if (someClass != null && someClass is not { SomeProperty.Count: 42 }) { } // Compliant

sealed record SomeClass
{
    public List<int> SomeProperty;
}
