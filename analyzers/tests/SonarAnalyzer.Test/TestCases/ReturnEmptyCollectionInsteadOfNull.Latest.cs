using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Numerics;

List<string> LocalFunction() => null; // Noncompliant {{Return an empty collection instead of null.}}
List<string> LocalFunctionNew() => new(); // Compliant

static IEnumerable<string> StaticLocalFunction() => (null); // Noncompliant

record Record
{
    IEnumerable<string> Property => null; // Noncompliant

    IEnumerable<char> Method(string str)
    {
        if (str == null)
        {
            return null; // Noncompliant
        }

        return str.ToCharArray();
    }

    static (IEnumerable<char>, IEnumerable<char>) SomeMethod()
    {
        return (null, null); // FN
    }

    IEnumerable<int> Compliant() => Enumerable.Empty<int>();
}

class TernariesAndDefaults
{
    private static readonly string checkAgainst = "42";

    public static IEnumerable<string> Foo1() => null; // Noncompliant
    public static IEnumerable<string> Foo2() => (null); // Noncompliant
    public static IEnumerable<string> Foo3() => default; // Noncompliant
    public static IEnumerable<string> Foo4() => (default); // Noncompliant
    public static IEnumerable<string> Foo5() => true ? null : new List<string>(); // Noncompliant
    public static IEnumerable<string> Foo6() =>
        ((
        checkAgainst == null // Compliant
        ? default // Noncompliant
        : checkAgainst is not ((null)) // Compliant
            ? ((default)) // Secondary
            : DateTime.Now.Second % 2 == 0
                ? null // Secondary
                : (((null))) // Secondary
        ));

    public static IEnumerable<string> Foo7()
    {
        if (true)
        {
            if (true)
            {
                if (true)
                {
                    return ((null)); // Noncompliant
                }
            }
            else
            {
                return default(IEnumerable<string>); // Secondary
            }
        }
        else
        {
            return checkAgainst != null // Compliant
                ? new List<string>()
                : (default); // Secondary
        }
    }

    public static IEnumerable<string> Foo8()
    {
        return true
            ? new List<string>()
            : false
                ? default // Noncompliant
                : null;  // Secondary
    }
}

class Operators : IAdditionOperators<Operators, Operators, IEnumerable<string>>
{
    private static readonly string checkAgainst = "42";

    public static IEnumerable<string> operator +(Operators right, Operators left) => null; // Noncompliant
    public static IEnumerable<string> operator -(Operators right, Operators left) => (null); // Noncompliant
    public static IEnumerable<string> operator /(Operators right, Operators left) => default; // Noncompliant
    public static IEnumerable<string> operator *(Operators right, Operators left) => (default); // Noncompliant
    public static IEnumerable<string> operator %(Operators right, Operators left) => true ? null : new List<string>(); // Noncompliant
    public static IEnumerable<string> operator ^(Operators right, Operators left) =>
        ((
        checkAgainst == null // Compliant
        ? default // Noncompliant
        : checkAgainst is not ((null)) // Compliant
            ? ((default)) // Secondary
            : DateTime.Now.Second % 2 == 0
                ? null // Secondary
                : (((null))) // Secondary
        ));

    public static IEnumerable<string> operator &(Operators right, Operators left)
    {
        if (true)
        {
            if (true)
            {
                if (true)
                {
                    return ((null)); // Noncompliant
                }
            }
            else
            {
                return default(IEnumerable<string>); // Secondary
            }
        }
        else
        {
            return checkAgainst != null // Compliant
                ? new List<string>()
                : (default); // Secondary
        }
    }

    public static IEnumerable<string> operator |(Operators right, Operators left)
    {
        return true
            ? new List<string>()
            : false
                ? default // Noncompliant
                : null;  // Secondary
    }
}

// https://sonarsource.atlassian.net/browse/NET-459
public class CSharp13
{
    partial class Partial
    {
        partial IEnumerable<string> MyStrings
        {
            get => ((null)); // Noncompliant
        }

        partial IEnumerable<string> this[int i] => null; // Noncompliant
    }

    partial class Partial
    {
        partial IEnumerable<string> MyStrings { get; }

        partial IEnumerable<string> this[int i] { get; }
    }
}

// https://sonarsource.atlassian.net/browse/NET-2347
public class Repro_NET2347
{

    public static ValueTypeCollection StructCollection(byte b)
    {
        return b == 0
            ? default // Noncompliant - FP - The returned struct is a valid ValueTypeCollection with the `bits` parameter initialized to 0
            : new ValueTypeCollection(b);
    }

    public static ImmutableArray<T> ReturnImmutableArray<T>() => default; // Noncompliant TP, default here is an uninitialized ImmutableArray<T> with the private `T[]` field being `null`.


    public readonly struct ValueTypeCollection(byte bits) : IReadOnlyCollection<int>
    {
        public int Count => BitOperations.PopCount(bits);

        public IEnumerator<int> GetEnumerator() =>
            Enumerable.Range(0, 8).GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
