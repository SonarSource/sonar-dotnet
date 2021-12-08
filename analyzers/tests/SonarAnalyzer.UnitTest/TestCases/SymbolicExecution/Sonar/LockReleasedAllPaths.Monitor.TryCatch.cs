using System;
using System.Threading;

class Program
{
    private object obj = new object();

    public void Method1(string arg)
    {
        Monitor.Enter(obj); // FN
        try
        {
            Console.WriteLine(arg.Length);
        }
        catch (Exception ex)
        {
            Monitor.Exit(obj);
            throw;
        }
    }

    public void Method2(string arg)
    {
        Monitor.Enter(obj); // Compliant
        try
        {
            Console.WriteLine(arg.Length);
        }
        catch (Exception ex)
        {
            throw;
        }
        finally
        {
            Monitor.Exit(obj);
        }
    }

    public void Method3(bool condition)
    {
        Monitor.Enter(obj); // FN

        if (condition)
        {
            throw new Exception();
        }

        Monitor.Exit(obj);
    }

    public void Method4(string arg)
    {
        Monitor.Enter(obj); // FN
        try
        {
            Console.WriteLine(arg.Length);
        }
        catch (Exception)
        {
            Monitor.Exit(obj);
            throw;
        }
    }

    public void Method5(string arg)
    {
        Monitor.Enter(obj); // Compliant
        try
        {
            Console.WriteLine(arg.Length);
        }
        catch (NullReferenceException nre)
        {
            Monitor.Exit(obj);
            throw;
        }
        Monitor.Exit(obj);
    }

    public void Method6(string arg)
    {
        Monitor.Enter(obj); // FN
        try
        {
            Console.WriteLine(arg.Length);
        }
        catch (NullReferenceException nre) when (nre.Message.Contains("Dummy string"))
        {
            Monitor.Exit(obj);
            throw;
        }
    }

    public void Method7(string arg)
    {
        Monitor.Enter(obj); // FN
        try
        {
            Console.WriteLine(arg.Length);
        }
        catch (Exception ex) when (ex is NullReferenceException)
        {
            Monitor.Exit(obj);
            throw;
        }
    }

    public void Method8(string arg)
    {
        Monitor.Enter(obj); // Compliant
        try
        {
            Console.WriteLine(arg.Length);
            Monitor.Exit(obj);
        }
        catch (Exception)
        {
            Monitor.Exit(obj);
        }
    }

    public void Method9(string arg)
    {
        Monitor.Enter(obj); // Compliant
        try
        {
            Console.WriteLine(arg.Length);
            Monitor.Exit(obj);
        }
        catch
        {
            Monitor.Exit(obj);
        }
    }

    public void Method10(string arg)
    {
        Monitor.Enter(obj); // Compliant
        try
        {
            Console.WriteLine(arg.Length);
            Monitor.Exit(obj);
        }
        catch(NullReferenceException nre)
        {
            Monitor.Exit(obj);
        }
        catch (Exception)
        {
            Monitor.Exit(obj);
        }
    }

    public void Method11(string arg)
    {
        Monitor.Enter(obj); // FN
        try
        {
            Console.WriteLine(arg.Length);
            Monitor.Exit(obj);
        }
        catch (NullReferenceException nre)
        {
        }
        catch (Exception)
        {
            Monitor.Exit(obj);
        }
    }

    public void Method12(string arg)
    {
        Monitor.Enter(obj); // FN

        try
        {
            throw new InvalidOperationException();
        }
        catch (InvalidOperationException)
        {
            Monitor.Exit(obj);
        }
    }

    public void Method13()
    {
        Monitor.Enter(obj); // Compliant

        goto Release;

    Release:
        Monitor.Exit(obj);
    }

    public void Method14()
    {
        Monitor.Enter(obj); // FN

        goto DoNotRelease;

    Release:
        Monitor.Exit(obj);

    DoNotRelease:
        return;
    }
}
