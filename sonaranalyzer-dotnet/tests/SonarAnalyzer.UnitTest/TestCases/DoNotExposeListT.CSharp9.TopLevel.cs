using System;
using System.Collections.Generic;

var x = 1;

public class Bar<T>
{
    public List<T> Method1<T>(T arg) => null; // Noncompliant
}
