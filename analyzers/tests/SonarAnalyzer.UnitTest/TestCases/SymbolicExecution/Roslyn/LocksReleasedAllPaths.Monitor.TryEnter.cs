using System;
using System.Threading;

class Program
{
    private object obj = new object();

    public void Method1()
    {
        if (Monitor.TryEnter(obj)) // Noncompliant
        {
        }
        else
        {
            Monitor.Exit(obj);
        }
    }

    public void Method2()
    {
        if (Monitor.TryEnter(obj)) // Noncompliant FP, we don't track the boolean result yet
        {
            Monitor.Exit(obj);
        }
        else
        {
        }
    }

    public void Method3()
    {
        if (Monitor.TryEnter(obj, 42)) // Noncompliant
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
        Monitor.TryEnter(obj, 42, ref isAcquired); // Noncompliant
        if (condition)
        {
            Monitor.Exit(obj);
        }
    }

    public void Method5()
    {
        if (Monitor.TryEnter(obj, new TimeSpan(42))) // Noncompliant
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
        Monitor.TryEnter(obj, new TimeSpan(42), ref isAcquired); // Noncompliant
        if (condition)
        {
            Monitor.Exit(obj);
        }
    }

    public void Method7()
    {
        bool isAcquired = Monitor.TryEnter(obj, 42); // Noncompliant

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
        bool isAcquired = Monitor.TryEnter(obj, 42); // Noncompliant FP, isAcquired is not tracked properly yet

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
        Monitor.TryEnter(obj); // Noncompliant {{Unlock this lock along all executions paths of this method.}}
    }

    public void Method11()
    {
        switch (Monitor.TryEnter(obj)) // Noncompliant FP, bool result is not tracked properly yet
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
        switch (Monitor.TryEnter(obj)) // Noncompliant
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
        Monitor.TryEnter(obj, 42, ref isAcquired);  // Noncompliant FP, isAcquired is not tracked properly yet
        if (isAcquired)
        {
            Monitor.Exit(obj);
        }
    }
}
