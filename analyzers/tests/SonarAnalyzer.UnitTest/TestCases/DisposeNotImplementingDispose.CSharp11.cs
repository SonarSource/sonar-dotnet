using System;
using System.Collections.Generic;
using System.IO;

namespace Tests.Diagnostics
{
    public interface IStaticAbstractMethod
    {
        static abstract void Dispose(); // Noncompliant {{Either implement 'IDisposable.Dispose', or totally rename this method to prevent confusion.}}
    }

    public interface IStaticVirtualMethod
    {
        static virtual void Dispose() { } // Noncompliant {{Either implement 'IDisposable.Dispose', or totally rename this method to prevent confusion.}}
    }
}
