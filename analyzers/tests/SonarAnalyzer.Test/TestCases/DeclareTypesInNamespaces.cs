using System;
using System.Collections.Generic;

public class Foo // Noncompliant {{Move 'Foo' into a named namespace.}}
{
    class InnerFoo // Compliant - we want to report only on the outer class
    {
    }
}

public struct Bar // Noncompliant {{Move 'Bar' into a named namespace.}}
{
    struct InnerBar // Compliant - we want to report only on the outer struct
    {
    }

    enum InnerEnu // Compliant - we want to report only on outer enum
    {
    }
}

public enum Enu // Noncompliant {{Move 'Enu' into a named namespace.}}
{
    Test
}

namespace Tests.Diagnostics
{
    class Program { }

    struct MyStruct { }

    public interface MyInt { }

    public enum Enu
    {
        Test
    }
}
