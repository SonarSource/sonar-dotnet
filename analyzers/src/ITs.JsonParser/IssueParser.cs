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
    private static readonly JsonSerializerOptions SerializerOptions = new() { PropertyNameCaseInsensitive = true, WriteIndented = true };

    public static void Execute(string inputRootPath, string outputRootPath)
    {
        Console.WriteLine($"Searching for SARIF reports in {inputRootPath}...");
        var sarifReports = Read(inputRootPath);
        ConsoleHelper.WriteLineColor("Successfully parsed all SARIF reports!", ConsoleColor.Green);

        Console.WriteLine("Mapping SARIF reports to Rule reports..");
        var customReports = Map(sarifReports);
        ConsoleHelper.WriteLineColor($"Successfully mapped all SARIF reports to {customReports.Length} Rule reports!", ConsoleColor.Green);

        Console.WriteLine($"Writing results to {outputRootPath}");
        Write(outputRootPath, customReports);
        ConsoleHelper.WriteLineColor("Successfully wrote all Rule reports.!", ConsoleColor.Green);
    }

    private static List<InputReport> Read(string rootPath)
    {
        var sarifPaths = Directory.GetFiles(rootPath, "*.json", SearchOption.AllDirectories);
        ConsoleHelper.WriteLineColor($"Found {sarifPaths.Length} SARIF reports in {rootPath}.", ConsoleColor.Green);
        var result = new List<InputReport>();
        foreach (var path in sarifPaths.Where(x => !x.Contains("AnalysisWarnings.MsBuild"))) // Remove non-SARIF JSONs
        {
            // .../project/assembly{-TFM}?.json
            var filename = Path.GetFileNameWithoutExtension(path);
            var splitted = filename.Split('-');

            Console.WriteLine($"Processing {path}...");
            var project = Path.GetFileName(Path.GetDirectoryName(path));
            var assembly = splitted[0];
            var tfm = splitted.ElementAtOrDefault(1) ?? "UNSET"; // some projects have only one TFM, so it is not included in the name.
            var sarif = JsonSerializer.Deserialize<SarifIssues>(File.ReadAllText(path), SerializerOptions);
            ConsoleHelper.WriteLineColor($"Successfully parsed {project}/{assembly} [{tfm}]", ConsoleColor.Green);
            result.Add(new(project, assembly, tfm, sarif));
        }
        return result;
    }

    private static OutputReport[] Map(List<InputReport> inputReports) =>
        inputReports.SelectMany(ExplodeToRuleIssues).ToArray();

    // One SARIF contains all the issues for the project. We need to split it by rule Id.
    private static IEnumerable<OutputReport> ExplodeToRuleIssues(InputReport inputReport)
    {
        var allIssues = inputReport.Sarif
            .AllIssues()
            .Where(x => Regex.IsMatch(x.RuleId, "^S[0-9]+$"))
            .OrderBy(x => x.Order())
            .GroupBy(x => x.RuleId);
        foreach (var issuesByRule in allIssues)
        {
            var issues = new RuleIssues { Issues = issuesByRule.Select(RuleIssue.From).ToArray() };
            yield return new OutputReport(inputReport.Project, issuesByRule.Key, inputReport.Assembly, inputReport.TFM, issues);
        }
    }

    private static void Write(string rootPath, OutputReport[] outputReports)
    {
        foreach (var project in outputReports.Select(x => x.Project).Distinct())
        {
            Directory.CreateDirectory(Path.Combine(rootPath, project));
        }
        foreach (var outputReport in outputReports)
        {
            // projectName/rule-assembly-TFM.json
            var resultPath = Path.Combine(rootPath, outputReport.Project, $"{outputReport.RuleId}-{outputReport.Assembly}-{outputReport.TFM}.json");
            File.WriteAllText(resultPath, JsonSerializer.Serialize(outputReport.RuleIssues, SerializerOptions));
        }
    }
}
