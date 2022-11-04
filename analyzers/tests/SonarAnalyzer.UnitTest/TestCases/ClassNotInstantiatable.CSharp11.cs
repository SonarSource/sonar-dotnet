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

file class Baz { }

file class Bar  // Noncompliant
{
    private Bar() { }
}
