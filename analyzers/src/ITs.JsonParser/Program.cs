/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2014-2024 SonarSource SA
 * mailto:info AT sonarsource DOT com
 * This program is free software; you can redistribute it and/or
 * modify it under the terms of the Sonar Source-Available License Version 1, as published by SonarSource SA.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
 * See the Sonar Source-Available License for more details.
 *
 * You should have received a copy of the Sonar Source-Available License
 * along with this program; if not, see https://sonarsource.com/license/ssal/
 */

using System.Diagnostics.CodeAnalysis;
using CommandLine;

namespace ITs.JsonParser;

[ExcludeFromCodeCoverage]
public static class Program
{
    public static void Main(string[] args) =>
        Parser.Default.ParseArguments<CommandLineOptions>(args).MapResult(Execute, errs => 1);

    public static int Execute(CommandLineOptions options)
    {
        ConsoleHelper.WriteLineColor("Processing analyzer results", ConsoleColor.Yellow);
        ParseIssues();
        ShowDiff(options.Project, options.RuleId);
        // ToDo: Update expected
        return 0;
    }

    public static void ParseIssues()
    {
        var sw = Stopwatch.StartNew();
        ConsoleHelper.WriteLineColor("Splitting the SARIF reports to actual folder", ConsoleColor.Yellow);
        var here = Directory.GetCurrentDirectory();
        var inputRoot = Path.Combine(here, "output");
        var outputRoot = Path.Combine(here, "actual");
        IssueParser.Execute(inputRoot, outputRoot);
        ConsoleHelper.WriteLineColor($"Finished splitting the SARIF reports in '{sw.Elapsed}'", ConsoleColor.Yellow);
    }

    // ToDo: Reimplement this functionality from the powershell function.
    public static void ShowDiff(string project, string ruleId)
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
}
