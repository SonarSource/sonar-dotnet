using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

file record Person(string FirstName, string LastName); // Compliant

file record Person1 // Noncompliant {{This record can't be instantiated; make its constructor 'public'.}}
{
    public string FirstName { get; }

    private Person1() { }
}

file record StaticUsage // Noncompliant
{
    private StaticUsage() { }
    public static void M() { }
}

file class Baz { }

file class Bar  // Noncompliant
{
    public static readonly Baz Instance = new();

    public bool IsActive => true;

    private Bar() { }
}
