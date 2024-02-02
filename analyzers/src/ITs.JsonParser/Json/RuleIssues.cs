namespace ITs.JsonParser.Json;

// Format of JSONs in "actual/" and "expected/"
internal class RuleIssues
{
    public RuleIssue[] Issues { get; set; }
}

internal class RuleIssue
{
    public string Id { get; set; }
    public string Message { get; set; }
    public Location Location { get; set; }

    public RuleIssue(SarifIssue issue)
    {
        Id = issue.RuleId;
        Message = issue.Message;
        Location = new()
        {
            Uri = issue.NormalizedUri(),
            Region = issue.Location?.Region
        };
    }
}
