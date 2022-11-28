using System;
using System.Collections.Generic;
using System.Numerics;

class Operators : IAdditionOperators<Operators, Operators, IEnumerable<string>>
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
            return false 
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
