using System;
using System.Threading;

class Program
{
    private object obj1 = new object();
    private object obj2 = new object();

    public object PublicObject = new object();

    public void Method1(string arg)
    {
        Monitor.Enter(obj1); // Compliant
        Console.WriteLine(arg.Length);
        Monitor.Exit(obj1);
    }

    public void Method2(string arg)
    {
        Monitor.Enter(obj1); // Compliant
        Console.WriteLine(arg.Length);
        Monitor.Exit(obj2);
    }

    public void Method3()
    {
        Monitor.Enter(obj1); // Compliant
        var a = new Action(() =>
        {
            Monitor.Exit(obj1);
        });
    }

    public void Method4(string arg)
    {
        var localObj = obj1;
        Monitor.Enter(localObj); // Compliant
        Console.WriteLine(arg.Length);
        Monitor.Exit(localObj);
    }

    public void Method5(string arg)
    {
        var localObj = obj1;
        Monitor.Enter(obj1); // Compliant
        Console.WriteLine(arg.Length);
        Monitor.Exit(localObj);
    }

    public void Method6(string arg, object paramObj)
    {
        paramObj = obj1;
        Monitor.Enter(paramObj); // Compliant
        Console.WriteLine(arg.Length);
        Monitor.Exit(paramObj);
    }

    public void Method7(string arg)
    {
        Monitor.Enter(obj1); // Compliant
        Console.WriteLine(arg.Length);
        var localObj = obj1;
        Monitor.Exit(localObj);
    }

    public void Method7(string arg, object paramObj)
    {
        Monitor.Enter(paramObj); // Compliant
        Console.WriteLine(arg.Length);
        Monitor.Exit(paramObj);
    }

    public void Method8(string arg, Program p1)
    {
        Monitor.Enter(p1.PublicObject); // Compliant
        Console.WriteLine(arg.Length);
        Monitor.Exit(p1.PublicObject);
    }

    public void Method9()
    {
        var a = new Action(() =>
        {
            Monitor.Enter(obj1); // Compliant
        });

        Monitor.Exit(obj1);
    }

    public void Method10()
    {
        var getObj = new Func<object>(() =>
        {
            return obj1;
        });

        Monitor.Enter(getObj());
        Monitor.Exit(getObj());
    }

    public void Method11()
    {
        var getObj = new Func<object>(() =>
        {
            return obj1;
        });

        Monitor.Enter(obj1);
        Monitor.Exit(getObj());
    }

    public void Method12()
    {
        Monitor.Enter(obj1); // Compliant
        var a = new Action(() =>
        {
            Monitor.Exit(obj1);
        });

        a();
    }

    public void Method13(string arg)
    {
        Monitor.Exit(obj1);
        Console.WriteLine(arg.Length);
        Monitor.Enter(obj1); // Compliant
    }

    public void Method14(string arg)
    {
        Monitor.Exit(obj1);
        Console.WriteLine(arg.Length);
        Monitor.Enter(obj1); // Compliant
        Console.WriteLine(arg.Length);
        Monitor.Exit(obj1);
    }
}
