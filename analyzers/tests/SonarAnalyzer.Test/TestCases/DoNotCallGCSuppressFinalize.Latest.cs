using System;

ref struct RefStructDisposable : IDisposable
{
    public void Dispose() => GC.SuppressFinalize(new object()); // Compliant
}

ref struct RefStructPatternDisposable
{
    public void Dispose() => GC.SuppressFinalize(new object()); // Compliant
}

ref struct RefStructOtherMethod
{
    public void Other() => GC.SuppressFinalize(new object()); // Noncompliant
}
