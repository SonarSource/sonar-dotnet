/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2021 SonarSource SA
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

using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using SonarAnalyzer.Common;
using SonarAnalyzer.Helpers;

namespace SonarAnalyzer.UnitTest.TestFramework
{
    public static class TestAnalysisComparer
    {
        public static StringBuilder Compare(Diagnostic[] diagnostics, Dictionary<string, IList<IIssueLocation>> expectedIssuesPerFile, string languageVersion)
        {
            var summary = new StringBuilder($"Comparing actual issues with expected:\nLanguage version: {languageVersion}.\n");
            var actualIssues = diagnostics.ToIssueLocations();

            foreach (var fileName in actualIssues.Keys.OrderBy(x => x))
            {
                summary.Append('\n').Append(fileName).Append('\n');

                foreach (var actualIssue in actualIssues[fileName].OrderBy(x => x.LineNumber))
                {
                    var issue = expectedIssuesPerFile[fileName].FirstOrDefault(x => x.IsPrimary == actualIssue.IsPrimary && x.LineNumber == actualIssue.LineNumber);
                    summary.Append(Compare(actualIssue, issue));
                    expectedIssuesPerFile[fileName].Remove(issue);
                }

                foreach (var expectedIssue in expectedIssuesPerFile[fileName].OrderBy(x => x.LineNumber))
                {
                    summary.Append($"Line {expectedIssue.LineNumber}: expected {IssueType(expectedIssue.IsPrimary)} issue {expectedIssue.IssueId ?? "(no id)"} was not raised!\n");
                }
            }

            return summary;
        }

        private static StringBuilder Compare(IIssueLocation actual, IIssueLocation expected)
        {
            var summary = new StringBuilder();
            var prefix = $"Line {actual.LineNumber}";
            var issueType = IssueType(actual.IsPrimary);
            var issueId = expected?.IssueId ?? actual.IssueId ?? "(no id)";

            if (expected == null)
            {
                summary.Append($"{prefix}: unexpected {issueType} issue {issueId} with message '{actual.Message}'.\n");
            }
            else if (expected.Message != null && actual.Message != expected.Message)
            {
                summary.Append($"{prefix}: {issueType} issue {issueId} message '{expected.Message}' does not match the actual message '{actual.Message}'.\n");
            }
            else if (expected.Start.HasValue && actual.Start != expected.Start)
            {
                summary.Append($"{prefix}: {issueType} issue {issueId} should start on column {expected.Start} but got column {actual.Start}.\n");
            }
            else if (expected.Length.HasValue && actual.Length != expected.Length)
            {
                summary.Append($"{prefix}: {issueType} issue {issueId} should have a length of {expected.Length} but got a length of {actual.Length}.\n");
            }

            return summary;
        }

        private static IIssueLocation ToIssueLocation(this Diagnostic diagnostic) =>
            new IssueLocationCollector.IssueLocation
            {
                IsPrimary = true,
                LineNumber = diagnostic.Location.GetLineNumberToReport(),
                Message = diagnostic.GetMessage(),
                IssueId = diagnostic.Id,
                Start = diagnostic.Location.GetLineSpan().StartLinePosition.Character,
                Length = diagnostic.Location.SourceSpan.Length
            };

        private static IIssueLocation ToIssueLocation(this SecondaryLocation secondaryLocation) =>
            new IssueLocationCollector.IssueLocation
            {
                IsPrimary = false,
                LineNumber = secondaryLocation.Location.GetLineNumberToReport(),
                Message = secondaryLocation.Message,
                IssueId = null,
                Start = secondaryLocation.Location.GetLineSpan().StartLinePosition.Character,
                Length = secondaryLocation.Location.SourceSpan.Length
            };

        private static Dictionary<string, IList<IIssueLocation>> ToIssueLocations(this IEnumerable<Diagnostic> diagnostics)
        {
            var actualIssues = new Dictionary<string, IList<IIssueLocation>>();

            foreach (var diagnostic in diagnostics)
            {
                var path = diagnostic.Location.SourceTree?.FilePath ?? string.Empty;
                if (!actualIssues.ContainsKey(path))
                {
                    actualIssues.Add(path, new List<IIssueLocation>());
                }

                actualIssues[path].Add(diagnostic.ToIssueLocation());

                foreach (var secondaryLocation in diagnostic.AdditionalLocations.Select((_, i) => diagnostic.GetSecondaryLocation(i)))
                {
                    actualIssues[path].Add(secondaryLocation.ToIssueLocation());
                }
            }

            return actualIssues;
        }

        private static string IssueType(bool isPrimary) =>
            isPrimary ? "primary" : "secondary";
    }
}
