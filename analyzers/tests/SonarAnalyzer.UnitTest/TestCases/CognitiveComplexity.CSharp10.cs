using System;
using System.Collections.Generic;

namespace TestCases;

class Fruit { public List<int> Prop; }
record MyRecord
{
    Fruit field;
    bool Prop => field == null || field is { Prop.Count: 5 }; // Noncompliant {{Refactor this property to reduce its Cognitive Complexity from 1 to the 0 allowed.}}
    //                                                           Secondary@-1 {{+1}}

    public void SomeMethod()
    {
        int x;

        (x, var y) = (16, 23);

        var (a, b) = (16, 23);

        var j = (a, b) = (23, 16);
    }
}
