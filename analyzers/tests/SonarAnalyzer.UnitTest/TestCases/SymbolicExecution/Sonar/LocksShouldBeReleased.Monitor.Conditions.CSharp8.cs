using System;
using System.Threading;

class Program
{
    private object obj1 = new object();
    private object obj2 = new object();

    public void Method1(bool condition)
    {
        var lockObj = condition switch
        {
            true => obj1,
            false => obj2,
        };

        Monitor.Enter(lockObj); // FN
        if (condition)
        {
            Monitor.Exit(lockObj);
        }
    }

    public void Method2(bool condition)
    {
        var lockObj = condition switch
        {
            true => obj1,
            false => obj2,
        };

        Monitor.Enter(lockObj); // Compliant
        if (!condition)
        {
            Monitor.Exit(obj2);
        }
    }
}
