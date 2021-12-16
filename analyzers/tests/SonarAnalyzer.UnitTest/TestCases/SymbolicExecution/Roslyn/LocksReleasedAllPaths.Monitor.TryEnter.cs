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

    public void Method4(bool condition)
    {
        bool isAcquired = false;
        Monitor.TryEnter(obj, 42, ref isAcquired); // FN
        if (condition)
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

    public void Method6(bool condition)
    {
        bool isAcquired = false;
        Monitor.TryEnter(obj, new TimeSpan(42), ref isAcquired); // FN
        if (condition)
        {
            Monitor.Exit(obj);
        }
    }

    public void Method7()
    {
        bool isAcquired = Monitor.TryEnter(obj, 42); // FN

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

    public void Method9()
    {
        Monitor.TryEnter(obj); // Compliant
        Monitor.Exit(obj);
    }

    public void Method10()
    {
        Monitor.Exit(obj);
        Monitor.TryEnter(obj); // Compliant
    }

    public void Method11()
    {
        switch (Monitor.TryEnter(obj)) // Compliant
        {
            case true:
                Monitor.Exit(obj);
                break;
            default:
                break;
        }
    }

    public void Method12()
    {
        switch (Monitor.TryEnter(obj)) // FN
        {
            case false:
                Monitor.Exit(obj);
                break;
            default:
                break;
        }
    }

    public void Method13(bool condition)
    {
        bool isAcquired = false;
        Monitor.TryEnter(obj, 42, ref isAcquired);
        if (isAcquired)
        {
            Monitor.Exit(obj);
        }
    }
}
