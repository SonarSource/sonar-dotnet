using System;
using System.Collections.Generic;
using System.Linq;

const int localConst_Zero = 0;
var someEnumerable = new List<string>();
var anotherEnumerable = new List<string>();

var result = someEnumerable.Count() >= 0; // Noncompliant {{The 'Count' of 'IEnumerable<T>' is always '>=0', so fix this test to get the real expected behavior.}}

if (someEnumerable.Count() is >= 0) // Noncompliant {{The 'Count' of 'IEnumerable<T>' is always '>=0', so fix this test to get the real expected behavior.}}
//                            ^^^^
{
}

if (someEnumerable.Count() is  >= localConst_Zero) // Noncompliant
{
}

if (((someEnumerable.Count()), anotherEnumerable.Count()) is (>= 0, // Noncompliant
//                                                            ^^^^
    >= 0)) // Noncompliant
//  ^^^^
{
}

if (someEnumerable.Count() is < 0) { } // Noncompliant
if (someEnumerable.Count() is >= 0 or 1) { } // Noncompliant
if (someEnumerable.Count() is not >= 0) { } // Noncompliant

int variable = 42;

var x = args.Length switch
{
    >= 0 => 1, // Noncompliant
//  ^^^^
    < 0 => 2 // Noncompliant
};

x = (args.Length, variable) switch
{
    (>= 0, 4) => 1, // Noncompliant
//   ^^^^
    (>= 0, 2) => 2 // Noncompliant
};

var y = someEnumerable.Count() switch
{
    1 => 1,
    2 => 2,
    >= 0 => 3, // Noncompliant
    < -2 => 4 // Noncompliant
};

List<string> list = new();
var z = list.Count switch
{
    1 => 1,
    not >= 0 => 2, // Noncompliant
    not < 0 => 3 // Noncompliant
};

switch (list.Count)
{
    case >= 0: // Noncompliant
//       ^^^^
        break;
    case -42:
        break;
    default:
        break;
}

var r = new R();

if (r is R {SomeProperty: { Length: >= 0} }) // Noncompliant
//                                  ^^^^
{
}

if (r is R { SomeProperty: { Length: not >= 0 } }) // Noncompliant
//                                       ^^^^
{
}

if (r is R { SomeProperty: { Length: >= 42 } })
{
}

if (r is R { SomeProperty: {  Rank: >= 0 } })
{
}

int a = 1, b = 2, c = 3;

if ((a, b) is (1)) // Error[CS0029]
{
}

if ((a, b) is (R { SomeProperty: { Count: 5 } })) // Error[CS8121]
{
}

if ((a, b, c) is (1, 2)) // Error[CS8502]
{
}

if ((a, b, c) is (1, 2, 3, 4)) // Error[CS8502]
{
}

record R
{
    public string[] SomeProperty { get; set; }

    List<string> Prop
    {
        init
        {
            if (value.Count < 0) { } // Noncompliant
            if (value.Count < -1) { } // Noncompliant
            if (0 > value.Count) { } // Noncompliant
            if (-42 > value.Count) { } // Noncompliant
            if (value.Count >= 0) { } // Noncompliant {{The 'Count' of 'ICollection' is always '>=0', so fix this test to get the real expected behavior.}}
            if (value.Count == 0) { }
            if (value.Count == 1) { }
        }
    }
}
