using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;

const int localConst_Zero = 0;
var someEnumerable = new List<string>();
var anotherEnumerable = new List<string>();

var result = someEnumerable.Count() >= 0; // Noncompliant {{The 'Count' of 'IEnumerable<T>' always evaluates as 'True' regardless the size.}}

if (someEnumerable.Count() is >= 0) // Noncompliant {{The 'Count' of 'IEnumerable<T>' always evaluates as 'True' regardless the size.}}
//                            ^^^^
{
}

if (someEnumerable.Count() is >= localConst_Zero) // Noncompliant
{
}

if (((someEnumerable.Count()), anotherEnumerable.Count()) is ( >= 0, // Noncompliant
//                                                             ^^^^
    >= 0)) // Noncompliant
//  ^^^^
{
}

if (someEnumerable.Count() is < 0) { } // Noncompliant
if (someEnumerable.Count() is < localConst_Zero) { } // Noncompliant
if (someEnumerable.Count() is >= 0 or 1) { } // Noncompliant
if (someEnumerable.Count() is not >= 0) { } // Noncompliant
if (someEnumerable.Count() is <= -1) { } // Noncompliant - AlwaysFalse
if (someEnumerable.Count() is > -17) { } // Noncompliant - AlwaysTrue
if (someEnumerable.Count() is { } _) { } // Compliant - Not a comparision

int variable = 42;

var x = args.Length switch
{
    >= 0 => 1, // Noncompliant
//  ^^^^
    < 0 => 2, // Noncompliant
};

x = (args.Length, variable) switch
{
    ( >= 0, 4) => 1, // Noncompliant
//    ^^^^
    ( >= 0, 2) => 2, // Noncompliant
    _ => 3,
};

var y = someEnumerable.Count() switch
{
    1 => 1,
    2 => 2,
    >= 0 => 3, // Noncompliant
    < -2 => 4, // Noncompliant
    _ => 5,
};

List<string> list = new();
var z = list.Count switch
{
    1 => 1,
    not >= 0 => 2, // Noncompliant
    not < 0 => 3   // Noncompliant
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

if (r is R { SomeProperty: { Length: >= 0 } }) // Noncompliant
//                                   ^^^^
{
}

if (r is R { SomeProperty: { Length: not >= 0 } }) // Error [CS8518] `An expression of type 'R' can never match the provided pattern
                                                   // Noncompliant@-1
{
}

if (r is R { SomeProperty: { Length: >= 42 } })
{
}

if (r is R { SomeProperty: { Rank: >= 0 } })
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

var r2 = new R();

if (r2 is R { SomeProperty.Length: >= 0 }) // Noncompliant
{
}

if (r2 is R { SomeOtherProperty.SomeProperty.LongLength: >= 0 }) // Noncompliant
{
}

if (r2 is R { SomeOtherProperty.SomeProperty.Rank: >= 0 }) // Compliant
{
}

if (r2 is R { SomeProperty.Length: not >= 0 }) // Error [CS8518] - this case is now covered by the compiler -> `An expression of type 'R' can never match the provided pattern`
                                               // Noncompliant@-1
{
}

if (r2 is R { SomeProperty.Length: >= 42 })
{
}

if (r2 is R {  :  52 }) // Error [CS1001, CS8503]
{
}

record R
{
    public string[] SomeProperty { get; set; }

    public R SomeOtherProperty { get; set; }

    List<string> Prop
    {
        init
        {
            if (value.Count < 0) { }   // Noncompliant
            if (value.Count < -1) { }  // Noncompliant
            if (0 > value.Count) { }   // Noncompliant
            if (-42 > value.Count) { } // Noncompliant
            if (value.Count >= 0) { }  // Noncompliant {{The 'Count' of 'List<T>' always evaluates as 'True' regardless the size.}}
            if (value.Count == 0) { }
            if (value.Count == 1) { }
        }
    }
}

class CSharp13
{
    void NewCollectionTypes(OrderedDictionary<int, int> orderedDictionary, ReadOnlySet<int> readonlySet)
    {
        _ = orderedDictionary.Count >= 0; // Noncompliant
        _ = readonlySet.Count >= 0;       // Noncompliant
    }
}

class DuckTypedCountables // NET-3922: duck-typed C# 8 "countable" covers any accessible int Count/Length
{
    void Spans()
    {
        Span<int> span = stackalloc int[4];
        _ = span.Length >= 0; // Noncompliant {{The 'Length' of 'Span<T>' always evaluates as 'True' regardless the size.}}
        ReadOnlySpan<int> readOnlySpan = span;
        _ = readOnlySpan.Length >= 0; // Noncompliant {{The 'Length' of 'ReadOnlySpan<T>' always evaluates as 'True' regardless the size.}}
    }

    void CustomTypes(Custom custom, LongSized longSized)
    {
        _ = custom.Count >= 0;      // Compliant
        _ = custom.Length >= 0;     // Compliant
        _ = custom.LongLength >= 0; // Compliant
        _ = custom.Size >= 0;       // Compliant
        _ = longSized.Count >= 0;   // Compliant
        _ = longSized.Length >= 0;  // Compliant
    }

    class Custom
    {
        public int Count => 0;
        public int Length => 0;
        public long LongLength => 0;
        public int Size => 0;
    }

    class LongSized
    {
        public long Count => 0;
        public long Length => 0;
    }
}

class CSharp15
{
    void BitArrayMembers(ReadOnlySpan<bool> values)
    {
        var bits = new System.Collections.BitArray(values);
        _ = bits.Count >= 0;  // Noncompliant {{The 'Count' of 'BitArray' always evaluates as 'True' regardless the size.}}
        _ = bits.Length >= 0; // Noncompliant {{The 'Length' of 'BitArray' always evaluates as 'True' regardless the size.}}
        _ = bits.PopCount() >= 0;
    }
}

class ExtensionCountables
{
    void Method(Bag bag, List<int> list)
    {
        _ = bag.Count >= 0;     // FN NET-5018
        _ = bag.Length >= 0;    // FN NET-5018
        _ = bag.Count > 0;      // Compliant
        _ = list.Count >= 0;    // Noncompliant
    }

    public class Bag : System.Collections.IEnumerable
    {
        public int this[int index] => 0;
        public System.Collections.IEnumerator GetEnumerator() => null;
    }
}

static class BagExtensions
{
    extension(ExtensionCountables.Bag b)
    {
        public int Count => 0;
        public int Length => 0;
    }
}

union ArrayOrList(int[], List<int>);

class CSharp15Unions
{
    void IsPattern(ArrayOrList u)
    {
        _ = u is int[] { Length: >= 0 };        // Noncompliant
        _ = u is List<int> { Count: >= 0 };     // Noncompliant
        _ = u is int[] { Length: >= 1 };
    }

    int SwitchExpression(ArrayOrList u) =>
        u switch
        {
            List<int> { Count: >= 0 } => 1,     // Noncompliant
            _ => 0
        };

    void SwitchStatement(ArrayOrList u)
    {
        switch (u)
        {
            case int[] { Length: >= 0 }:        // Noncompliant
                break;
        }
    }
}
