namespace ITs.JsonParser.Json;

internal class Location
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
