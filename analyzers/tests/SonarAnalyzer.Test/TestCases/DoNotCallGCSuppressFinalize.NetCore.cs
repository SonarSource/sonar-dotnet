using System;
using System.Threading.Tasks;

namespace Tests.Diagnostics
{
    // https://github.com/SonarSource/sonar-dotnet/issues/3639
    public class Implicit : IAsyncDisposable
    {
        public void Method()
        {
            GC.SuppressFinalize(this); // Noncompliant
        }

        public async ValueTask DisposeAsync()
        {
            await DisposeAsyncCore();

            GC.SuppressFinalize(this); // Compliant, see: https://docs.microsoft.com/en-us/dotnet/standard/garbage-collection/implementing-disposeasync#the-disposeasync-method
        }

        protected virtual ValueTask DisposeAsyncCore() => default;
    }

    public class Explicit : IAsyncDisposable
    {
        public void Method()
        {
            GC.SuppressFinalize(this); // Noncompliant
        }

        async ValueTask IAsyncDisposable.DisposeAsync()
        {
            await DisposeAsyncCore();

            GC.SuppressFinalize(this); // Compliant, see: https://docs.microsoft.com/en-us/dotnet/standard/garbage-collection/implementing-disposeasync#the-disposeasync-method
        }

        protected virtual ValueTask DisposeAsyncCore() => default;
    }

    public class FakeA
    {
        public async ValueTask DisposeAsync()
        {
            await DisposeAsyncCore();

            GC.SuppressFinalize(this); // Noncompliant - ok; it does not implement IAsyncDisposable
        }

        public async ValueTask DisposeAsync(bool argument)
        {
            await DisposeAsyncCore();

            GC.SuppressFinalize(this); // Noncompliant - ok; it does not implement IAsyncDisposable
        }

        protected virtual ValueTask DisposeAsyncCore() => default;
    }

    public class FakeB
    {
        public async void DisposeAsync()
        {
            await DisposeAsyncCore();

            GC.SuppressFinalize(this); // Noncompliant - ok; it does not implement IAsyncDisposable
        }

        protected virtual ValueTask DisposeAsyncCore() => default;
    }

    public class FakeInterface : IFake
    {
        public async ValueTask DisposeAsync()
        {
            await DisposeAsyncCore();

            GC.SuppressFinalize(this); // Noncompliant - ok; it does not implement IAsyncDisposable
        }

        protected virtual ValueTask DisposeAsyncCore() => default;
    }

    public interface IFake
    {
        ValueTask DisposeAsync();
    }
}
