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

public class Operators
{
    public static IEnumerable<string> operator +(Operators right, Operators left) => null; // Noncompliant
    public static IEnumerable<string> operator -(Operators right, Operators left) => (null); // Noncompliant
    public static IEnumerable<string> operator /(Operators right, Operators left) => default; // Noncompliant
    public static IEnumerable<string> operator *(Operators right, Operators left) => (default); // Noncompliant
    public static IEnumerable<string> operator %(Operators right, Operators left) => true ? null : new List<string>(); // Noncompliant
    public static IEnumerable<string> operator ^(Operators right, Operators left) =>
        ((
        true
        ? default // Noncompliant
        : false
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
                return default; // Secondary
            }
        }
        else
        {
            return true
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
