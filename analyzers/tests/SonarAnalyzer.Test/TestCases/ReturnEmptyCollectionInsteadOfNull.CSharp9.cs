using System;
using System.Collections.Generic;
using System.Linq;


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
