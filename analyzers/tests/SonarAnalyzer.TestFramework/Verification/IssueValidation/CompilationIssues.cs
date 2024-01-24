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

using System.IO;

namespace SonarAnalyzer.TestFramework.Verification.IssueValidation;

internal sealed class CompilationIssues
{
    public string LanguageVersion { get; }
    private readonly FileIssueLocations[] fileIssues;

    public CompilationIssues(string languageVersion, IEnumerable<FileContent> files)
    {
        LanguageVersion = languageVersion;
        fileIssues = files.Select(x => new FileIssueLocations(x.FileName, IssueLocationCollector.GetExpectedIssueLocations(x.Content.Lines))).ToArray();
    }

    public CompilationIssues(string languageVersion, Diagnostic[] diagnostics)
    {
        var map = new Dictionary<string, List<IIssueLocation>>();
        foreach (var diagnostic in diagnostics)
        {
            Add(new IssueLocationCollector.IssueLocation(diagnostic));
            for (var i = 0; i < diagnostic.AdditionalLocations.Count; i++)
            {
                Add(new IssueLocationCollector.IssueLocation(diagnostic.GetSecondaryLocation(i)));
            }
        }
        LanguageVersion = languageVersion;
        fileIssues = map.Select(x => new FileIssueLocations(x.Key, x.Value)).ToArray();

        void Add(IssueLocationCollector.IssueLocation issue)
        {
            var list = map.GetOrAdd(issue.FilePath ?? string.Empty, _ => new List<IIssueLocation>());
            list.Add(issue);
        }
    }

    public void Dump()
    {
        foreach (var file in fileIssues)
        {
            Console.WriteLine($"Actual {LanguageVersion} diagnostics {Path.GetFileName(file.FileName)}:");
            foreach (var issue in file.IssueLocations.OrderBy(x => x.LineNumber))
            {
                Console.WriteLine($"  ID: {issue.IssueId}, Line: {issue.LineNumber}, [{issue.Start}, {issue.Length}] {issue.Message}");
            }
        }
    }
}
