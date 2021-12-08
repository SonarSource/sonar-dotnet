using System;
using System.Threading;

class Program
{
    public void Method1(bool b)
    {
        SpinLock sl = new SpinLock(false);
        bool isAcquired = false;
        sl.Enter(ref isAcquired); // FN
        if (b)
        {
            sl.Exit();
        }
    }

    public void Method2(bool b)
    {
        SpinLock sl = new SpinLock(true);
        bool isAcquired = false;
        sl.Enter(ref isAcquired); // FN
        if (b)
        {
            sl.Exit(true);
        }
    }

    public void Method3(bool b)
    {
        SpinLock sl = new SpinLock(false);
        bool isAcquired = false;
        sl.TryEnter(ref isAcquired);
        if (b)
        {
            sl.Exit();
        }
    }

    public void Method4(bool b)
    {
        SpinLock sl = new SpinLock(false);
        bool isAcquired = false;
        sl.TryEnter(42, ref isAcquired);
        if (b)
        {
            sl.Exit();
        }
    }

    public void Method5(bool b)
    {
        SpinLock sl = new SpinLock(false);
        bool isAcquired = false;
        sl.TryEnter(new TimeSpan(42), ref isAcquired);
        if (b)
        {
            sl.Exit();
        }
    }

    public void Method6(string b)
    {
        bool isAcquired = false;
        SpinLock sl = new SpinLock(false);;
        sl.Enter(ref isAcquired);
        try
        {
            Console.WriteLine(b.Length);
        }
        finally
        {
            sl.Exit();
        }
    }
}
