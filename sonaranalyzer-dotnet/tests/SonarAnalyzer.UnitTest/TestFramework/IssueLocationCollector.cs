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
    /// <summary>
    /// This class will look for specific patterns inside the unit test files being analyzed when testing a rule.
    /// Here's a summary and examples of the different patterns that can be used to mark part of the code as noncompliant.
    /// These patterns must appear after a single line comment ("//" or "'" token).
    ///
    /// Simple 'Noncompliant' comment. Will mark the current line as expecting the primary location of an issue.
    /// <code>
    ///     private void MyMethod() // Noncompliant
    /// </code>
    ///
    /// 'Secondary' location comment. Must be used together with a main primary location to mark the expected line of a
    /// secondary location.
    /// <code>
    ///     if (myCondition) // Noncompliant
    ///     {
    ///       var a = null; // Secondary
    ///     }
    /// </code>
    ///
    /// Using offsets. Using @[+-][0-9]+ after a 'Noncompliant' or 'Secondary' comment will mark the expected location to be
    /// offset by the given number of lines.
    /// <code>
    ///  private void MyMethod() // Noncompliant@+2 - issue is actually expected 2 lines after this comment
    /// </code>
    ///
    /// Checking the issue message. The message raised by the issue can be checked using the {{expected message}} pattern.
    /// <code>
    ///     private void MyMethod() // Noncompliant {{Remove this unused private method}}
    /// </code>
    ///
    /// Checking the precise/exact location of an issue. Only one precise location or column location can be present
    /// at one time. Precise location is used by adding '^^^^' comment under the location where the issue is expected.
    /// The alternative column location pattern can be used by following the 'Noncompliant' or 'Secondary' comment
    /// with '^X#Y' where 'X' is the expected start column and Y the length of the issue.
    /// <code>
    ///     private void MyMethod() // Noncompliant
    /// //  ^^^^^^^
    /// </code>
    /// <code>
    ///     private void MyMethod() // Noncompliant ^4#7
    /// </code>
    /// 
    /// Multiple issues per line. To declare that multiple issues are expected, each issue must be assigned an id. All
    /// secondary locations associated with an issue must have the same id. Note that it is not possible to have multiple
    /// precise/column locations on a single line.
    /// <code>
    ///     var a = null; // Noncompliant [myId2]
    ///     if (myCondition) // Noncompliant [myId1, myId3]
    ///     {
    ///       a = null; // Secondary [myId1, myId2]
    ///     }
    /// </code>
    ///
    /// Note that most of the previous patterns can be used together when needed.
    /// <code>
    ///     private void MyMethod() // Noncompliant@+1 ^4#7 [MyIssueId] {{Remove this unused private method}}
    /// </code>
    /// <code>
    /// </summary>
    public class IssueLocationCollector
    {
        private const string CommentPattern = "(?<comment>//|')";
        private const string PrecisePositionPattern = @"\s*(?<position>\^+)(\s+(?<invalid>\^+))*\s*";
        private const string NoPrecisePositionPattern = @"(?<!\s*\^+\s{1})";
        private const string IssueTypePattern = @"\s*(?<issueType>Noncompliant|Secondary)";
        private const string ErrorTypePattern = @"\s*Error";
        private const string OffsetPattern = @"(\s*@(?<offset>[+-]?\d+))?";
        private const string ExactColumnPattern = @"(\s*\^(?<columnStart>\d+)#(?<length>\d+))?";
        private const string IssueIdsPattern = @"(\s*\[(?<issueIds>.+)\])?";
        private const string MessagePattern = @"(\s*\{\{(?<message>.+)\}\})?";

        private static readonly Regex RxPreciseLocation =
            new Regex(@"^\s*" + CommentPattern + PrecisePositionPattern + IssueTypePattern + "?" + OffsetPattern + IssueIdsPattern + MessagePattern + "$", RegexOptions.Compiled);
        private static readonly Regex RxBuildError =
            new Regex(CommentPattern + NoPrecisePositionPattern + ErrorTypePattern + OffsetPattern + ExactColumnPattern + IssueIdsPattern, RegexOptions.Compiled);
        internal static readonly Regex RxIssue =
                   new Regex(CommentPattern + NoPrecisePositionPattern + IssueTypePattern + OffsetPattern + ExactColumnPattern + IssueIdsPattern + MessagePattern, RegexOptions.Compiled);

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

            var mergedLocations = MergeLocations(locations, preciseLocations);

            EnsureNoDuplicatedPrimaryIds(mergedLocations);

            return mergedLocations;
        }

        public static bool IsNotIssueLocationLine(string line)
        {
            return !RxPreciseLocation.IsMatch(line);
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
                    if (location.Start.HasValue)
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
            return GetLocations(line, RxIssue);
        }

        internal /*for testing*/ IEnumerable<IssueLocation> GetBuildErrorsLocations(TextLine line)
        {
            return GetLocations(line, RxBuildError);
        }

        internal /*for testing*/ IEnumerable<IssueLocation> GetPreciseIssueLocations(TextLine line)
        {
            var lineText = line.ToString();

            var match = RxPreciseLocation.Match(lineText);
            if (match.Success)
            {
                EnsureNoRemainingCurlyBrace(line, match);
                return CreateIssueLocations(match, line.LineNumber);
            }
            return Enumerable.Empty<IssueLocation>();
        }

        private IEnumerable<IssueLocation> GetLocations(TextLine line, Regex rx)
        {
            var lineText = line.ToString();

            var match = rx.Match(lineText);
            if (match.Success)
            {
                EnsureNoRemainingCurlyBrace(line, match);
                return CreateIssueLocations(match, line.LineNumber + 1);
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

            var invalid = match.Groups["invalid"];
            if (invalid.Success)
            {
                ThrowUnexpectedPreciseLocationCount(invalid.Captures.Count + 1, line);
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
            return columnStart.Success ? (int?)int.Parse(columnStart.Value) - 1 : null;
        }

        private static int? GetColumnLength(Match match)
        {
            var length = match.Groups["length"];
            return length.Success ? (int?)int.Parse(length.Value) : null;
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
            var offset = match.Groups["offset"];
            return offset.Success ? int.Parse(offset.Value) : 0;
        }

        private static void EnsureNoDuplicatedPrimaryIds(IList<IIssueLocation> mergedLocations)
        {
            var duplicateLocationsIds = mergedLocations
                            .Where(location => location.IsPrimary)
                            .GroupBy(x => x.IssueId)
                            .Where(x => x.Key != null)
                            .FirstOrDefault(group => group.Count() > 1);
            if (duplicateLocationsIds != null)
            {
                var duplicatedIdLines = duplicateLocationsIds.Select(issueLocation => issueLocation.LineNumber).JoinStr(",");
                throw new InvalidOperationException($"Primary location with id [{duplicateLocationsIds.Key}] found on multiple lines: {duplicatedIdLines}");
            }
        }

        private static void EnsureOnlyOneElement(IEnumerable<IssueLocation> preciseLocationsOnSameLine)
        {
            if (preciseLocationsOnSameLine.Skip(1).Any())
            {
                ThrowUnexpectedPreciseLocationCount(preciseLocationsOnSameLine.Count(), preciseLocationsOnSameLine.First().LineNumber);
            }
        }

        private static void EnsureNoRemainingCurlyBrace(TextLine line, Match match)
        {
            var remainingLine = line.ToString().Substring(match.Index + match.Length);
            if (remainingLine.Contains("{") || remainingLine.Contains("}"))
            {
                Execute.Assertion.FailWith("Unexpected '{{' or '}}' found on line: {0}. Either correctly use the '{{{{message}}}}' " +
                    "format or remove the curly braces on the line of the expected issue", line.LineNumber);
            }
        }

        private static void ThrowUnexpectedPreciseLocationCount(int  count, int line)
        {
            var message = $"Expecting only one precise location per line, found {count} on line {line}. " +
                @"If you want to specify more than one precise location per line you need to omit the Noncompliant comment:
internal class MyClass : IInterface1 // there should be no Noncompliant comment
^^^^^^^ {{Do not create internal classes.}}
                         ^^^^^^^^^^^ @-1 {{IInterface1 is bad for your health.}}";
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
