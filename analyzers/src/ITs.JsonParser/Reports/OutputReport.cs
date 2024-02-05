using ITs.JsonParser.Json;

namespace ITs.JsonParser.Reports;

/// <summary>
/// Custom report extracted from the SARIF report.
/// </summary>
public record OutputReport(string Project, string RuleId, string Assembly, string Tfm, RuleIssues RuleIssues)
{
    public string Path(string root)
    {
        // projectName/rule-assembly{-TFM}?.json
        var suffix = Tfm is null ? string.Empty : $"-{Tfm}";
        return System.IO.Path.Combine(root, Project, $"{RuleId}-{Assembly}{suffix}.json");
    }
}
