namespace ITs.JsonParser.Json;

// Format of JSONs in "output/"
internal class SarifReport
{
    public SarifRun[] Runs { get; set; }

    public SarifIssue[] AllIssues() => Runs.SelectMany(x => x.Results).ToArray();
}

internal class SarifRun
{
    public SarifIssue[] Results { get; set; }
}

internal class SarifIssue
{
    public string RuleId { get; set; }
    public string Message { get; set; }
    public SarifLocation[] Locations { get; set; }

    public Location Location => Locations is { Length: > 0 } ? Locations[0].ResultFile : null;

    public object Order() =>
        Location is null
            ? default
            : (NormalizedUri(), Location.Region.StartLine, Location.Region.StartColumn, Location.Region.EndLine, Location.Region.EndColumn, Message);

    public string NormalizedUri()
    {
        if (Location is null)
        {
            return null;
        }
        else
        {
            const string prefix = "https://github.com/SonarSource/sonar-dotnet/blob/master/";
            // analyzers/its/sources/project/...
            var filePath = Location.Uri.Substring(Location.Uri.IndexOf("analyzers/its"));
            // ...#L1-L2
            var suffix = string.Empty;
            if (Location.Region.StartLine > 0)
            {
                suffix = Location.Region.StartLine == Location.Region.EndLine
                    ? $"#L{Location.Region.StartLine}"
                    : $"#L{Location.Region.StartLine}-L{Location.Region.EndLine}";
            }
            return $"{prefix}{filePath}{suffix}";
        }
    }
}

internal class SarifLocation
{
    public Location ResultFile { get; set; }
}
