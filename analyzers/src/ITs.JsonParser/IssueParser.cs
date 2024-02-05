using System.Text.RegularExpressions;
using ITs.JsonParser.Json;
using ITs.JsonParser.Reports;

namespace ITs.JsonParser;

/// <summary>
/// Reads SARIF reports from "output/" and writes custom reports to "actual/".
/// </summary>
public static class IssueParser
{
    private static readonly Regex RxSonarRule = new("^S[0-9]+$", RegexOptions.None, TimeSpan.FromMilliseconds(100));
    private static readonly JsonSerializerOptions SerializerOptions = new() { PropertyNameCaseInsensitive = true, WriteIndented = true };

    public static void Execute(string inputPath, string outputPath)
    {
        Console.WriteLine($"Searching for SARIF reports in {inputPath}...");
        var sarifReports = Read(inputPath);
        ConsoleHelper.WriteLineColor($"Successfully parsed {sarifReports.Length} SARIF reports.", ConsoleColor.Green);

        Console.WriteLine("Converting SARIF reports to Rule reports..");
        var customReports = sarifReports.SelectMany(ExplodeToRuleIssues).ToArray();
        ConsoleHelper.WriteLineColor($"Successfully converted all SARIF reports to {customReports.Length} Rule reports.", ConsoleColor.Green);

        Console.WriteLine($"Writing results to {outputPath}");
        WriteOutput(outputPath, customReports);
        ConsoleHelper.WriteLineColor("Successfully wrote all Rule reports.", ConsoleColor.Green);
    }

    private static InputReport[] Read(string rootPath) =>
        Directory.GetFiles(rootPath, "*.json", SearchOption.AllDirectories)
            .Select(x => new InputReport(x, SerializerOptions))
            .ToArray();

    // One SARIF contains all the issues for the project. We need to split it by rule Id.
    private static IEnumerable<OutputReport> ExplodeToRuleIssues(InputReport inputReport)
    {
        var allIssues = inputReport.Sarif
            .AllIssues()
            .Where(x => RxSonarRule.IsMatch(x.RuleId))
            .OrderBy(x => x.Order())
            .GroupBy(x => x.RuleId);
        foreach (var issuesByRule in allIssues)
        {
            var issues = new RuleIssues { Issues = issuesByRule.Select(x => new RuleIssue(x)).ToArray() };
            yield return new OutputReport(inputReport.Project, issuesByRule.Key, inputReport.Assembly, inputReport.Tfm, issues);
        }
    }

    private static void WriteOutput(string root, OutputReport[] reports)
    {
        foreach (var projectReports in reports.GroupBy(x => x.Project))
        {
            Directory.CreateDirectory(Path.Combine(root, projectReports.Key));
            foreach (var report in projectReports)
            {
                File.WriteAllText(report.Path(root), JsonSerializer.Serialize(report.RuleIssues, SerializerOptions));
            }
        }
    }
}
