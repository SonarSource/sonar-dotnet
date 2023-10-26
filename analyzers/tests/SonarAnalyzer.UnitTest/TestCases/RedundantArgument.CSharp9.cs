using System;
using System.Collections.Generic;

A(1, 5); //Noncompliant, y has the default value
//   ^
A(1, z: 7); //Noncompliant, z has the default value

A(1);
A(1, 2, 4);

Record r1 = new(1);
Record r2 = new(1, 5); // Noncompliant
Record r3 = new(1, 6); // Compliant

r1 = new Record(1);
r2 = new Record(1, 5); // Noncompliant
r3 = new Record(1, 6); // Compliant

_ = new List<int> { 1 }; // Constructor without ArgumentList

int x = 5;
var y = x switch
{
    1 => A(1),
    5 => A(1, x), // Compliant - FN
    7 => A(1, z: 7) // Noncompliant
};

if (x is 5)
{
    A(1, x); // Compliant - FN
}

Action<int> a = static x =>
{
    A(x);
    A(x, 5); // Noncompliant
};

static int A(int x, int y = 5, int z = 7) => x;
static int B(int x, int y, int z) => x;

record Record
{
    public Record(int x, int y = 5) { }
}
