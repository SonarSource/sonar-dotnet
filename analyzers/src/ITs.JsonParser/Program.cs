using CommandLine;
using ITs.JsonParser;

Parser.Default.ParseArguments<CommandLineOptions>(args).MapResult(Execute, errs => 1);

int Execute(CommandLineOptions options)
{
    WriteLineColor("Processing analyzer results", ConsoleColor.Yellow);
    NewIssueReports();
    ShowDiffResults(options.Project, options.RuleId);
    return 0;
}

// TODO: Reimplement this functionality from the powershell function.
void NewIssueReports()
{
    Console.WriteLine("Normalizing the SARIF reports");
}

// TODO: Reimplement this functionality from the powershell function.
void ShowDiffResults(string project, string ruleId)
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

void WriteLineColor(string value, ConsoleColor color)
{
    var before = Console.ForegroundColor;
    Console.ForegroundColor = color;
    Console.WriteLine(value);
    Console.ForegroundColor = before;
}
