using System;
using System.ComponentModel.Composition;

namespace Classes
{
    partial class Exported : IDisposable
    {
        public void Dispose() { }
    }

    partial class NotExported
    {
    }
}
