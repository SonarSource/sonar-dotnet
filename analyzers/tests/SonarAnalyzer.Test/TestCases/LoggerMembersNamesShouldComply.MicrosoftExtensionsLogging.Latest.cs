using System;
using Microsoft.Extensions.Logging;

public class Cat;
public class Dog;

public union Pet(Cat, Dog)
{
    static ILogger myLogger;                // Noncompliant {{Rename this field 'myLogger' to match the regular expression '^_?[Ll]og(ger)?$'.}}
    static ILogger<Pet> Prop { get; set; }  // Noncompliant {{Rename this property 'Prop' to match the regular expression '^_?[Ll]og(ger)?$'.}}
    static ILogger logger;                  // Compliant
}

public closed class ClosedBase
{
    ILogger myLogger;                       // Noncompliant
    ILogger _logger;                        // Compliant
}

public static class Extensions
{
    extension(string s)
    {
        public ILogger this[int i] => null; // Compliant, not a field or property
        ILogger OtherName => null;          // Noncompliant
    }
}
