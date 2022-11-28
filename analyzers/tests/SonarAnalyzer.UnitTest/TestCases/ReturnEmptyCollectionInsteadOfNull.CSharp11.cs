using System;
using System.Collections.Generic;
using System.Numerics;

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
