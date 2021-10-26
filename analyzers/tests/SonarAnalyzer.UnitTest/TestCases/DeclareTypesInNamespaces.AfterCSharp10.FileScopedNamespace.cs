namespace FileScopedNamespace;

class Foo // Noncompliant FP
{
    class InnerFoo { } // Compliant - we want to report only on the outer class

    record struct InnerBar { } // Compliant - we want to report only on the outer record
}

public record class Product { } // Noncompliant FP
