using System;

namespace Tests.Diagnostics
{















    public partial class ImplementIDisposableCorrectlyPartial
    {
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}