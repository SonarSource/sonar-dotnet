using System;
using System.Threading;

class Program
{
    private object obj = new object();
    private object other = new object();

    public object PublicObject = new object();

    public void Method1(string arg)
    {
        Monitor.Enter(obj); // Compliant
        Console.WriteLine(arg.Length);
        Monitor.Exit(obj);
    }

    public void Method2(string arg)
    {
        Monitor.Enter(obj); // Compliant
        Console.WriteLine(arg.Length);
        Monitor.Exit(other);
    }

    public void Method3()
    {
        Monitor.Enter(obj); // Compliant
        var a = new Action(() =>
        {
            Monitor.Exit(obj);
        });
    }

    public void Method4(string arg)
    {
        var localObj = obj;
        Monitor.Enter(localObj); // Compliant
        Console.WriteLine(arg.Length);
        Monitor.Exit(localObj);
    }

    public void Method5(string arg)
    {
        var localObj = obj;
        Monitor.Enter(obj); // Compliant
        Console.WriteLine(arg.Length);
        Monitor.Exit(localObj);
    }

    public void Method6(string arg, object paramObj)
    {
        paramObj = obj;
        Monitor.Enter(paramObj); // Compliant
        Console.WriteLine(arg.Length);
        Monitor.Exit(paramObj);
    }

    public void Method7(string arg)
    {
        Monitor.Enter(obj); // Compliant
        Console.WriteLine(arg.Length);
        var localObj = obj;
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
            Monitor.Enter(obj); // Compliant
        });

        Monitor.Exit(obj);
    }

    public void Method10()
    {
        var getObj = new Func<object>(() =>
        {
            return obj;
        });

        Monitor.Enter(getObj());
        Monitor.Exit(getObj());
    }

    public void Method11()
    {
        var getObj = new Func<object>(() =>
        {
            return obj;
        });

        Monitor.Enter(obj);
        Monitor.Exit(getObj());
    }

    public void Method12()
    {
        Monitor.Enter(obj); // Compliant
        var a = new Action(() =>
        {
            Monitor.Exit(obj);
        });

        a();
    }

    public void Method13(string arg)
    {
        Monitor.Exit(obj);
        Console.WriteLine(arg.Length);
        Monitor.Enter(obj); // Compliant
    }

    public void Method14(string arg)
    {
        Monitor.Exit(obj);
        Console.WriteLine(arg.Length);
        Monitor.Enter(obj); // Compliant
        Console.WriteLine(arg.Length);
        Monitor.Exit(obj);
    }

    public void Method15(Program first, Program second)
    {
        Monitor.Enter(first.obj); // Compliant
        Monitor.Exit(second.obj);
    }


    public void Method16()
    {
        void LocalFunc()
        {
            Monitor.Enter(obj); // Compliant
        }

        Monitor.Exit(obj);
    }

    public void Method17()
    {
        object LocalFunc()
        {
            return obj;
        }

        Monitor.Enter(LocalFunc());
        Monitor.Exit(LocalFunc());
    }

    public void Method18()
    {
        object LocalFunc()
        {
            return obj;
        }

        Monitor.Enter(obj);
        Monitor.Exit(LocalFunc());
    }

    public void Method19()
    {
        Monitor.Enter(obj); // Compliant
        LocalFunc();

        void LocalFunc()
        {
            Monitor.Exit(obj); // Compliant
        }
    }
}
