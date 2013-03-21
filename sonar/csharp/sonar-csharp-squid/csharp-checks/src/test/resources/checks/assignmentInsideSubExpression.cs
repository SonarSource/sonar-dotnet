using System;

class Program
{
    static void foo(int a)
    {
    }

    static void foo(bool a)
    {
    }

    static void foo(Func<int, int> f)
    {
    }

    static void Main(string[] args)
    {
        int i = 0;          // Compliant
        foo(i = 42);        // Non-Compliant

        i = 42;             // Compliant
        foo(i == 42);       // Compliant

        foo(
            (int x) =>
            {
                int a;
                a = 42;     // Compliant
                return a;
            });
    }
}
