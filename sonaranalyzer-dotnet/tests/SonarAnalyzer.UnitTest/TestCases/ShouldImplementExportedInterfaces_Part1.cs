using System;
using System.ComponentModel.Composition;

namespace Classes
{
    [Export(typeof(IDisposable))]
    partial class Exported
    {
    }

    [Export(typeof(IDisposable))] // Noncompliant
    // Noncompliant@-1
    partial class NotExported
    {
    }
}
