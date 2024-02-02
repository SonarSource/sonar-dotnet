namespace ITs.JsonParser.Json;

internal class Location // TODO: This should be moved to SarifReport file and not re-used by the output JSON format.
{
    public string Uri { get; set; }
    public Region Region { get; set; }
}

public class Region
{
    public int StartLine { get; set; }
    public int StartColumn { get; set; }
    public int EndLine { get; set; }
    public int EndColumn { get; set; }
}
