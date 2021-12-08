using System;
using System.Collections.Generic;
using System.Threading;

class Program
{
    private object obj = new object();

    public void Method1()
    {
        Monitor.Enter(obj); // FN
        for (int i = 0; i < 10; i++)
        {
            if (i == 9)
            {
                Monitor.Exit(obj);
            }
        }
    }

    public void Method2()
    {
        Monitor.Enter(obj); // FN
        for (int i = 0; i < 10; i++)
        {
            break;
            if (i == 9)
            {
                Monitor.Exit(obj);
            }
        }
    }

    public void Method3()
    {
        Monitor.Enter(obj); // FN
        for (int i = 0; i < 10; i++)
        {
            if (i == 5)
            {
                break;
            }

            if (i == 9)
            {
                Monitor.Exit(obj);
            }
        }
    }

    public void Method4()
    {
        Monitor.Enter(obj); // FN
        for (int i = 0; i < 10; i++)
        {
            if (i == 10)
            {
                break;
            }

            if (i == 9)
            {
                Monitor.Exit(obj);
            }
        }
    }

    public void Method5()
    {
        Monitor.Enter(obj); // Compliant
        for (int i = 0; i < 10; i++)
        {
            if (i == 9)
            {
                continue;
            }

            if (i == 9)
            {
                Monitor.Exit(obj);
            }
        }
    }

    public void Method6(bool condition, byte[] array)
    {
        Monitor.Enter(obj); // FN
        foreach (var item in array)
        {         
            if (condition)
            {
                Monitor.Exit(obj);
            }
        }
    }

    public void Method6(bool condition, List<byte> array)
    {
        Monitor.Enter(obj); // FN
        while (array.Count < 42)
        {
            if (condition)
            {
                Monitor.Exit(obj);
            }
            array.RemoveAt(0);
        }
    }

    public void Method7(bool condition, List<byte> array)
    {
        Monitor.Enter(obj); // FN
        do
        {
            if (condition)
            {
                Monitor.Exit(obj);
            }
            array.RemoveAt(0);
        }
        while (array.Count < 42);
    }
}
