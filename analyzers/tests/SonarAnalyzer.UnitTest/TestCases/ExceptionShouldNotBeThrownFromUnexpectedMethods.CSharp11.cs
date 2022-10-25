using System;

namespace Tests.Diagnostics
{
    interface IMyInterface : IDisposable
    {
        static event EventHandler OnSomeArrow
        {
            add => throw new Exception(); // Noncompliant
            remove => throw new InvalidOperationException(); // Compliant - allowed exception
        }
    }

    class MyClass : IMyInterface
    {
        public void Dispose() => throw new Exception(); // Noncompliant
    }
}
