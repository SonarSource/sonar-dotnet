namespace ITs.JsonParser.Json;

// Format of JSONs in "output/"
public class SarifReport
{
    public SarifRun[] Runs { get; set; }

    public SarifIssue[] AllIssues() => Runs.SelectMany(x => x.Results).ToArray();
}

public class SarifRun
{
    public SarifIssue[] Results { get; set; }
}

public class SarifIssue
{
    public string RuleId { get; set; }
    public string Message { get; set; }
    public SarifLocation[] Locations { get; set; }

    public SarifLocationFile Location => Locations is { Length: > 0 } ? Locations[0].ResultFile : null;

    public object Order() =>
        Location is null
            ? null
            : (NormalizedUri(), Location.Region.StartLine, Location.Region.StartColumn, Location.Region.EndLine, Location.Region.EndColumn, Message);

    public string NormalizedUri()
    {
        if (Location is null)
        {
            return null;
        }
        else
        {
            // analyzers/its/sources/project/...
            var filePath = Location.Uri.Substring(Location.Uri.IndexOf("analyzers/its"));
            // ...#L1-L2
            var suffix = Location.Region.StartLine == Location.Region.EndLine
                ? $"#L{Location.Region.StartLine}"
                : $"#L{Location.Region.StartLine}-L{Location.Region.EndLine}";
            return $"https://github.com/SonarSource/sonar-dotnet/blob/master/{filePath}{suffix}";
        }
    }
}

public class SarifLocation
{
    public SarifLocationFile ResultFile { get; set; }
}

public class SarifLocationFile
{
    public string Uri { get; set; }
    public SarifRegion Region { get; set; }
}

public class SarifRegion
{
    public int StartLine { get; set; }
    public int StartColumn { get; set; }
    public int EndLine { get; set; }
    public int EndColumn { get; set; }
}
