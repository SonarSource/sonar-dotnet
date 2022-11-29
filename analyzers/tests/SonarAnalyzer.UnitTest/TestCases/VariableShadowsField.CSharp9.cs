using System;

Console.WriteLine("Hi!");

record Rec
{
    public int @int;
    public int foo;
    public int bar;

    public void DoSomething()
    {
        int foo = 0; // Noncompliant
//          ^^^
        int @int = 42; // Noncompliant

        foreach (var bar in new[] { 1, 2 }) // Noncompliant
        {
        }
    }

    public void PropertyPattern()
    {
        _ = new ArgumentException() is
        {
            Message: { } bar, // Noncompliant
        };
    }
}
