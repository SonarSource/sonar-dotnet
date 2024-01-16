using System;
using System.Linq; // Noncompliant
using System.Collections;

Console.WriteLine("a");

record Record
{
    public double Size(IList list) => list.Count;
}
