using System;
using System.Threading;
using System.Threading.Tasks;

public class TestClass : IAsyncDisposable
{
    private CancellationTokenSource cancellationTokenSource;

    public Task MethodAsync()
    {
        this.cancellationTokenSource = new CancellationTokenSource();
        return Task.Delay(1000, this.cancellationTokenSource.Token);
    }

    public ValueTask DisposeAsync()
    {
        this.cancellationTokenSource?.Dispose();
        return new ValueTask();
    }
}

public class C1 // Noncompliant, needs to implement IDisposable or IAsyncDisposable
{
    private IAsyncDisposable disposable;

    public void Init() => disposable = new AsyncDisposable();
}

public class C2 : IAsyncDisposable // Implements IAsyncDisposable
{
    private IAsyncDisposable disposable;

    public void Init() => disposable = new AsyncDisposable();

    public ValueTask DisposeAsync() => disposable.DisposeAsync();
}

public class AsyncDisposable : IAsyncDisposable
{
    public ValueTask DisposeAsync() => new ValueTask();
}
