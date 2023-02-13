using System;
using System.Linq;

public class Program
{
    int MyInt { get; }
    int Tim { get; }
    int MyProperty
    {
        get
        {
            int i;      // Noncompliant {{Expected Identifier to be 'tim' but found 'i'!}}
//              ^
            int tim;
            i = MyInt;  // Noncompliant {{Expected Identifier to be 'Tim' but found 'MyInt'!}}
            tim = Tim;
            return i;
        }
    }
    void Basics()
    {
        int i;      // Noncompliant
        int tim;
        int Tim;    // Noncompliant
        i = 5;
    }
    void Porperty()
    {
        var tim = MyInt;    // Noncompliant
        _ = 5 == MyInt;     // Noncompliant
        tim = Tim;
    }
    void At()
    {
        int @tim;
        tim = Tim;
        int @i; // Noncompliant
        //  ^^
    }
    void ForEach()
    {
        for (int i; ;)  // Noncompliant
            foreach (var t in Enumerable.Empty<int>())  // Noncompliant
            { }
    }
    void Out()
    {
        local(out var i);   // Noncompliant
        void local(out int j)
        {
            j = 5;
        }
    }
    void Tuple()
    {
        var (i, tim) = (5, 6);  // Noncompliant
        //   ^
    }
    void Tuple2()
    {
        (var i, var tim) = (5, 6);  // Noncompliant
        //   ^
    }
    void MultiDeclaration()
    {
        int tim, i;  // Noncompliant
    }
    void MultiDeclaration2()
    {
        int i, tim;  // Noncompliant
    }
    void InvalidSyntax()
    {
        int tim,;   // Error
    }
}
