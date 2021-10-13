global using System.Collections;
global using System.Collections.Generic;

namespace Tests.Diagnostics;
using System;
// the System.Linq using should be here as this is the closest namespace

interface I { }
class A : I { }
class B : A { }
record Record
{
    public void M2(IEnumerable<A> enumerable)
    {
        foreach (A i in enumerable)
        { }
        foreach (var i in enumerable)
        { }
        foreach (B i in enumerable) // Noncompliant {{Either change the type of 'i' to 'A' or iterate on a generic collection of type 'B'.}}
//               ^
            { }
    }
}
record struct RecordStruct
{
    public void M3(A[] array)
    {
        foreach (A i in array)
        { }
        foreach (I i in array)
        { }
        foreach (B i in array) // Noncompliant
        { }
    }
}
