using System;
using System.Collections.Generic;
using System.IO;

namespace Tests.Diagnostics
{

    public record struct Struct
    {
        public void Dispose() // Noncompliant {{Either implement 'IDisposable.Dispose', or totally rename this method to prevent confusion.}}
        {
        }
    }

    public struct DisposableStruct : IDisposable
    {
        public DisposableStruct() { }
        public void Dispose() { }
    }
}
