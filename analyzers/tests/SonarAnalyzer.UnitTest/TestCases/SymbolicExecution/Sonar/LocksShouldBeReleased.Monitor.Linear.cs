using System;
using System.Threading;

class Program
{
    private object obj = new object();

    public void Method1(string b)
    {
        Monitor.Enter(obj); // Compliant
        Console.WriteLine(b.Length);
        Monitor.Exit(obj);
    }

    public void Method2()
    {
        Monitor.Enter(obj); // FN
        var a = new Action(() =>
        {
            Monitor.Exit(obj);
        });
    }
}
