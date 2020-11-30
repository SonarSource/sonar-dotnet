public record RecordBase
{
    public virtual string GetFooMethod() => ""; // Compliant - FN
}

public record Record : RecordBase
{
    private string name;

    public string GetName() // Compliant - FN
    {
        return name;
    }

    public override string GetFooMethod() => ""; // Compliant

    public object GetName2() => null; // Compliant - FN

    protected string GetName3() => null; // Compliant - FN

    private string GetName4() => null;

    public string Property { get; set; }
}
