using System;

void Method1()
{
    string s = "test";
    Console.WriteLine("Result: {0}", s);
}

void Method2() // FN {{Update this method so that its implementation is not identical to 'Method1'.}}
{
    string s = "test";
    Console.WriteLine("Result: {0}", s);
}

void Method3() // FN {{Update this method so that its implementation is not identical to 'Method1'.}}
{
    string s = "test";
    Console.WriteLine("Result: {0}", s);
}

void Method4()
{
    Console.WriteLine("Result: 0");
}

public record Sample
{
    public void Method1()
    {
        string s = "test";
        Console.WriteLine("Result: {0}", s);
    }

    public void Method2() // FN {{Update this method so that its implementation is not identical to 'Method1'.}}
    {
        string s = "test";
        Console.WriteLine("Result: {0}", s);
    }

    public void Method3() // FN {{Update this method so that its implementation is not identical to 'Method1'.}}
    {
        string s = "test";
        Console.WriteLine("Result: {0}", s);
    }

    public void Method4()
    {
        Console.WriteLine("Result: 0");
    }
}
