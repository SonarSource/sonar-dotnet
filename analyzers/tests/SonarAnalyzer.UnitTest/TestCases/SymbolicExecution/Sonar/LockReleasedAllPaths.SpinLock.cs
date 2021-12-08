using System;
using System.Threading;

class Program
{
    public void Method1(bool condition)
    {
        SpinLock sl = new SpinLock(false);
        bool isAcquired = false;
        sl.Enter(ref isAcquired); // FN
        if (condition)
        {
            sl.Exit();
        }
    }

    public void Method2(bool condition)
    {
        SpinLock sl = new SpinLock(true);
        bool isAcquired = false;
        sl.Enter(ref isAcquired); // FN
        if (condition)
        {
            sl.Exit(true);
        }
    }

    public void Method3(bool condition)
    {
        SpinLock sl = new SpinLock(false);
        bool isAcquired = false;
        sl.TryEnter(ref isAcquired);
        if (condition)
        {
            sl.Exit();
        }
    }

    public void Method4(bool condition)
    {
        SpinLock sl = new SpinLock(false);
        bool isAcquired = false;
        sl.TryEnter(42, ref isAcquired);
        if (condition)
        {
            sl.Exit();
        }
    }

    public void Method5(bool condition)
    {
        SpinLock sl = new SpinLock(false);
        bool isAcquired = false;
        sl.TryEnter(new TimeSpan(42), ref isAcquired);
        if (condition)
        {
            sl.Exit();
        }
    }

    public void Method6(string condition)
    {
        bool isAcquired = false;
        SpinLock sl = new SpinLock(false);;
        sl.Enter(ref isAcquired);
        try
        {
            Console.WriteLine(condition.Length);
        }
        finally
        {
            sl.Exit();
        }
    }
}
