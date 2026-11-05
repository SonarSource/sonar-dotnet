using System;
using System.Linq;
using System.Diagnostics.Contracts;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Collections.Frozen;

class CSharp13
{
    // https://sonarsource.atlassian.net/browse/NET-416
    void ReadOnlySet(HashSet<int> set)
    {
        var readonlySet = new ReadOnlySet<int>(set);
        readonlySet.OfType<object>();   // Noncompliant
        readonlySet.ToList();           // Noncompliant
        readonlySet.Where(i => true);   // Noncompliant
        readonlySet.Min();              // Noncompliant
        readonlySet.ToHashSet();        // Noncompliant
        readonlySet.ToImmutableArray(); // Noncompliant
        readonlySet.ToFrozenSet();      // Noncompliant
    }

    void LinqExtensions(List<int> list)
    {
        list.CountBy(i => i);                              // Noncompliant
        list.AggregateBy(x => x, y => y, (x, y) => x + y); // Noncompliant
        list.Index();                                      // Noncompliant
    }
}

class CSharp14
{
    private IEnumerable<int> field;

    void NullConditionalAssignment(CSharp14 sample, List<int> list)
    {
        sample?.field = list.Where(x => x > 5); // Compliant
    }
}

namespace CSharp15
{
    class NewNumericTypes
    {
        void Decimal32IgnoredToString(System.Numerics.Decimal32 a)
        {
            a.ToString(); // FN NET-4960
        }

        void Decimal32UsedToString(System.Numerics.Decimal32 a)
        {
            var s = a.ToString(); // Compliant, return value is used
        }

        void OtherNewNumericTypesIgnoredToString(System.Numerics.Decimal64 d64, System.Numerics.Decimal128 d128, System.Numerics.BFloat16 bf16)
        {
            d64.ToString();     // FN NET-4960
            d128.ToString();    // FN NET-4960
            bf16.ToString();    // FN NET-4960
        }

        void OtherNewNumericTypesUsedToString(System.Numerics.Decimal64 d64, System.Numerics.Decimal128 d128, System.Numerics.BFloat16 bf16)
        {
            var s1 = d64.ToString();    // Compliant
            var s2 = d128.ToString();   // Compliant
            var s3 = bf16.ToString();   // Compliant
        }

        void GenericComplexIgnoredToString(System.Numerics.Complex<double> c)
        {
            c.ToString();           // FN NET-4960
        }

        void GenericComplexUsedToString(System.Numerics.Complex<double> c)
        {
            var s = c.ToString();   // Compliant
        }
    }
}

class OlderNumericTypes
{
    void IgnoredToString(Half h, Int128 i128, UInt128 u128, System.Numerics.BigInteger bigInt, System.Numerics.Complex complex)
    {
        h.ToString();        // FN NET-5032
        i128.ToString();     // FN NET-5032
        u128.ToString();     // FN NET-5032
        bigInt.ToString();   // FN NET-5032
        complex.ToString();  // FN NET-5032
    }

    void UsedToString(Half h, Int128 i128, UInt128 u128, System.Numerics.BigInteger bigInt, System.Numerics.Complex complex)
    {
        var s1 = h.ToString();        // Compliant
        var s2 = i128.ToString();     // Compliant
        var s3 = u128.ToString();     // Compliant
        var s4 = bigInt.ToString();   // Compliant
        var s5 = complex.ToString();  // Compliant
    }
}

class NewLinqMethods
{
    void JoinResultIgnored(List<int> outer, List<int> inner)
    {
        outer.FullJoin(inner, x => x, x => x);     // Noncompliant
        outer.LeftJoin(inner, x => x, x => x);     // Noncompliant
        outer.RightJoin(inner, x => x, x => x);    // Noncompliant
    }

    void JoinResultUsed(List<int> outer, List<int> inner)
    {
        var full = outer.FullJoin(inner, x => x, x => x);      // Compliant
        var left = outer.LeftJoin(inner, x => x, x => x);      // Compliant
        var right = outer.RightJoin(inner, x => x, x => x);    // Compliant
    }
}
