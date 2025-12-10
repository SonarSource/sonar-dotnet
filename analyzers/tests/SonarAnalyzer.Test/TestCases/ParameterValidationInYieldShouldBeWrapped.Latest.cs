using System;
using System.Collections.Generic;
using System.Threading.Tasks;

public static class InvalidCases
{
    public static IEnumerable<string> YieldReturn(string something) // Noncompliant
    {
        ArgumentNullException.ThrowIfNull(something);
//      ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^ Secondary

        yield return something;
    }

    public static IEnumerable<int> YieldBreak(string something) // Noncompliant
    {
        ArgumentNullException.ThrowIfNull(something);
//      ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^ Secondary

        yield break;
    }

    public static IEnumerable<int> ThrowExpression(string something) // FN #6369
    {
        _ = something ?? throw new ArgumentNullException();

        yield break;
    }

    // For details, check https://github.com/SonarSource/sonar-dotnet/pull/6624.
    public static async IAsyncEnumerable<int> AsyncThenYield(object arg) // Noncompliant
    {
        if (arg is null)
        {
            throw new ArgumentException(nameof(arg));
//                ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^ Secondary
        }

        var res = await Task.Run(() => 42);
        yield return res;
    }

    // For details, check https://github.com/SonarSource/sonar-dotnet/pull/6624.
    public static async IAsyncEnumerable<int> NestedAsyncThenYield(object arg) // Noncompliant
    {
        if (arg is null)
        {
            throw new ArgumentException(nameof(arg));
//                ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^ Secondary
        }

        var res = (await Task.Run(() => 42)).GetHashCode();
        yield return res;
    }
}

public static class ValidCases
{
    public static IEnumerable<string> Foo(string something) // Compliant - split into 2 methods
    {
        ArgumentNullException.ThrowIfNull(something);

        return FooIterator(something);
    }

    private static IEnumerable<string> FooIterator(string something)
    {
        yield return something;
    }
}

public interface IInvalidCases
{
    public static virtual IEnumerable<string> Foo(string something) // Noncompliant {{Split this method into two, one handling parameters check and the other handling the iterator.}}
    {
        if (something == null)
        { throw new ArgumentNullException(nameof(something)); } // Secondary

        yield return something;
    }
}

class InlineArrays
{
    IEnumerable<int> Invalid(Buffer? buffer)     // Noncompliant
    {
        if (buffer == null)
            throw new ArgumentNullException(nameof(buffer)); // Secondary

        foreach (var item in new Buffer())
            yield return item;
    }

    IEnumerable<int> Valid(Buffer? buffer)       // Compliant
    {
        if (buffer == null)
            throw new ArgumentNullException(nameof(buffer));
        return Generator(buffer.Value);

        IEnumerable<int> Generator(Buffer buffer)
        {
            foreach (var item in buffer)
                yield return item;
        }
    }

    [System.Runtime.CompilerServices.InlineArray(10)]
    struct Buffer
    {
        int arrayItem;
    }
}

public static class NewExtensions
{
    extension<TSource>(IEnumerable<TSource> source)
    {
        public IEnumerable<TSource> Noncompliant(Func<TSource, bool> predicate)               // Noncompliant
        {
            if (source is null) { throw new ArgumentNullException(nameof(source)); }        // Secondary
            if (predicate is null) { throw new ArgumentNullException(nameof(predicate)); }  // Secondary

            foreach (var element in source)
            {
                if (!predicate(element))
                { break; }
                yield return element;
            }
        }

        public IEnumerable<TSource> Compliant(Func<TSource, bool> predicate)
        {
            if (source == null)
            { throw new ArgumentNullException(nameof(source)); }
            if (predicate == null)
            { throw new ArgumentNullException(nameof(predicate)); }
            return CompliantIterator<TSource>(source, predicate);
        }

        private static IEnumerable<TSource> CompliantIterator(IEnumerable<TSource> target, Func<TSource, bool> predicate)
        {
            foreach (TSource element in target)
            {
                if (!predicate(element))
                    break;
                yield return element;
            }
        }
    }
}
