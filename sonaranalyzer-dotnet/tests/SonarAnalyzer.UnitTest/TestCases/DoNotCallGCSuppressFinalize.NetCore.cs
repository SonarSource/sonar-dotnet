using System;
using System.Threading.Tasks;

namespace Tests.Diagnostics
{
    public class Custom : IAsyncDisposable
    {
        public async ValueTask DisposeAsync()
        {
            await DisposeAsyncCore();

            GC.SuppressFinalize(this); // Noncompliant - FP, see: https://docs.microsoft.com/en-us/dotnet/standard/garbage-collection/implementing-disposeasync#the-disposeasync-method
        }

        protected virtual ValueTask DisposeAsyncCore() => default;
    }
}
