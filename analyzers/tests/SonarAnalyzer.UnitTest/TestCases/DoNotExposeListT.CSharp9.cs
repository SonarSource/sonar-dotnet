using System;
using System.Collections.Generic;

var x = 1;

public class Bar<T>
{
    public List<T> Method1<T>(T arg) => null; // Noncompliant
}

public record R<T>
{
    public List<T> Method1<T>(T arg) => null; // Noncompliant {{Refactor this method to use a generic collection designed for inheritance.}}

    private List<T> Method2<T>(T arg) => null;

    private object Method3(int arg) => null;

    public List<int> Method4(List<string> someParam) => null;
//         ^^^^^^^^^ Noncompliant
//                           ^^^^^^^^^^^^ Noncompliant@-1

    public List<T> field, field2;
//         ^^^^^^^ Noncompliant {{Refactor this field to use a generic collection designed for inheritance.}}

    public List<int> Property { get; init; }
//         ^^^^^^^^^ Noncompliant {{Refactor this property to use a generic collection designed for inheritance.}}

    public R(List<R<int>> bars) { } // Noncompliant  {{Refactor this constructor to use a generic collection designed for inheritance.}}

    public void Foo()
    {
        Action<List<int>> x = static x => { };
    }
}
