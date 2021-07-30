using System;
using System.Collections.Generic;

public class Foo2 // Noncompliant {{Move 'Foo2' into a named namespace.}}
{
    class InnerFoo // Compliant - we want to report only on the outer class
    {
    }
}

public struct Bar2 // Noncompliant {{Move 'Bar2' into a named namespace.}}
{
    struct InnerBar // Compliant - we want to report only on the outer struct
    {
    }

    enum InnerEnu // Compliant - we want to report only on outer enum
    {
    }
}

public enum Enu2 // Noncompliant {{Move 'Enu2' into a named namespace.}}
{
    Test
}

namespace Tests.Diagnostics2
{
    class Program { }

    struct MyStruct { }

    public interface MyInt { }

    public enum Enu
    {
        Test
    }
}
