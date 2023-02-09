using System;

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
