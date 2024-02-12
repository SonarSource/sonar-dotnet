/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2024 SonarSource SA
 * mailto: contact AT sonarsource DOT com
 *
 * This program is free software; you can redistribute it and/or
 * modify it under the terms of the GNU Lesser General Public
 * License as published by the Free Software Foundation; either
 * version 3 of the License, or (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
 * Lesser General Public License for more details.
 *
 * You should have received a copy of the GNU Lesser General Public License
 * along with this program; if not, write to the Free Software Foundation,
 * Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02110-1301, USA.
 */

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

    private static InputReport[] Read(string rootPath)
    {
        var sarifs = new List<InputReport>();
        foreach (var projectPath in Directory.GetDirectories(rootPath))
        {
            sarifs.AddRange(Directory.GetFiles(Path.Combine(projectPath, "issues"))
                .Select(x => new InputReport(x, Path.GetFileName(projectPath), SerializerOptions)));
        }
        return sarifs.ToArray();
    }

    // One SARIF contains all the issues for the project. We need to split it by rule Id.
    private static IEnumerable<OutputReport> ExplodeToRuleIssues(InputReport inputReport)
    {
        var allIssues = inputReport.Sarif
            .AllIssues()
            .Where(x => RxSonarRule.IsMatch(x.RuleId))
            .GroupBy(x => x.RuleId);
        foreach (var issuesByRule in allIssues)
        {
            var issues = new RuleIssues
            {
                Issues = issuesByRule.Select(x => new RuleIssue(x))
                    .OrderBy(x => x.Uri, StringComparer.InvariantCulture)
                    .ThenBy(x => x.Location, StringComparer.InvariantCulture)
                    .ThenBy(x => x.Message, StringComparer.InvariantCulture)
                    .ToArray()
            };
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
                File.WriteAllText(report.Path(root), JsonSerializer.Serialize(report.RuleIssues, SerializerOptions).Replace("\r\n", "\n"));
            }
        }
    }
}
