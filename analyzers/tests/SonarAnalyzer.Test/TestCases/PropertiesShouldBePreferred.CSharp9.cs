public record RecordBase
{
    public virtual string GetFooMethod() => ""; // Noncompliant
}

public record Record : RecordBase
{
    private string name;

    public string GetName() // Noncompliant
    {
        return name;
    }

    public override string GetFooMethod() => ""; // Compliant

    public object GetName2() => null; // Noncompliant

    protected string GetName3() => null; // Noncompliant

    protected internal string GetName4() => null; // Noncompliant

    private string GetName5() => null; // Compliant

    public string Property { get; set; }
}

public record PositionalRecord(string Parameter) : RecordBase
{
    private string name;

    public string GetName() // Noncompliant
    {
        return name;
    }

    public override string GetFooMethod() => ""; // Compliant

    public object GetName2() => null; // Noncompliant

    protected string GetName3() => null; // Noncompliant

    protected internal string GetName4() => null; // Noncompliant

    private string GetName5() => null; // Compliant

    public string Property { get; set; }
}
