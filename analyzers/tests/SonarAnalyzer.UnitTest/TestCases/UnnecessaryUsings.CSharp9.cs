using System;
using System.Linq; // Noncompliant
                   // Noncompliant@-1 - FP: duplicate
using System.Collections;

Console.WriteLine("a");

record Record
{
    public double Size(IList list) => list.Count;
}
