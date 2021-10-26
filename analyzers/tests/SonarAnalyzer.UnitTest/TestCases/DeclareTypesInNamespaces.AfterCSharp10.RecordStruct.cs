record struct Bar // FN
{
    record struct InnerBar { } // Compliant - we want to report only on the outer record
}

public record class Product { } // Noncompliant
