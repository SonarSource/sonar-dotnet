record struct Bar               // Noncompliant {{Move 'Bar' into a named namespace.}}
//            ^^^
{
    record struct InnerBar { }  // Compliant - we want to report only on the outer record
}

public record class Product { } // Noncompliant
