using System;
using System.Collections.Generic;

public class Foo // Noncompliant {{Move the type 'Foo' into a named namespace.}}
{
    class InnerFoo // Compliant - we want to report only on the outer class
    {
    }
}

public struct Bar // Noncompliant {{Move the type 'Bar' into a named namespace.}}
{
    struct InnerBar // Compliant - we want to report only on the outer struct
    {
    }
}

namespace Tests.Diagnostics
{
    class Program { }

    struct MyStruct { }
}
