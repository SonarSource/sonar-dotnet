using System.IO;
using System.Text.Json;
using System.Text.RegularExpressions;
using ITs.JsonParser.Json;
using ITs.JsonParser.Reports;

namespace ITs.JsonParser;

/// <summary>
/// Reads SARIF reports from "output/" and writes custom reports to "actual/".
/// </summary>
public static class IssueParser
{
    private static readonly Regex RxSonarRule = new("^S[0-9]+$", RegexOptions.Compiled);
    private static readonly JsonSerializerOptions SerializerOptions = new() { PropertyNameCaseInsensitive = true, WriteIndented = true };

    public static void Execute(string inputRootPath, string outputRootPath)
    {
        Console.WriteLine($"Searching for SARIF reports in {inputRootPath}...");
        var sarifReports = Read(inputRootPath);
        ConsoleHelper.WriteLineColor($"Successfully parsed {sarifReports.Length} SARIF reports.", ConsoleColor.Green);

        Console.WriteLine("Mapping SARIF reports to Rule reports..");
        var customReports = sarifReports.SelectMany(ExplodeToRuleIssues).ToArray();
        ConsoleHelper.WriteLineColor($"Successfully mapped all SARIF reports to {customReports.Length} Rule reports.", ConsoleColor.Green);

        Console.WriteLine($"Writing results to {outputRootPath}");
        WriteOutput(outputRootPath, customReports);
        ConsoleHelper.WriteLineColor("Successfully wrote all Rule reports.", ConsoleColor.Green);
    }

    private static InputReport[] Read(string rootPath) =>
        Directory.GetFiles(rootPath, "*.json", SearchOption.AllDirectories)
            .Where(x => !x.Contains("AnalysisWarnings.MsBuild", StringComparison.InvariantCulture))
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
            yield return new OutputReport(inputReport.Project, issuesByRule.Key, inputReport.Assembly, inputReport.TFM, issues);
        }
    }

    private static void WriteOutput(string root, OutputReport[] reports)
    {
        foreach (var project in reports.Select(x => x.Project).Distinct())
        {
            Directory.CreateDirectory(Path.Combine(root, project));
        }
        foreach (var report in reports)
        {
            // projectName/rule-assembly{-TFM}?.json
            var suffix = report.TFM is null ? string.Empty : $"-{report.TFM}";
            var resultPath = Path.Combine(root, report.Project, $"{report.RuleId}-{report.Assembly}{suffix}.json");
            File.WriteAllText(resultPath, JsonSerializer.Serialize(report.RuleIssues, SerializerOptions));
        }
    }
}
