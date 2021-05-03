var x = 1;

class Foo // Noncompliant {{Move 'Foo' into a named namespace.}}
{
    class InnerFoo { } // Compliant - we want to report only on the outer class
}

record Bar // Noncompliant
{
    record InnerBar { } // Compliant - we want to report only on the outer record
}

namespace Tests.Diagnostics
{
    class Program { }

    record Record { }

    struct MyStruct { }

    public interface MyInt { }

    public enum Enu
    {
        Test
    }
}
