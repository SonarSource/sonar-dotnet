namespace ITs.JsonParser.Json;

// Format of JSONs in "output/"
internal class SarifIssues
{
    public Run[] Runs { get; set; }

    public SarifIssue[] AllIssues() => Runs.SelectMany(r => r.Results).ToArray();
}

internal class Run
{
    public SarifIssue[] Results { get; set; }
}

internal class SarifIssue
{
    public string RuleId { get; set; }
    public string Level { get; set; }
    public string Message { get; set; }
    public LocationWrapper[] Locations { get; set; }

    public Location Location => Locations is { Length: > 0 } ? Locations[0].ResultFile : null;

    public object Order() =>
        Location is not null
            ? (NormalizedUri(), Location.Region.StartLine, Location.Region.StartColumn, Location.Region.EndLine, Location.Region.EndColumn, Message)
            : default;

    public string NormalizedUri()
    {
        if (Location is not null)
        {
            const string prefix = "https://github.com/SonarSource/sonar-dotnet/blob/master/";
            // analyzers/its/sources/project/...
            var filepath = Location.Uri[Location.Uri.IndexOf("analyzers/its")..];
            // ...#L1-L2
            var suffix = string.Empty;
            if (Location.Region.StartLine > 0)
            {
                suffix = Location.Region.StartLine == Location.Region.EndLine
                    ? $"#L{Location.Region.StartLine}"
                    : $"#L{Location.Region.StartLine}-L{Location.Region.EndLine}";
            }
            return $"{prefix}{filepath}{suffix}";
        }
        return null;
    }
}

internal class LocationWrapper
{
    public Location ResultFile { get; set; }
}
