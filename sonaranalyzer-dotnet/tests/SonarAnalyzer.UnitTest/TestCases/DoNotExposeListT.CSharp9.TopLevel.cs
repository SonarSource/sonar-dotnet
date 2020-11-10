using System;
using System.Collections.Generic;

var x = 1;

public class Bar<T>
{
    // should inner classes in top-level statement console apps raise?
    public List<T> Method1<T>(T arg) => null; // Noncompliant
}
