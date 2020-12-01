using System;
using System.Collections.Generic;
using System.Linq;

List<int> list = new();
int d = unchecked(list.Sum());  // Noncompliant {{Refactor this code to handle 'OverflowException'.}}
unchecked
{
    int e = list.Sum();  // Noncompliant
    e = Enumerable.Sum(list); // Noncompliant
}

