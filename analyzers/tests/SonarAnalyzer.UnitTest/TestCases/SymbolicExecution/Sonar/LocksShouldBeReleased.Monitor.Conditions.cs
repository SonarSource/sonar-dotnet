using System;
using System.Threading;

class Program
{
    private object obj = new object();

    public void Method1(bool b)
    {
        Monitor.Enter(obj); // FN
        if (b)
        {
            Monitor.Exit(obj);
        }
    }

    public void Method2(bool b)
    {
        Monitor.Enter(obj); // FN
        switch (b)
        {
            case true:
                Monitor.Exit(obj);
                break;
            default:
                break;
        }
    }

    public void Method3(bool b)
    {
        bool isAcquired = false;
        Monitor.Enter(obj, ref isAcquired); // FN
        if (b)
        {
            Monitor.Exit(obj);
        }
    }

    public int MyProperty
    {
        set
        {
            Monitor.Enter(obj); // FN
            if (value == 42)
            {
                Monitor.Exit(obj);
            }
        }
    }
}
