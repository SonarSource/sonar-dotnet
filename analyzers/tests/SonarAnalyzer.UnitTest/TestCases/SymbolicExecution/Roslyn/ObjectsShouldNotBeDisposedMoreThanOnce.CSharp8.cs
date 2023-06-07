using System;
using System.IO;

public class Disposable : IDisposable
{
    public void Dispose() { }
}

class UsingDeclaration
{
    public void Disposed_UsingDeclaration()
    {
        using var d = new Disposable(); // Noncompliant {{Resource 'd = new Disposable()' is ensured to be disposed by this using statement. You don't need to dispose it twice.}}
        d.Dispose();
    }

    public void Disposed_UsingStatement()
    {
        using (var d = new Disposable()) // Noncompliant {{Resource 'd = new Disposable()' is ensured to be disposed by this using statement. You don't need to dispose it twice.}}
        {
            d.Dispose();
        }
    }
}

public class NullCoalescenceAssignment
{
    public void NullCoalescenceAssignment_Compliant(IDisposable s)
    {
        s ??= new Disposable();
        s.Dispose();
    }

    public void NullCoalescenceAssignment_NonCompliant(IDisposable s)
    {
        using (s ??= new Disposable()) // FN - FIXME add issue link
        {
            s.Dispose();
        }
    }
}

public interface IWithDefaultMembers
{
    void DoDispose()
    {
        var d = new Disposable();
        d.Dispose();
        d.Dispose(); // Noncompliant
    }
}

public class LocalStaticFunctions
{
    public void Method(object arg)
    {
        void LocalFunction()
        {
            var d = new Disposable();
            d.Dispose();
            d.Dispose(); // FN: local functions are not supported
        }

        static void LocalStaticFunction()
        {
            var d = new Disposable();
            d.Dispose();
            d.Dispose(); // FN: local functions are not supported
        }
    }
}

public ref struct Struct
{
    public void Dispose()
    {
    }
}

public class Consumer
{
    public void M1()
    {
        var s = new Struct();

        s.Dispose();
        s.Dispose(); // Noncompliant
    }

    public void M2()
    {
        using var s = new Struct(); // Noncompliant {{Resource 's = new Struct()' is ensured to be disposed by this using statement. You don't need to dispose it twice.}}
        s.Dispose();
    }

    public void DonotDisposeOnAllPaths(bool flag)
    {
        using var s = new Struct(); // Noncompliant
        if (flag)
        {
            s.Dispose();
        }
    }
}
