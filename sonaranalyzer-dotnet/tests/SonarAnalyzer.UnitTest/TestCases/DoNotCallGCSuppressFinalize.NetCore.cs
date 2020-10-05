using System;
using System.Threading.Tasks;

namespace Tests.Diagnostics
{
    // https://github.com/SonarSource/sonar-dotnet/issues/3639
    public class Custom : IAsyncDisposable
    {
        public void Method()
        {
            GC.SuppressFinalize(this); // Noncompliant
        }

        public async ValueTask DisposeAsync()
        {
            await DisposeAsyncCore();

            GC.SuppressFinalize(this); // Noncompliant - FP, see: https://docs.microsoft.com/en-us/dotnet/standard/garbage-collection/implementing-disposeasync#the-disposeasync-method
        }

        protected virtual ValueTask DisposeAsyncCore() => default;
    }

    public class Fake
    {
        public async ValueTask DisposeAsync()
        {
            await DisposeAsyncCore();

            GC.SuppressFinalize(this); // Noncompliant - ok; it does not implement IAsyncDisposable
        }

        protected virtual ValueTask DisposeAsyncCore() => default;
    }
}
