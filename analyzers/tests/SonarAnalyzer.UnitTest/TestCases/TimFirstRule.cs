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
            int i;  // Noncompliant {{Tim's first rule!}}
            int tim;
            i = 5;
            tim = i;
            return i;
        }
    }
    void Basics()
    {
        int i;      // Noncompliant
        int tim;
        int Tim;    // Noncompliant
        i = 5;
        tim = i;
    }
    void At()
    {
        int @tim;
    }
    void ForEach()
    {
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
    }
    void MultiDeclaration()
    {
        int tim, i;  // Noncompliant
    }
    void MultiDeclaration2()
    {
        int i, tim;  // Noncompliant
    }
}
