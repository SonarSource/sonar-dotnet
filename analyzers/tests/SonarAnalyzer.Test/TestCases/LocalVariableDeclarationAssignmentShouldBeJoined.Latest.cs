using System;
using System.IO;

class LocalVariableDeclarationAssignmentShouldBeJoined
{
    void UsingDeclaration()
    {
        using var stream = File.OpenRead("test"); // Compliant - using declaration
    }

    void RefLocal(int[] array)
    {
        ref int r = ref array[0];                 // Compliant - already initialized
        r = ref array[1];
    }

    void TupleType()
    {
        (int, string) t;                          // Noncompliant
        t = (1, "a");

        (int A, string B) named;                  // Noncompliant
        named = (A: 1, B: "a");
    }

    void TupleDeconstruction()
    {
        int x;                                    // Compliant - next statement is a declaration
        string s;                                 // Noncompliant
        (x, s) = (1, "a");
    }

    void MixedDeconstruction()
    {
        int y = 0;
        int x;                                    // Noncompliant - can be joined into "(int x, y) = ..." since C# 10
        (x, y) = (1, 2);
    }

    void SpanLocal(int[] array)
    {
        Span<int> span;                           // Noncompliant
        span = array;

        scoped Span<int> scopedSpan;              // Noncompliant
        scopedSpan = array;

        ReadOnlySpan<char> chars;                 // Noncompliant
        chars = "abc".AsSpan();
    }

    void NullableReferenceType()
    {
        string? s;                                // Noncompliant
        s = null;
    }
}
