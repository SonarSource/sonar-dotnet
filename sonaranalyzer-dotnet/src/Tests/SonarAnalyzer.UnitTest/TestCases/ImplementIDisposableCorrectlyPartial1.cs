using System;

namespace Tests.Diagnostics
{
    public partial class ImplementIDisposableCorrectlyPartial : IDisposable // Noncompliant
    {
        protected virtual void Dispose(bool disposing)
        {
        }
    }
}










// Secondary