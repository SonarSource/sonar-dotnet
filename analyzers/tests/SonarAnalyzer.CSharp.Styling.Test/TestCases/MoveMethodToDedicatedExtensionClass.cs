using System;
using System.Collections;
using System.Collections.Generic;

using MyString = string;

static class StringExtensions
{
    public static string ToUpperCase(this string str)
    {
        return str.ToUpper();
    }

    public static string String(this System.String str) => str;

    public static string Generic<T>(this System.String str, T other) => str + other.ToString();

    public static string ToLowerCase(this MyString str) => str.ToLower();

    public static short Short(this short nb) => nb; // Noncompliant {{Move this extension method to the Int16Extensions class.}}
    //                  ^^^^^
    public static int Int(this int nb) => nb; // Noncompliant {{Move this extension method to the Int32Extensions class.}}
    //                ^^^
    public static long Long(this long nb) => nb; // Noncompliant {{Move this extension method to the Int64Extensions class.}}

    public static short Int16(this System.Int16 nb) => nb; // Noncompliant {{Move this extension method to the Int16Extensions class.}}
    public static int Int32(this System.Int32 nb) => nb; // Noncompliant {{Move this extension method to the Int32Extensions class.}}
    public static long Int64(this System.Int64 nb) => nb; // Noncompliant {{Move this extension method to the Int64Extensions class.}}
    public static System.Int128 Int128(this System.Int128 nb) => nb; // Noncompliant {{Move this extension method to the Int128Extensions class.}}

    public static void ValueTuple(this (int, int) tuple) { } // Noncompliant {{Move this extension method to the ValueTupleExtensions class.}}
    public static void ValueTruple(this (int, int, int) truple) { } // Noncompliant {{Move this extension method to the ValueTupleExtensions class.}}
}

class Generic { }

static class GenericExtensions
{
    public static T Generic<T>(this T t) => t; // Noncompliant {{Move this extension method to the ObjectExtensions class.}}
    public static T[] Generic<T>(this T[] t) => t; // Noncompliant {{Move this extension method to the ObjectExtensions class.}}

    public static T ConstrainedGeneric<T>(this T t) where T : Generic => t;

    // Error@+1 [CS0450]
    public static T InvalidConstrainedGeneric<T>(this T t) where T : class, Generic => t; // Noncompliant - the where clause is invalid and thus we cannot find Generic
}

static class IEnumerableExtensions
{
    public static IEnumerable<T> ToEnumerable<T>(this T item) // Noncompliant {{Move this extension method to the ObjectExtensions class.}}
    {
        yield return item;
    }

    public static IEnumerable<T> GenericExtension<T>(this IEnumerable<T> enumerable) => enumerable;

    public static T ComplexConstrainedGeneric<T>(this T t) where T : class, IEnumerable => t;

    public static T TwoConstrainedGeneric<T>(this T t) where T : class, IEnumerable, ICloneable => t;
}

static class ICloneableExtensions
{
    public static T TwoConstrainedGeneric<T>(this T t) where T : struct, IEnumerable, ICloneable => t;
}

static class NotClonableNorEnumerableExtensions
{
    public static T TwoConstrainedGeneric<T>(this T t) where T : class, IEnumerable, ICloneable => t; // Noncompliant {{Move this extension method to the IEnumerableExtensions or ICloneableExtensions class.}}
    public static T DifferentOrder<T>(this T t) where T : class, ICloneable, IEnumerable => t; // Noncompliant {{Move this extension method to the ICloneableExtensions or IEnumerableExtensions class.}}
}

static class DictionaryExtensions
{
    public static Dictionary<T, U> GenericExtension<T, U>(this Dictionary<T, U> dictionary) => dictionary;
}

class NotAnExtensionClass // Error [CS1106]
{
    public static int Int(this int nb) => nb; // Noncompliant
}

class IntExtensions // Error [CS1106]
{
    public static int Int(this int nb) => nb; // Noncompliant - The class is not static
}

class IntExtensions<T> // Error [CS1106]
{
    public static int Int(this int nb) => nb; // Noncompliant - The class is not static
}

static class InvalidExtensions
{
    // Error@+1 [CS1103]
    public static void Dynamic(this dynamic dynamic) { } // Noncompliant {{Move this extension method to the dynamicExtensions class.}}

    // Error@+1 [CS1103]
    unsafe public static void Pointer(this int* pointer) { } // Noncompliant {{Move this extension method to the ObjectExtensions class.}}
}
