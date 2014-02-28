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

        if (i = 0) {}      // Compliant
        if (i == 0) i = 2; // Compliant

        if (!string.IsNullOrEmpty(result)) result = result + separator; // Compliant
        asyncManager.Finished += delegate { finishTrigger.Fire(); };    // Compliant
        asyncManager.Finished += delegate { foo = 42; };                // Compliant
        asyncManager.Finished += (x) => x = 42;                         // Compliant
        asyncManager.Finished += (x) => { x = 42; };                    // Compliant
    }
}
