using System;
using System.Collections.Generic;

public class Foo2 // Noncompliant
{
    class InnerFoo // Compliant - we want to report only on the outer class
    {
    }
}

public struct Bar2 // Noncompliant
{
    struct InnerBar // Compliant - we want to report only on the outer struct
    {
    }

    enum InnerEnu // Compliant - we want to report only on outer enum
    {
    }
}

public enum Enu2 // Noncompliant
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
