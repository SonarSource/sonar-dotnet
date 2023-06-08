using System;
using System.IO;
using System.Threading.Tasks;

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

public class DisposableAsync : IDisposable, IAsyncDisposable
{
    public void Dispose() { }
    public async ValueTask DisposeAsync() { }
}

public class DisposeAsync
{
    async Task DisposeAsyncTwiceUsingStatement()
    {
        await using var d = new DisposableAsync(); // Noncompliant
        await d.DisposeAsync();
    }

    async Task DisposeAsyncTwice()
    {
        var d = new DisposableAsync();
        await d.DisposeAsync();
        await d.DisposeAsync(); // Noncompliant
    }

    async Task DisposeTwiceMixed()
    {
        var d = new DisposableAsync();
        await d.DisposeAsync();
        d.Dispose(); // Noncompliant
    }
}

public class DisposableWithExplicitImplementation : IDisposable
{
    void IDisposable.Dispose() { } // Explicit Implementation
}

public class ExplicitDisposeImplementation
{
    void DisposeTwiceExplicit()
    {
        IDisposable d = new DisposableAsync();
        d.Dispose();
        d.Dispose(); // Noncompliant
    }
}

public class ExpressionsTest
{
    public void CoalescingAssignment(Disposable a, Disposable b)
    {
        (a ??= b).Dispose();
        a.Dispose(); // FN
        b.Dispose(); // FN
    }

    public void Ternary(Disposable a, Disposable b, bool condition)
    {
        (condition ? a : b).Dispose();
        a.Dispose(); // FN
        b.Dispose(); // FN
    }
}
