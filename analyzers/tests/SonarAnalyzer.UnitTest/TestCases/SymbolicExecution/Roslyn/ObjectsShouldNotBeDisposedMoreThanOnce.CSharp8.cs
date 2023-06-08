﻿using System;
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
        using var d = new Disposable(); // Noncompliant {{Resource 'd = new Disposable()' has already been disposed explicitly or implicitly through a using statement. Please remove the redundant disposal.}}
        d.Dispose();
    }

    public void Disposed_UsingStatement()
    {
        using (var d = new Disposable()) // Noncompliant {{Resource 'd = new Disposable()' has already been disposed explicitly or implicitly through a using statement. Please remove the redundant disposal.}}
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
        using (s ??= new Disposable()) // FN
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
    public void Dispose() { }
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
        using var s = new Struct(); // Noncompliant {{Resource 's = new Struct()' has already been disposed explicitly or implicitly through a using statement. Please remove the redundant disposal.}}
        s.Dispose();
    }

    public void DoesNotDisposeTwiceOnAllPaths(bool condition)
    {
        using var s = new Struct(); // Noncompliant
        if (condition)
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
    void IDisposable.Dispose() { }
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
    public void CoalescingAssignment(Disposable a, Disposable b, Disposable x, Disposable y)
    {
        a.Dispose();
        b.Dispose();
        (a ??= b).Dispose(); // Noncompliant

        (x ??= y).Dispose();
        x.Dispose(); // FN
        y.Dispose(); // FN
    }

    public void Ternary(Disposable a, Disposable b, Disposable x, Disposable y, bool condition)
    {
        a.Dispose();
        b.Dispose();
        (condition ? a : b).Dispose(); // Noncompliant

        (condition ? x : y).Dispose();
        x.Dispose(); // FN
        y.Dispose(); // FN
    }
}
