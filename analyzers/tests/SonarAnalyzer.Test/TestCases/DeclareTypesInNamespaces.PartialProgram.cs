public partial class Program // Noncompliant {{Move 'Program' into a named namespace.}}
{
    private Program() { }
}

public partial class Program // Noncompliant {{Move 'Program' into a named namespace.}}
{
    public void M() { }
}
