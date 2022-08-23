using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;

class Conmpliant
{
    async IAsyncEnumerable<int> Yields([EnumeratorCancellation] CancellationToken token) // Compliant
    {
        yield return 13;
    }
    async IAsyncEnumerable<int> YieldsWithOptionalCancellation([EnumeratorCancellation] CancellationToken token = default) // Compliant
    {
        yield return 13;
    }
    public static async IAsyncEnumerable<int> YieldsWithMultipleModifiers([EnumeratorCancellation] CancellationToken token) // Compliant
    {
        yield return 13;
    }
    async IAsyncEnumerator<int> Iterates(CancellationToken token) // Compliant
    {
        yield return 13;
    }
    IAsyncEnumerable<int> WithoutYielding(CancellationToken token) // Compliant
    {
        return null;
    }
    IAsyncEnumerable<int> WithoutYielding() // Compliant
    {
        return null;
    }
    IAsyncEnumerable<int> WithoutBody() => null; // Compliant
}

class Noncompliant
{
    async IAsyncEnumerable<int> Yields(CancellationToken token) // Noncompliant
    {
        yield return 42;
    }
    async IAsyncEnumerable<int> YieldsWithoutCancelation() // Noncompliant
    {
        yield return 42;
    }
    internal static async IAsyncEnumerable<int> YieldsWithMultipleModifiers() // Noncompliant
    {
        yield return 42;
    }
    async IAsyncEnumerable<int> YieldsWithMisplacedDecoration([EnumeratorCancellation] int number) // Noncompliant
    {
        yield return number;
    }
}
