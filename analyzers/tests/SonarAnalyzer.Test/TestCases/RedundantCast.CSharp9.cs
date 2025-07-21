#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;

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
