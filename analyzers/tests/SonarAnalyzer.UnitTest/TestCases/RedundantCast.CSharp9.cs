using System.Collections.Generic;
using System.Linq;

public static class Test
{
    public static IEnumerable<T> WhereNotNull<T>(IEnumerable<T?> result) =>
        result.OfType<T>();

    public static IEnumerable<T> Another<T>(IEnumerable<T> result) =>
        result.OfType<T>();  // Noncompliant
}
