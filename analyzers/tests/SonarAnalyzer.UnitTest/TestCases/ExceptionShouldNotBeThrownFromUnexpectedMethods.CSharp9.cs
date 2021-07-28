using System;
using System.Runtime.CompilerServices;

namespace Tests.Diagnostics
{
    record Record1 : IDisposable
    {
        static Record1() => throw new NotImplementedException();
        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }

    record Record2 : IDisposable
    {
        static Record2() => throw new Exception(); // Noncompliant

        [ModuleInitializer]
        internal static void M1()
        {
            throw new Exception(); // FN
        }

        event EventHandler OnSomething
        {
            add
            {
                throw new Exception(); // Noncompliant
            }
            remove
            {
                throw new InvalidOperationException(); // Compliant
            }
        }

        event EventHandler OnSomeArrow
        {
            add => throw new Exception(); // Noncompliant
            remove => throw new InvalidOperationException(); // Compliant
        }

        public override string ToString()
        {
            throw new Exception(); // Noncompliant
        }

        public void Dispose()
        {
            throw new Exception(); // Noncompliant
        }
    }
}
