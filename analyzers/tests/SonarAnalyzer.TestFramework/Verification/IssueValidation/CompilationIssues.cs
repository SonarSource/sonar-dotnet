﻿/*
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

using System.Collections;
using System.IO;

namespace SonarAnalyzer.TestFramework.Verification.IssueValidation;

internal sealed class CompilationIssues : IEnumerable<IssueLocation>
{
    private readonly List<IssueLocation> issues;

    public CompilationIssues(IEnumerable<FileContent> files)
        : this(files.SelectMany(x => IssueLocationCollector.ExpectedIssueLocations(x.FileName, x.Content.Lines))) { }

    public CompilationIssues(Diagnostic[] diagnostics)
        : this(ToIssueLocations(diagnostics)) { }

    public CompilationIssues(IEnumerable<IssueLocation> issues) =>
        this.issues = issues.ToList();

    public IssueLocationKey[] UniqueKeys() =>
        issues.Select(x => new IssueLocationKey(x)).Distinct().ToArray();

    public List<IssueLocation> Remove(IssueLocationKey key)
    {
        var ret = issues.Where(key.IsMatch).ToList();
        foreach (var issue in ret)
        {
            issues.Remove(issue);
        }
        return ret;
    }

    public void Dump(string languageVersion)
    {
        foreach (var file in issues.GroupBy(x => Path.GetFileName(x.FilePath)).OrderBy(x => x.Key))
        {
            Console.WriteLine($"Actual {languageVersion} diagnostics {file.Key}:");
            foreach (var issue in file.OrderBy(x => x.LineNumber))
            {
                Console.WriteLine($"    {issue.RuleId}, Line: {issue.LineNumber}, [{issue.Start}, {issue.Length}] {issue.Message}");
            }
        }
    }

    private static IEnumerable<IssueLocation> ToIssueLocations(Diagnostic[] diagnostics)
    {
        var ret = new List<IssueLocation>();
        foreach (var diagnostic in diagnostics)
        {
            var primary = new IssueLocation(diagnostic);
            ret.Add(primary);
            for (var i = 0; i < diagnostic.AdditionalLocations.Count; i++)
            {
                var secondaryMessage = diagnostic.Properties.TryGetValue(i.ToString(), out var message) ? message : null;
                ret.Add(new IssueLocation(primary, new SecondaryLocation(diagnostic.AdditionalLocations[i], secondaryMessage)));
            }
        }
        return ret;
    }

    IEnumerator<IssueLocation> IEnumerable<IssueLocation>.GetEnumerator() =>
        issues.GetEnumerator();

    [ExcludeFromCodeCoverage]
    IEnumerator IEnumerable.GetEnumerator() =>
        issues.GetEnumerator();
}
