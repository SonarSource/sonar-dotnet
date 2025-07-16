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
