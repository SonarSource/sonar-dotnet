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

List<IntPtr> list1 = new();
List<UIntPtr> list2 = new();
int res;

res = unchecked(list1.Sum(e => (int)e));  // Noncompliant {{Refactor this code to handle 'OverflowException'.}}
res = unchecked(list2.Sum(e => (int)e));  // Noncompliant {{Refactor this code to handle 'OverflowException'.}}

unchecked
{
    res = list1.Sum(e => (int)e);  // Noncompliant {{Refactor this code to handle 'OverflowException'.}}
    res = list2.Sum(e => (int)e);  // Noncompliant {{Refactor this code to handle 'OverflowException'.}}

    try
    {
        res = list1.Sum(e => (int)e);  // Compliant
        res = list2.Sum(e => (int)e);  // Compliant
    }
    catch (Exception ex)
    {
    }
}
