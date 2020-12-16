using System;
using System.Threading.Tasks;

// https://github.com/SonarSource/sonar-dotnet/issues/3639
record R1 : IAsyncDisposable
{
    public void Method() => GC.SuppressFinalize(this); // Noncompliant

    public async ValueTask DisposeAsync()
    {
        await DisposeAsyncCore();
        GC.SuppressFinalize(this); // Compliant, see: https://docs.microsoft.com/en-us/dotnet/standard/garbage-collection/implementing-disposeasync#the-disposeasync-method
    }

    protected virtual ValueTask DisposeAsyncCore() => default;
}

record R2 : IDisposable
{
    public string Prop
    {
        init => GC.SuppressFinalize(this); // Noncompliant
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this); // Compliant
    }

    public async ValueTask DisposeAsync()
    {
        await DisposeAsyncCore();
        GC.SuppressFinalize(this); // Noncompliant - ok; it does not implement IAsyncDisposable
    }

    protected virtual ValueTask DisposeAsyncCore() => default;
}
