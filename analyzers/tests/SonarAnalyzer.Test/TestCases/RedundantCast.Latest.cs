using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

#nullable enable

public class InvocationTests
{
    public static void Invocations()
    {
        var ints = new int[1];
        var objects = new object[1];
        var moreInts = new int[1][];
        var moreObjects = new object[1][];
        ints?.Cast<int>();              // Noncompliant
        objects?.Cast<int>();           // Compliant
        moreInts[0].Cast<int>();        // Noncompliant
        moreObjects[0].Cast<int>();     // Compliant
        moreInts[0]?.Cast<int>();       // Noncompliant
        moreObjects[0]?.Cast<int>();    // Compliant
        GetInts().Cast<int>();          // Noncompliant
        GetObjects().Cast<int>();       // Compliant
        GetInts()?.Cast<int>();         // Noncompliant
        GetObjects()?.Cast<int>();      // Compliant
        Enumerable.Cast<int>();         // Error [CS7036] - overload resolution failure
    }

    public static int[] GetInts() => null;
    public static object[] GetObjects() => null;
}

// https://github.com/SonarSource/sonar-dotnet/issues/3273
public class CastOnNullable
{
    public static IEnumerable<string> Array()
    {
        var nullableStrings = new string?[] { "one", "two", null, "three" };
        return nullableStrings.OfType<string>(); // Compliant - filters out the null
    }

    public void Tuple()
    {
        _ = (a: (string?)"", b: "");    // Compliant
    }

    public void ValueTypes(int nonNullable, int? nullable)
    {
        _ = (int?)nonNullable;  // Compliant
        _ = (int?)nullable;     // Noncompliant
        _ = (int)nonNullable;   // Noncompliant
        _ = (int)nullable;      // Compliant
    }
}

// https://github.com/SonarSource/sonar-dotnet/issues/6438
public class AnonTypes
{
    public void Simple(string nonNullable, string? nullable)
    {
        _ = new { X = (string?)nonNullable };   // Compliant
        _ = new { X = (string?)nullable };      // Noncompliant
        _ = new { X = (string)nonNullable };    // Noncompliant
        _ = new { X = (string)nullable };       // Compliant
    }

    public void Array(string nonNullable, string? nullable)
    {
        _ = new[] { new { X = (string?)nonNullable }, new { X = (string?)null } };  // Compliant
        _ = new[] { new { X = (string?)nullable }, new { X = (string?)null } };     // Noncompliant
        _ = new[] { new { X = (string?)nonNullable } };                             // Compliant
        _ = new[] { new { X = (string?)nullable } };                                // Noncompliant
        _ = new[] { new HoldsObject(new { X = (string?)nonNullable }) };            // Compliant
        _ = new[] { new HoldsObject(new { X = (string?)nullable }) };               // Noncompliant
    }

    public void SwitchExpression(string nonNullable, string? nullable)
    {
        _ = true switch
        {
            true => new { X = (string?)nonNullable },   // Compliant
            false => new { X = (string?)null }          // Compliant
        };
        _ = true switch
        {
            true => new { X = (string?)nullable },   // Noncompliant
            false => new { X = (string?)null }          // Compliant
        };
    }
}

internal class HoldsObject
{
    object O { get; }
    public HoldsObject(object o)
    {
        O = o;
    }
}

// https://github.com/SonarSource/sonar-dotnet/issues/8413
// See also https://github.com/SonarSource/sonar-dotnet/pull/7036
class Repro_8413
{
    public IEnumerable<string> GetNonNullStringsDirectCast(IEnumerable<string?> strings)
    {
        return (IEnumerable<string>)strings.Where(s => s != null); // Compliant
    }

    public IEnumerable<string> GetNonNullStringsMethodCast(IEnumerable<string?> strings)
    {
        return strings.Where(s => s != null).Cast<string>(); // Compliant
    }

    public IEnumerable<string> GetNonNullStringsAsCast(IEnumerable<string?> strings)
    {
        return strings.Where(s => s != null) as IEnumerable<string>;  // Compliant
    }
}

public static class Test
{
    public static IEnumerable<T> WhereNotNull<T>(IEnumerable<T?> result) =>
        result.OfType<T>();

    public static IEnumerable<T> Another<T>(IEnumerable<T> result) =>
        result.OfType<T>();  // Noncompliant

    public static void SpanConversion()
    {
        const int bufferSize = 1024;
        var buffer1 = (Span<char>)stackalloc char[bufferSize]; // Compliant, stackalloc returns char* without the cast
        var buffer2 = (Span<char>)stackalloc char[] { 'c' };
        var buffer3 = (Span<char>)stackalloc [] { 'c' }; // Implicit stackalloc array creation expression
        var buffer4 = (Span<char>)(stackalloc char[bufferSize]); 
        unsafe
        {
            var buffer5 = (char*)stackalloc char[bufferSize]; // Error [CS8346] (This would have been a FN, but it doesn't compile. See also https://github.com/dotnet/roslyn/issues/23995)
        }
    }
}

public class DefaultLiteralTest
{
    // https://sonarsource.atlassian.net/browse/NET-2198
    public static void NET2198()
    {
        _ = new Result<Foo>((Foo)default!); // Compliant, without the cast the overload resolution of the ctor would be ambiguous. Without the null supression (!) two warnings would be raised:
                                            // CS8600: Converting null literal or possible null value to non-nullable type.
                                            // CS8625: Cannot convert null literal to non-nullable reference type.
    }
    public record Result<TR>
    {
        public Result(TR value) { }
        public Result(DefaultLiteralTest other) { }
    }
    public class Foo { }
}

static class Extensions
{
    extension(IEnumerable source)
    {
        public IEnumerable<T1> OfType<T1, T2>() => source.OfType<T1>();
        public IEnumerable<T1> Cast<T1, T2>() => source.Cast<T1>();
        public int OfType<T1, T2, T3>() => 0;
        public int Cast<T1, T2, T3>() => 0;
    }

    static void Method(IEnumerable<int> intValues, List<string> stringValues)
    {
        intValues.OfType<int, string>();        // FN NET-2746
        intValues.Cast<int, string>();          // FN NET-2746
        stringValues.OfType<string, string>();
        stringValues.Cast<string, string>();    // FN NET-2746

        intValues.OfType<int, int, int>();
        intValues.Cast<int, int, int>();
    }
}

class FieldKeyword
{
    int Property
    {
        get => (int)field;          // Noncompliant
        set => field = (int)value;  // Noncompliant
    }
}

class ImplicitSpanConversion
{
    void Method(string[] array)
    {
        Span<string> span = (Span<string>)array;                                        // FN NET-2747
        ReadOnlySpan<string> readOnlySpan = (ReadOnlySpan<string>)array;                // FN NET-2747
        readOnlySpan = (ReadOnlySpan<string>)span;                                      // FN NET-2747
        ReadOnlySpan<object> objectReadOnlySpan = (ReadOnlySpan<object>)readOnlySpan;   // FN NET-2747
        ReadOnlySpan<char> chars = (ReadOnlySpan<char>)"string";                        // FN NET-2747
    }
}
