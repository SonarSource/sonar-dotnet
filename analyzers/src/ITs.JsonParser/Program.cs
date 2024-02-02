using System.IO;
using CommandLine;
using ITs.JsonParser;

Parser.Default.ParseArguments<CommandLineOptions>(args).MapResult(Execute, errs => 1);

int Execute(CommandLineOptions options)
{
    ConsoleHelper.WriteLineColor("Processing analyzer results", ConsoleColor.Yellow);
    ParseIssues();
    ShowDiff(options.Project, options.RuleId);
    return 0;
}

void ParseIssues()
{
    var sw = Stopwatch.StartNew();
    ConsoleHelper.WriteLineColor("Splitting the SARIF reports to actual folder", ConsoleColor.Yellow);
    var here = Directory.GetCurrentDirectory();
    var inputRoot = Path.Combine(here, "output");
    var outputRoot = Path.Combine(here, "actual");
    IssueParser.Execute(inputRoot, outputRoot);
    ConsoleHelper.WriteLineColor($"Normalized the SARIF reports in '{sw.Elapsed}'", ConsoleColor.Yellow);
}

// TODO: Reimplement this functionality from the powershell function.
void ShowDiff(string project, string ruleId)
{
    // Placeholder, to be deleted
    if (project is not null)
    {
        Console.WriteLine($"Target Project: {project}");
    }
    if (ruleId is not null)
    {
        Console.WriteLine($"Target Rule: {ruleId}");
    }
}
