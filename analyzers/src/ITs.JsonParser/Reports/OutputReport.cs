using ITs.JsonParser.Json;

namespace ITs.JsonParser.Reports;

/// <summary>
/// Custom report extracted from the SARIF report.
/// </summary>
internal record OutputReport(string Project, string RuleId, string Assembly, string TFM, RuleIssues RuleIssues);
