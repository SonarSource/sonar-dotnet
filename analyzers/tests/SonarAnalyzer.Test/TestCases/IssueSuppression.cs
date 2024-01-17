using System;

[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("", "")] // Noncompliant
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("")] // Compliant // Error [CS7036] - ctor doesn't exist

namespace Tests.TestCases
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("", "")] // Noncompliant
//                                   ^^^^^^^^^^^^^^^
    class IssueSuppression
    {
  #pragma warning disable XXX // Noncompliant
//^^^^^^^^^^^^^^^^^^^^^^^


#pragma warning restore XXX // Compliant
    }
}
