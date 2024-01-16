using System;
using System.Collections.Generic;

public class Sample
{
    public void Method()
    {
        List<int> list;

        (list, var a) = (new List<int>(), 42);
        list.Clear();   // Noncompliant
        list.Add(42);
        list.Clear();

        (var list2, var b) = (new List<int>(), 42);
        list2.Clear();   // Noncompliant
        list2.Add(42);
        list2.Clear();

        (var list3, var c) = (new List<int>() { 42 }, 42);
        list3.Clear();   // Compliant
    }
}
