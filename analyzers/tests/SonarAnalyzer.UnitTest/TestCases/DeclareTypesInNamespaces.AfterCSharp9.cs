var x = 1;

class Foo // Noncompliant {{Move 'Foo' into a named namespace.}}
{
    class InnerFoo { } // Compliant - we want to report only on the outer class
}

record Bar // Noncompliant
{
    record InnerBar { } // Compliant - we want to report only on the outer record
}

record PositionalRecord(string FirstParam, string SecondParam); // Noncompliant

namespace Tests.Diagnostics
{
    class Program { }

    record Record { }

    record PositionalRecordInNamespace(string FirstParam, string SecondParam);

    struct MyStruct { }

    public interface MyInt { }

    public enum Enu
    {
        Test
    }
}
