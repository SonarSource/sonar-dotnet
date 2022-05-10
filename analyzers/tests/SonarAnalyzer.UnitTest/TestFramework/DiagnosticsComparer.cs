/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2022 SonarSource SA
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

using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using SonarAnalyzer.Common;

namespace SonarAnalyzer.UnitTest.TestFramework
{
    public static class DiagnosticsComparer
    {
        public static StringBuilder Compare(Diagnostic[] diagnostics, Dictionary<string, IList<IIssueLocation>> expectedIssuesPerFile, string languageVersion)
        {
            var summary = new StringBuilder($"Comparing actual issues with expected:\nLanguage version: {languageVersion}.\n");
            var actualIssuesPerFile = diagnostics.ToIssueLocationsPerFile();

            foreach (var fileName in actualIssuesPerFile.Keys.OrderBy(x => x))
            {
                summary.Append('\n').Append(fileName).Append('\n');

                var expectedIssues = expectedIssuesPerFile[fileName];
                foreach (var actualIssue in actualIssuesPerFile[fileName].OrderBy(x => x.LineNumber))
                {
                    var expectedIssue = expectedIssues.FirstOrDefault(x => x.IsPrimary == actualIssue.IsPrimary && x.LineNumber == actualIssue.LineNumber);
                    summary.AppendComparison(actualIssue, expectedIssue);
                    expectedIssues.Remove(expectedIssue);
                }

                foreach (var expectedIssue in expectedIssues.OrderBy(x => x.LineNumber))
                {
                    var issueId = expectedIssue.IssueId is { } id ? $" ID: {id}" : string.Empty;
                    summary.Append($"Line {expectedIssue.LineNumber}: Expected {IssueType(expectedIssue.IsPrimary)} issue was not raised!{issueId}\n");
                }
            }

            return summary;
        }

        private static void AppendComparison(this StringBuilder summary, IIssueLocation actual, IIssueLocation expected)
        {
            var prefix = $"Line {actual.LineNumber}";
            var issueType = IssueType(actual.IsPrimary);
            var issueId = (expected?.IssueId ?? actual.IssueId) is { } id ? $" ID: {id}" : string.Empty;

            if (expected == null)
            {
                summary.Append($"{prefix}: Unexpected {issueType} issue '{actual.Message}'!{issueId}\n");
            }
            else if (expected.Message != null && actual.Message != expected.Message)
            {
                summary.Append($"{prefix}: {issueType} issue message '{expected.Message}' does not match the actual message '{actual.Message}'!{issueId}\n");
            }
            else if (expected.Start.HasValue && actual.Start != expected.Start)
            {
                summary.Append($"{prefix}: {issueType} issue should start on column {expected.Start} but got column {actual.Start}!{issueId}\n");
            }
            else if (expected.Length.HasValue && actual.Length != expected.Length)
            {
                summary.Append($"{prefix}: {issueType} issue should have a length of {expected.Length} but got a length of {actual.Length}!{issueId}\n");
            }
        }

        private static Dictionary<string, IList<IIssueLocation>> ToIssueLocationsPerFile(this IEnumerable<Diagnostic> diagnostics)
        {
            var issuesPerFile = new Dictionary<string, IList<IIssueLocation>>();

            foreach (var diagnostic in diagnostics)
            {
                var path = diagnostic.Location.SourceTree?.FilePath ?? string.Empty;
                if (!issuesPerFile.ContainsKey(path))
                {
                    issuesPerFile.Add(path, new List<IIssueLocation>());
                }
                issuesPerFile[path].Add(new IssueLocationCollector.IssueLocation(diagnostic));

                for (var i = 0; i < diagnostic.AdditionalLocations.Count; i++)
                {
                    issuesPerFile[path].Add(new IssueLocationCollector.IssueLocation(diagnostic.GetSecondaryLocation(i)));
                }
            }

            return issuesPerFile;
        }

        private static string IssueType(bool isPrimary) =>
            isPrimary ? "Primary" : "Secondary";
    }
}
