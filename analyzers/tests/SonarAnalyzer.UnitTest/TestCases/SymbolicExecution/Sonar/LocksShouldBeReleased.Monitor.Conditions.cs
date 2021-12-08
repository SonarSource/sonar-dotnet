using System;
using System.Threading;

class Program
{
    private object obj1 = new object();
    private object obj2 = new object();

    public object PublicObject = new object();

    public void Method1(bool condition)
    {
        Monitor.Enter(obj1); // FN
        if (condition)
        {
            Monitor.Exit(obj1);
        }
    }

    public void Method2(bool condition)
    {
        Monitor.Enter(obj1); // FN
        switch (condition)
        {
            case true:
                Monitor.Exit(obj1);
                break;
            default:
                break;
        }
    }

    public void Method3(bool condition)
    {
        bool isAcquired = false;
        Monitor.Enter(obj1, ref isAcquired); // FN
        if (condition)
        {
            Monitor.Exit(obj1);
        }
    }

    public void Method4(bool condition)
    {
        Monitor.Enter(obj1); // FN
        Monitor.Enter(obj2); // FN
        if (condition)
        {
            Monitor.Exit(obj1);
        }
        else
        {
            Monitor.Exit(obj2);
        }
    }

    public void Method5(bool condition)
    {
        Monitor.Enter(obj1); // Compliant
        if (condition)
        {
            Monitor.Exit(obj2);
        }
    }

    public void Method6(bool condition, string arg)
    {
        var localObj = obj1;
        Monitor.Enter(localObj); // FN
        Console.WriteLine(arg.Length);
        if (condition)
        {
            Monitor.Exit(localObj);
        }
    }

    public void Method7(bool condition, string arg)
    {
        var localObj = obj1;
        Monitor.Enter(obj1); // FN
        Console.WriteLine(arg.Length);
        if (condition)
        {
            Monitor.Exit(localObj);
        }
    }

    public void Method8(bool condition, string arg, object paramObj)
    {
        paramObj = obj1;
        Monitor.Enter(obj1); // FN
        Console.WriteLine(arg.Length);
        if (condition)
        {
            Monitor.Exit(paramObj);
        }
    }

    public void Method9(bool condition, string arg, object paramObj)
    {
        Monitor.Enter(obj1); // FN
        Console.WriteLine(arg.Length);
        if (condition)
        {
            Monitor.Exit(paramObj);
        }
    }

    public void Method10(bool condition, string arg, Program p1)
    {
        Monitor.Enter(p1.PublicObject); // FN
        Console.WriteLine(arg.Length);
        if (condition)
        {
            Monitor.Exit(p1.PublicObject);
        }
    }

    public void Method11(bool condition, string arg, Program p1, Program p2)
    {
        Monitor.Enter(p1.PublicObject); // FN
        Console.WriteLine(arg.Length);
        if (condition)
        {
            Monitor.Exit(p2.PublicObject);
        }
    }

    public void Method12(bool condition)
    {
        var getObj = new Func<object>(() =>
        {
            return obj1;
        });

        Monitor.Enter(getObj()); // FN
        if (condition)
        {
            Monitor.Exit(getObj());
        }
    }

    public void Method13(bool condition)
    {
        Monitor.Enter(obj1); // FN
        var a = new Action(() =>
        {
            Monitor.Exit(obj1);
        });

        if (condition)
        {
            a();
        }
    }

    public void Method14(bool condition)
    {
        Monitor.Enter(obj1); // Compliant
        if (condition)
        {
            Monitor.Exit(obj1);
        }
        else
        {
            Monitor.Exit(obj1);
        }
    }

    public void Method15(string arg)
    {
        Monitor.Enter(obj1); // Compliant
        if (arg.Length == 16)
        {
            Monitor.Exit(obj1);
        }
        else if (arg.Length == 23)
        {
            Monitor.Exit(obj1);
        }
        else
        {
            Monitor.Exit(obj1);
        }
    }

    public void Method16(string arg)
    {
        Monitor.Enter(obj1); // FN
        if (arg.Length == 16)
        {
            Monitor.Exit(obj1);
        }
        else if (arg.Length == 23)
        {
            Monitor.Exit(obj1);
        }
        else
        {
        }
    }

    public void Method17(bool condition1, bool condition2)
    {
        Monitor.Enter(obj1); // FN
        if (condition1)
        {
            if (!condition2)
            {
                Monitor.Exit(obj2);
            }
        }
    }

    public void Method18(bool condition1, bool condition2, bool condition3)
    {
        Monitor.Enter(obj1); // FN
        if (condition1)
        {
            switch (condition2)
            {
                case true:
                    {
                        if (!condition3)
                        {
                            Monitor.Exit(obj1);
                        }
                    }
                    break;
            }
        }
    }

    public int MyProperty
    {
        set
        {
            Monitor.Enter(obj1); // FN
            if (value == 42)
            {
                Monitor.Exit(obj1);
            }
        }
    }
}
