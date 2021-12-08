using System;
using System.Threading;

class Program
{
    private object obj = new object();

    public void Method1()
    {
        if (Monitor.TryEnter(obj)) // FN
        {
        }
        else
        {
            Monitor.Exit(obj);
        }
    }

    public void Method2()
    {
        if (Monitor.TryEnter(obj)) // Compliant
        {
            Monitor.Exit(obj);
        }
        else
        {
        }
    }

    public void Method3()
    {
        if (Monitor.TryEnter(obj, 42)) // FN
        {
        }
        else
        {
            Monitor.Exit(obj);
        }
    }

    public void Method4(bool b)
    {
        bool isAcquired = false;
        Monitor.TryEnter(obj, 42, ref isAcquired);
        if (b)
        {
            Monitor.Exit(obj);
        }
    }

    public void Method5()
    {
        if (Monitor.TryEnter(obj, new TimeSpan(42))) // FN
        {
        }
        else
        {
            Monitor.Exit(obj);
        }
    }

    public void Method6(bool b)
    {
        bool isAcquired = false;
        Monitor.TryEnter(obj, new TimeSpan(42), ref isAcquired);
        if (b)
        {
            Monitor.Exit(obj);
        }
    }

    public void Method7()
    {
        bool isAcquired = Monitor.TryEnter(obj, 42);

        if (isAcquired)
        {
        }
        else
        {
            Monitor.Exit(obj);
        }
    }

    public void Method8()
    {
        bool isAcquired = Monitor.TryEnter(obj, 42); // Compliant

        if (isAcquired)
        {
            Monitor.Exit(obj);
        }
    }
}
