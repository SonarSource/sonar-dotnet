using System;
using System.Threading;

class Program
{
    private object obj = new object();

    public void Method1(string b)
    {
        Monitor.Enter(obj); // FN
        try
        {
            Console.WriteLine(b.Length);
        }
        catch (Exception)
        {
            Monitor.Exit(obj);
            throw;
        }
    }

    public void Method2(string b)
    {
        Monitor.Enter(obj);
        try
        {
            Console.WriteLine(b.Length);
        }
        catch (Exception)
        {
            throw;
        }
        finally
        {
            Monitor.Exit(obj);
        }
    }

    public void Method3(bool b)
    {
        Monitor.Enter(obj); // FN

        if (b)
        {
            throw new Exception();
        }

        Monitor.Exit(obj);
    }
}
