public record struct Record
{
    void Method(string[] messages) { }
    void Method(string[] messages, string delimiter = "\n") { } // Noncompliant
}


public record struct PositionalRecord(string value)
{
    void Method(string[] messages) { }
    void Method(string[] messages, string delimiter = "\n") { } // Noncompliant
}
