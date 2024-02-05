using System;
using System.Linq;

var r = new R();

if (r is R { SomeProperty.Length: >= 0 }) // Noncompliant
{
}

if (r is R { SomeOtherProperty.SomeProperty.LongLength: >= 0 }) // Noncompliant
{
}

if (r is R { SomeOtherProperty.SomeProperty.Rank: >= 0 }) // Compliant
{
}

if (r is R { SomeProperty.Length: not >= 0 }) // Error [CS8518] - this case is now covered by the compiler -> `An expression of type 'R' can never match the provided pattern`
// Noncompliant@-1
{
}

if (r is R { SomeProperty.Length: >= 42 })
{
}

if (r is R {  :  52 }) // Error [CS1001, CS8503]
{
}

record R
{
    public string[] SomeProperty { get; set; }

    public R SomeOtherProperty { get; set; }
}
