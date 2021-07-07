using System;
using System.Collections.Generic;
using System.Linq;

var someEnumerable = new List<string>();
var anotherEnumerable = new List<string>();

var result = someEnumerable.Count() >= 0; // Noncompliant {{The count of 'IEnumerable<T>' is always '>=0', so fix this test to get the real expected behavior.}}

if (someEnumerable.Count() is >= 0) // Noncompliant {{The count of 'IEnumerable<T>' is always '>=0', so fix this test to get the real expected behavior.}}
//                            ^^^^
{
}

if (((someEnumerable.Count()), anotherEnumerable.Count()) is (>= 0, // Noncompliant
//                                                            ^^^^
    >= 0)) // Noncompliant
//  ^^^^
{
}

if (someEnumerable.Count() is < 0) { } // FN
if (someEnumerable.Count() is >= 0 or 1) { } // Noncompliant
if (someEnumerable.Count() is not >= 0) { } // Noncompliant

int variable = 42;

var x = args.Length switch
{
    >= 0 => 1, // Noncompliant
//  ^^^^
    < 0 => 2 // FN
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
    < 0 => 4 // FN
};

List<string> list = new();
var z = list.Count switch
{
    1 => 1,
    not >= 0 => 2, // Noncompliant
    not < 0 => 3 // FN, it means it's >=0
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

record R
{
    public string[] SomeProperty { get; set; }

    List<string> Prop
    {
        init
        {
            if (value.Count < 0) { } // FN https://github.com/SonarSource/sonar-dotnet/issues/3735
            if (value.Count >= 0) { } // Noncompliant {{The count of 'ICollection' is always '>=0', so fix this test to get the real expected behavior.}}
            if (value.Count == 0) { }
            if (value.Count == 1) { }
        }
    }
}
