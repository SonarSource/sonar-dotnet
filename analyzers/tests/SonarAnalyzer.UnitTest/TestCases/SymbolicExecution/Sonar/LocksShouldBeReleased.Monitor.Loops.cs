using System;
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
}
