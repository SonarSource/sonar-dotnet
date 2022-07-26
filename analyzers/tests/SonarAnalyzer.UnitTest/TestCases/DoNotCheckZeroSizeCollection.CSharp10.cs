using System;
using System.Linq;

var r = new R();

if (r is R { SomeProperty.Length: >= 0 }) // FN
{
}

if (r is R { SomeProperty.Length: not >= 0 }) // Error [CS8502] - this case is now covered by the compiler -> `An expression of type 'R' can never match the provided pattern`
{
}

if (r is R { SomeProperty.Length: >= 42 })
{
}

record R
{
    public string[] SomeProperty { get; set; }
}
