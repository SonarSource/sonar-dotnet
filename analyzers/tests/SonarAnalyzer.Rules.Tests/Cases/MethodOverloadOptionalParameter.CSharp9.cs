public record Record
{
    void Method(string[] messages) { }
    void Method(string[] messages, string delimiter = "\n") { }                 // Noncompliant
}

public partial class Partial
{
    public void Single(string[] messages) { }
    public partial void Single(string[] messages, string delimiter = "; ");     // Noncompliant

    public partial void Together(string[] messages);
    public partial void Together(string[] messages, string delimiter = "; ");   // Noncompliant

    public partial void Mixed(string[] messages);
    public partial void Mixed(string[] messages, string delimiter = "; ") { }   // Noncompliant
}

public partial class Partial
{
    public partial void Single(string[] messages, string delimiter = "; ") { }      // Noncompliant

    public partial void Together(string[] messages) { }
    public partial void Together(string[] messages, string delimiter = "; ") { }    // Noncompliant

    public partial void Mixed(string[] messages) { }
    public partial void Mixed(string[] messages, string delimiter = "; ");          // Noncompliant
}
