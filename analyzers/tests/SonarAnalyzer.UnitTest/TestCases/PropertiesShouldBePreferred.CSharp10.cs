public record struct RecordStruct
{
    private string name = "";

    public string GetName() // FN
    {
        return name;
    }

    public object GetName2() => null; // FN

    private string GetName3() => null; // Compliant

    public string Property { get; set; } = "";
}

public record struct PositionalRecordStruct(string Parameter)
{
    private string name = "";

    public string GetName() // FN
    {
        return name;
    }

    public object GetName2() => null; // FN

    private string GetName3() => null; // Compliant

    public string Property { get; set; } = "";
}
