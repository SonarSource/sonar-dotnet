public partial class Partial
{
    public partial void Single(string[] messages, string delimiter = "; ") { }      // Noncompliant

    public partial void Together(string[] messages) { }
    public partial void Together(string[] messages, string delimiter = "; ") { }    // Noncompliant

    public partial void Mixed(string[] messages) { }
    public partial void Mixed(string[] messages, string delimiter = "; ");          // Noncompliant
}

public partial class PartialConstructor
{
    public partial PartialConstructor(string[] messages);
    public partial PartialConstructor(string[] messages, string delimiter = "\n") { } // Noncompliant
}

