namespace ITs.JsonParser.Json;

// Format of JSONs in "actual/" and "expected/"
public class RuleIssues
{
    public RuleIssue[] Issues { get; set; }
}

public class RuleIssue
{
    public string Id { get; set; }
    public string Message { get; set; }
    public SarifLocationFile Location { get; set; } // TODO: This should not re-use SarifLocationFile type.

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
