using System;
using System.Linq;
using System.Diagnostics.Contracts;

Record r = new();
r.GetValue();
r.Method(); // Noncompliant
var x = r.Method();

new nint[] { 1 }.Where(i => true); // Noncompliant
new nint[] { 1 }.ToList(); // Noncompliant

Action<int> a = static (input) => "this string".Equals("other string"); // Noncompliant
Action<nint, nuint> b = static (_, _)=> "this string".Equals("other string"); // Noncompliant

record Record
{
    [Pure]
    public uint Method() { return 0; }

    public nint GetValue() => 42;
}
