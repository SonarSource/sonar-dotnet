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

        int NestedInExpression(int a, int b)
        {
            if (a > b)
            {
                return a +
#pragma warning disable XXX // Noncompliant
                    b;
#pragma warning restore XXX
            }
            return 0;
        }

#pragma warning disable XXX // Noncompliant
#pragma warning disable YYY // Noncompliant
#pragma warning disable // Noncompliant
        void ConsecutiveDirectives() { }
#pragma warning restore

#if false
#pragma warning disable XXX // Noncompliant, directives in inactive regions are still parsed and reported
#endif

#pragma checksum "file.cs" "{406EA660-64CF-4C82-B6F0-42D48172A799}" "ab007f1d23d9" // Compliant, not a pragma warning
    }
}
#pragma warning disable XXX // Noncompliant
