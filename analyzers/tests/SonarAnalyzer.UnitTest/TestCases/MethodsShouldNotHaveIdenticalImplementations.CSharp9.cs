using System;

void Method1()
{
    string s = "test";
    Console.WriteLine("Result: {0}", s);
}

void Method2() // Noncompliant {{Update this method so that its implementation is not identical to 'Method1'.}}
{
    string s = "test";
    Console.WriteLine("Result: {0}", s);
}

void Method3() // Noncompliant {{Update this method so that its implementation is not identical to 'Method1'.}}
{
    string s = "test";
    Console.WriteLine("Result: {0}", s);
}

void Method4()
{
    Console.WriteLine("Result: 0");
}
