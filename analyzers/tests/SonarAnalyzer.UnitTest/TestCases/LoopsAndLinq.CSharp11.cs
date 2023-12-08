using System;
using System.Collections.Generic;

class IsPatternTests
{
    void ListPattern(List<int[]> list)
    {
        foreach (int[] array in list) // Noncompliant
        {
            if (array is [1, 2, 3]) // Secondary
            {
                Console.WriteLine("Pattern match successful");
            }
        }

        foreach (var array in list) // Compliant, do not raise on VarPattern in ListPattern
        {
            if (array is [1, var x, var z])
            {
                Console.WriteLine("Pattern match successful");
            }

        }

        foreach (var array in list) // Compliant, do not raise on declaration statements in ListPattern
        {
            if (array is [1, ..] local)
            {
                Console.WriteLine("Pattern match successful");
            }

        }
    }
}

// https://github.com/SonarSource/sonar-dotnet/issues/7730
class Repro_7730
{
    void SpansAndLogicalPatterns(Span<char> s, ReadOnlySpan<char> ros)
    {
        foreach (var c in s)                   // Noncompliant, FP: iterable but not enumerable, nor queriable
            if (c is ' ') { }                  // Secondary
        foreach (var c in s)                   // Noncompliant, FP
            if (c is not ' ') { }              // Secondary
        foreach (var c in s)                   // Noncompliant, FP
            if (c is ' ' or '\n' or '\r') { }  // Secondary

        foreach (var c in ros)                 // Noncompliant, FP
            if (c is ' ') { }                  // Secondary
        foreach (var c in ros)                 // Noncompliant, FP
            if (c is not ' ') { }              // Secondary
        foreach (var c in ros)                 // Noncompliant, FP
            if (c is ' ' or '\n' or '\r') { }  // Secondary
    }

    void SpansAndLogicalOperators(Span<char> s, ReadOnlySpan<char> ros)
    {
        foreach (var c in s)                             // Compliant: iterable but not enumerable, nor queriable
            if (c == ' ') { }
        foreach (var c in s)                             // Compliant
            if (c != ' ') { }
        foreach (var c in s)                             // Compliant
            if (c == ' ' || c == '\n' || c == '\r') { }

        foreach (var c in ros)                           // Compliant
            if (c == ' ') { }
        foreach (var c in ros)                           // Compliant
            if (c != ' ') { }
        foreach (var c in ros)                           // Compliant
            if (c == ' ' || c == '\n' || c == '\r') { }
    }

    void IterableNotEnumerableAndLogicalPatterns(IterableNotEnumerable s)
    {
        foreach (var c in s)                   // Noncompliant, FP: iterable but not enumerable, nor queriable
            if (c is ' ') { }                  // Secondary
        foreach (var c in s)                   // Noncompliant, FP
            if (c is not ' ') { }              // Secondary
        foreach (var c in s)                   // Noncompliant, FP
            if (c is ' ' or '\n' or '\r') { }  // Secondary
    }

    void EnumerableNotCollectionAndLogicalPatterns()
    {
        foreach (var c in EnumerableNotCollection()) // Noncompliant
            if (c is ' ') { }                        // Secondary
        foreach (var c in EnumerableNotCollection()) // Noncompliant
            if (c is not ' ') { }                    // Secondary
        foreach (var c in EnumerableNotCollection()) // Noncompliant
            if (c is ' ' or '\n' or '\r') { }        // Secondary

        IEnumerable<char> EnumerableNotCollection()
        {
            yield return 'a';
            yield return 'b';
        }
    }

    class IterableNotEnumerable
    {
        public IEnumerator<char> GetEnumerator()
        {
            yield return 'a';
            yield return 'b';
        }
    }
}

// https://github.com/SonarSource/sonar-dotnet/issues/7776
class Repro_7776
{
    class ForEach_WithCustomList
    {
        void Test(CustomList s)
        {
            foreach (var c in s)                   // Noncompliant
                if (c is ' ') { }                  // Secondary
            foreach (var c in s)                   // Noncompliant
                if (c is not ' ') { }              // Secondary
            foreach (var c in s)                   // FN, equivalent to c is ' '
                if (c == ' ') { }
            foreach (var c in s)                   // FN, equivalent to c is not ' '
                if (c != ' ') { }
        }

        class CustomList : List<char>
        {
        }
    }
}

// https://github.com/SonarSource/sonar-dotnet/issues/8430
class Repro_8430
{
    void Visit(ReadOnlySpan<char> x) { }

    void Test(IEnumerable<ReadOnlyMemory<char>> tokens)
    {
        // rule suggests "Select(token => token.Span)", but "Span" is ref-struct, so it cannot be used a type parameter.
        foreach (ReadOnlyMemory<char> token in tokens) // Noncompliant FP
        {
            ReadOnlySpan<char> chars = token.Span;
            Visit(chars);
        }
    }
}
