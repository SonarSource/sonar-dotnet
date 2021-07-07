using System;
using System.Collections.Generic;
using System.Linq;

var someEnumerable = new List<string>();
var anotherEnumerable = new List<string>();

var result = someEnumerable.Count() >= 0; // Noncompliant {{The count of 'IEnumerable<T>' is always '>=0', so fix this test to get the real expected behavior.}}

if (someEnumerable.Count() is >= 0) // Noncompliant {{The count of 'IEnumerable<T>' is always '>=0', so fix this test to get the real expected behavior.}}
//  ^^^^^^^^^^^^^^^^^^^^^^
{
}

if (((someEnumerable.Count()), // Noncompliant
//    ^^^^^^^^^^^^^^^^^^^^^^
    anotherEnumerable.Count()) is (>= 0, >= 0)) // Noncompliant
//  ^^^^^^^^^^^^^^^^^^^^^^^^^
{
}

if (someEnumerable.Count() is < 0) { } // FN
if (someEnumerable.Count() is >= 0 or 1) { } // Noncompliant
if (someEnumerable.Count() is not >= 0) { } // Noncompliant

var x = args.Length switch
{
    >= 0 => 1, // FN
    < 0 => 2 // FN
};

var y = someEnumerable.Count() switch
{
    1 => 1,
    2 => 2,
    >= 0 => 3, // FN
    < 0 => 4 // FN
};

List<string> list = new();
var z = list.Count switch
{
    1 => 1,
    not >= 0 => 2, // FN, it means it's <0
    not < 0 => 3 // FN, it means it's >=0
};

record R
{
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
