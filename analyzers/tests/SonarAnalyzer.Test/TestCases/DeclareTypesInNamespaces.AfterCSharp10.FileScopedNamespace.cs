namespace FileScopedNamespace;

class Foo // Compliant - File scoped namespace
{
    class InnerFoo { } // Compliant - we want to report only on the outer class

    record struct InnerBar { } // Compliant - we want to report only on the outer record
}

public record class Product { } //Compliant - File scoped namespace
