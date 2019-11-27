/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2019 SonarSource SA
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

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using FluentAssertions.Execution;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using SonarAnalyzer.Helpers;

namespace SonarAnalyzer.UnitTest.TestFramework
{
    public class IssueLocationCollector
    {
        private const string EXACT_COLUMN_PATTERN = @"\s*(\^(?<columnStart>[0-9]+)#(?<length>[0-9]+))?";
        private const string OFFSET_PATTERN = @"(?:@(?<sign>[\+|-]?)(?<offset>[0-9]+))?";
        private const string ISSUE_IDS_PATTERN = @"\s*(\[(?<issueIds>.*)\])*";
        private const string MESSAGE_PATTERN = @"\s*(\{\{(?<message>.*)\}\})*";
        private const string TYPE_PATTERN = @"(?<issueType>Noncompliant|Secondary)";
        private const string BUILD_ERROR_PATTERN = @"Error";
        private const string PRECISE_LOCATION_PATTERN = @"\s*(?<position>\^+)(\s+(?<position>\^+))*\s*";
        private const string NO_PRECISE_LOCATION_PATTERN = @"\s*(?<!\^+\s{1})";
        private const string COMMENT_PATTERN = @"(?<comment>\/\/|\')";

        internal const string ISSUE_LOCATION_PATTERN =
            COMMENT_PATTERN + "+" + NO_PRECISE_LOCATION_PATTERN + TYPE_PATTERN + OFFSET_PATTERN + EXACT_COLUMN_PATTERN + ISSUE_IDS_PATTERN + MESSAGE_PATTERN;
        private const string PRECISE_ISSUE_LOCATION_PATTERN =
            COMMENT_PATTERN + PRECISE_LOCATION_PATTERN + TYPE_PATTERN + "?" + OFFSET_PATTERN + ISSUE_IDS_PATTERN + MESSAGE_PATTERN + "$";
        internal const string BUILD_ERROR_LOCATION_PATTERN =
            COMMENT_PATTERN + NO_PRECISE_LOCATION_PATTERN + BUILD_ERROR_PATTERN + OFFSET_PATTERN + EXACT_COLUMN_PATTERN + ISSUE_IDS_PATTERN;

        public IList<IIssueLocation> GetExpectedIssueLocations(IEnumerable<TextLine> lines)
        {
            var preciseLocations = lines
                .SelectMany(GetPreciseIssueLocations)
                .WhereNotNull()
                .ToList();

            var locations = lines
                .SelectMany(GetIssueLocations)
                .WhereNotNull()
                .ToList();

            return MergeLocations(locations, preciseLocations);
        }

        internal IList<IIssueLocation> GetExpectedBuildErrors(IEnumerable<TextLine> lines)
        {
            return lines?.SelectMany(GetBuildErrorsLocations)
                .WhereNotNull()
                .ToList()
                .Cast<IIssueLocation>()
                .ToList()
                ?? Enumerable.Empty<IIssueLocation>().ToList();
        }

        public static bool IsNotIssueLocationLine(string line)
        {
            return !Regex.IsMatch(line, PRECISE_ISSUE_LOCATION_PATTERN);
        }

        internal /*for testing*/ IList<IIssueLocation> MergeLocations(IEnumerable<IssueLocation> locations, IEnumerable<IssueLocation> preciseLocations)
        {
            var usedLocations = new List<IssueLocation>();

            foreach (var location in locations)
            {
                var preciseLocationsOnSameLine = preciseLocations.Where(l => l.LineNumber == location.LineNumber);

                EnsureOnlyOneElement(preciseLocationsOnSameLine);

                var preciseLocation = preciseLocationsOnSameLine.SingleOrDefault();
                if (preciseLocation != null)
                {
                    if (location.Start != null)
                    {
                        throw new InvalidOperationException($"Unexpected redundant issue location on line {location.LineNumber}. Issue location can " +
                            $"be set either with 'precise issue location' or 'exact column location' pattern but not both.");
                    }
                    location.Start = preciseLocation.Start;
                    location.Length = preciseLocation.Length;
                    usedLocations.Add(preciseLocation);
                }
            }

            return locations
                .Union(preciseLocations.Except(usedLocations))
                .Cast<IIssueLocation>()
                .ToList();
        }

        internal /*for testing*/ IEnumerable<IssueLocation> GetIssueLocations(TextLine line)
        {
            return GetLocations(line, ISSUE_LOCATION_PATTERN);
        }

        internal /*for testing*/ IEnumerable<IssueLocation> GetBuildErrorsLocations(TextLine line)
        {
            return GetLocations(line, BUILD_ERROR_LOCATION_PATTERN);
        }

        private IEnumerable<IssueLocation> GetLocations(TextLine line, string pattern)
        {
            var lineText = line.ToString();

            var match = Regex.Match(lineText, pattern);
            if (match.Success)
            {
                EnsureNoRemainingCurlyBrace(line, match);
                return CreateIssueLocations(match, line.LineNumber + 1);
            }

            return Enumerable.Empty<IssueLocation>();
        }

        internal /*for testing*/ IEnumerable<IssueLocation> GetPreciseIssueLocations(TextLine line)
        {
            var lineText = line.ToString();

            var match = Regex.Match(lineText, PRECISE_ISSUE_LOCATION_PATTERN);
            if (match.Success)
            {
                EnsureNoRemainingCurlyBrace(line, match);
                return CreateIssueLocations(match, line.LineNumber);
            }
            return Enumerable.Empty<IssueLocation>();
        }

        private IEnumerable<IssueLocation> CreateIssueLocations(Match match, int lineNumber)
        {
            var line = lineNumber + GetOffset(match);
            var isPrimary = GetIsPrimary(match);
            var message = GetMessage(match);
            var start = GetStart(match) ?? GetColumnStart(match);
            var length = GetLength(match) ?? GetColumnLength(match);

            var position = match.Groups["position"];
            if (position.Success && position.Captures.Count > 1)
            {
                ThrowUnexpectedPreciseLocationCount(position.Captures.Count, line);
            }

            return GetIssueIds(match).Select(
                issueId => new IssueLocation
                {
                    IsPrimary = isPrimary,
                    LineNumber = line,
                    Message = message,
                    IssueId = issueId,
                    Start = start,
                    Length = length,
                });
        }

        private int? GetStart(Match match)
        {
            var position = match.Groups["position"];
            return position.Success ? (int?)position.Index : null;
        }

        private int? GetLength(Match match)
        {
            var position = match.Groups["position"];
            return position.Success ? (int?)position.Length : null;
        }

        private static int? GetColumnStart(Match match)
        {
            var columnStart = match.Groups["columnStart"];
            if (columnStart.Success)
            {
                return int.Parse(columnStart.Value) - 1;
            }
            return null;
        }

        private static int? GetColumnLength(Match match)
        {
            var length = match.Groups["length"];
            if (length.Success)
            {
                return int.Parse(length.Value);
            }
            return null;
        }

        private bool GetIsPrimary(Match match)
        {
            var issueType = match.Groups["issueType"];
            return !issueType.Success || issueType.Value == "Noncompliant";
        }

        private string GetMessage(Match match)
        {
            var message = match.Groups["message"];
            return message.Success ? message.Value : null;
        }

        private IEnumerable<string> GetIssueIds(Match match)
        {
            var issueIds = match.Groups["issueIds"];
            if (!issueIds.Success)
            {
                // We have a single issue without ID even if the group did not match
                return new string[] { null };
            }

            return issueIds.Value
                .Split(',')
                .Select(s => s.Trim())
                .Where(s => !string.IsNullOrEmpty(s))
                .OrderBy(s => s)
                .Select(s => s.Split('.').First());
        }

        private int GetOffset(Match match)
        {
            var sign = match.Groups["sign"];
            var offset = match.Groups["offset"];
            if (!sign.Success && !offset.Success)
            {
                return 0;
            }

            var offsetValue = int.Parse(offset.Value);

            return sign.Value == "-" ? -offsetValue : offsetValue;
        }

        private void EnsureOnlyOneElement(IEnumerable<IssueLocation> preciseLocationsOnSameLine)
        {
            if (preciseLocationsOnSameLine.Skip(1).Any())
            {
                ThrowUnexpectedPreciseLocationCount(preciseLocationsOnSameLine.Count(), preciseLocationsOnSameLine.First().LineNumber);
            }
        }

        private static void EnsureNoRemainingCurlyBrace(TextLine line, Match match)
        {
            var remainingLine = line.ToString().Substring(match.Index + match.Length);
            if (remainingLine.Contains("{"))
            {
                Execute.Assertion.FailWith("Unexpected '{{' found on line: {0}. Either correctly use the '{{{{message}}}}' " +
                    "format or remove the curly brace on the line of the expected issue", line.LineNumber);
            }
        }

        private static void ThrowUnexpectedPreciseLocationCount(int  count, int line)
        {
            var message = $"Expecting only one precise location per line, found {count} on line {line}. " +
                @"If you want to specify more than one precise location per line you need to omit the Noncompliant comment:
internal class MyClass : IInterface1 // there should be no Noncompliant comment
^^^^^^^ {{Do not create internal classes.}}
                         ^^^^^^^^^^^ @-1 {{IInterface1 IInterface1 is bad for your health.}}";
            throw new InvalidOperationException(message);
        }

        [DebuggerDisplay("ID:{IssueId} @{LineNumber} Primary:{IsPrimary} Start:{Start} Length:{Length} '{Message}'")]
        internal class IssueLocation : IIssueLocation
        {
            public bool IsPrimary { get; set; }

            public int LineNumber { get; set; }

            public string Message { get; set; }

            public string IssueId { get; set; }

            public int? Start { get; set; }

            public int? Length { get; set; }
        }
    }
}
