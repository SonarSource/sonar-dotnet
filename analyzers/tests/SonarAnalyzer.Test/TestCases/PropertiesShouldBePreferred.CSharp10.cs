public record struct RecordStruct
{
    public RecordStruct() { }

    private string name = "";

    public string GetName() // Noncompliant {{Consider making method 'GetName' a property.}}
    //            ^^^^^^^
    {
        return name;
    }

    public object GetName2() => null;  // Noncompliant
    private string GetName3() => null; // Compliant
    public string Property { get; set; } = "";
}

public record struct PositionalRecordStruct(string Parameter)
{
    private string name = "";

    public string GetName()            // Noncompliant
    {
        return name;
    }

    public object GetName2() => null;  // Noncompliant
    private string GetName3() => null; // Compliant
    public string Property { get; set; } = "";
}
