using System;

namespace Tests.Diagnostics
{
    class Program
    {
        public void DisposedTwise1()
        {
            var d = new Disposable();
            d.Dispose();
            d.Dispose(); // Noncompliant
        }

        public void DisposedTwise2()
        {
            IDisposable d = null;
            d = new Disposable();
            if (d != null)
            {
                d.Dispose();
            }
            d.Dispose(); // Noncompliant
        }

        public void DisposedTwise3()
        {
            IDisposable d = null;
            try
            {
                d = new Disposable();
                var x = d;
                x.Dispose();
            }
            finally
            {
                d.Dispose(); // Noncompliant
            }
        }

        public void DisposedTwise4()
        {
            var a = new[] { new Disposable() };
            a[0].Dispose();
            a[0].Dispose(); // Compliant, we don't handle arrays
        }

    }

    public class Disposable : IDisposable
    {
        public void Dispose() { }
    }
}
