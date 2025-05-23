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

using System.IO;
using System.Text.RegularExpressions;
using FluentAssertions.Execution;
using Microsoft.CodeAnalysis.Text;

namespace SonarAnalyzer.TestFramework.Verification.IssueValidation;

/// <summary>
/// See <see href="https://github.com/SonarSource/sonar-dotnet/blob/master/docs/verifier-syntax.md">docs/verifier-syntax.md</see> for a comprehensive documentation of the verifier syntax.
/// </summary>
internal static class IssueLocationCollector
{
    private const string IssueTypeGroup = "IssueType";
    private const string CommentPattern = @"(?<Comment>//|'|<!--|/\*|@\*)";
    private const string PrecisePositionPattern = @"\s*(?<Position>\^+)(\s+(?<Invalid>\^+))*";
    private const string NoPrecisePositionPattern = @"(?<!\s*\^+\s)";
    private const string IssueTypePattern = @$"\s*(?<{IssueTypeGroup}>Noncompliant|Secondary|Error)";
    private const string OffsetPattern = @"(\s*@(?<Offset>[+-]?\d+))?";
    private const string ExactColumnPattern = @"(\s*\^(?<ColumnStart>\d+)#(?<Length>\d+))?";
    private const string IssueIdsPattern = @"(\s*\[(?<IssueIds>[^]]+)\])?";
    private const string MessagePattern = @"(\s*\{\{(?<Message>.+)\}\})?";

    public static readonly Regex RxIssue =
        CreateRegex(CommentPattern + NoPrecisePositionPattern + IssueTypePattern + OffsetPattern + ExactColumnPattern + IssueIdsPattern + MessagePattern);

    public static readonly Regex RxPreciseLocation =
        CreateRegex(@"^\s*" + CommentPattern + PrecisePositionPattern + IssueTypePattern + "?" + OffsetPattern + IssueIdsPattern + MessagePattern + @"\s*(-->|\*/|\*@)?$");

    private static readonly Regex RxInvalidType = CreateRegex(CommentPattern + ".*" + IssueTypePattern);
    private static readonly Regex RxInvalidPreciseLocation = CreateRegex(@"^\s*" + CommentPattern + ".*" + PrecisePositionPattern);

    public static IList<IssueLocation> ExpectedIssueLocations(string filePath, IEnumerable<TextLine> lines)
    {
        var preciseLocations = new List<IssueLocation>();
        var locations = new List<IssueLocation>();
        foreach (var line in lines)
        {
            var newPreciseLocations = FindPreciseIssueLocations(filePath, line).ToList();
            if (newPreciseLocations.Any())
            {
                preciseLocations.AddRange(newPreciseLocations);
            }
            else if (FindIssueLocations(filePath, line).ToList() is var newLocations && newLocations.Any())
            {
                locations.AddRange(newLocations);
            }
            else
            {
                EnsureNoInvalidFormat(filePath, line);
            }
        }
        return EnsureNoDuplicatedPrimaryIds(MergeLocations(locations.ToArray(), preciseLocations.ToList()));
    }

    internal static /* for testing */ IList<IssueLocation> MergeLocations(IssueLocation[] locations, List<IssueLocation> preciseLocations)
    {
        foreach (var location in locations)
        {
            var preciseLocationsOnSameLine = preciseLocations.Where(x => x.LineNumber == location.LineNumber).ToList();
            if (preciseLocationsOnSameLine.Count > 1)
            {
                throw UnexpectedPreciseLocationCount(preciseLocationsOnSameLine.Count, preciseLocationsOnSameLine[0].LineNumber);
            }

            if (preciseLocationsOnSameLine.SingleOrDefault() is { } preciseLocation)
            {
                if (location.Start.HasValue)
                {
                    throw new InvalidOperationException($"Unexpected redundant issue location on line {location.LineNumber}. Issue location can " +
                                                        "be set either with 'precise issue location' or 'exact column location' pattern but not both.");
                }

                location.Start = preciseLocation.Start;
                location.Length = preciseLocation.Length;
                preciseLocations.Remove(preciseLocation);
            }
        }
        return locations.Concat(preciseLocations).ToList();
    }

    internal static /* for testing */ IEnumerable<IssueLocation> FindIssueLocations(string filePath, TextLine line)
    {
        var match = RxIssue.Match(line.ToString());
        if (match.Success)
        {
            EnsureNoRemainingCurlyBrace(line, match);
            return CreateIssueLocations(match, filePath, line.LineNumber + 1);
        }
        return Enumerable.Empty<IssueLocation>();
    }

    internal static /* for testing */ IEnumerable<IssueLocation> FindPreciseIssueLocations(string filePath, TextLine line)
    {
        var match = RxPreciseLocation.Match(line.ToString());
        if (match.Success)
        {
            EnsureNoRemainingCurlyBrace(line, match);
            return CreateIssueLocations(match, filePath, line.LineNumber);
        }

        return Enumerable.Empty<IssueLocation>();
    }

    private static IEnumerable<IssueLocation> CreateIssueLocations(Match match, string filePath, int originalLineNumber)
    {
        var lineNumber = originalLineNumber + Offset();
        var type = Type();
        var message = Message();
        var start = Start() ?? ColumnStart();
        var length = Length() ?? ColumnLength();
        var invalid = match.Groups["Invalid"];
        return invalid.Success
            ? throw UnexpectedPreciseLocationCount(invalid.Captures.Count + 1, lineNumber)
            : IssueIds().Select(x => new IssueLocation(type, filePath, lineNumber, message, x, start, length));

        int? Start() =>
            Group("Position")?.Index;

        int? Length() =>
            Group("Position")?.Length;

        int? ColumnStart() =>
            Group("ColumnStart") is { } columnStart ? (int?)int.Parse(columnStart.Value) - 1 : null;

        int? ColumnLength() =>
            Group("Length") is { } length ? int.Parse(length.Value) : null;

        IssueType Type() =>
            match.Groups[IssueTypeGroup] switch
            {
                { Success: false } => IssueType.Primary,
                { Value: "Noncompliant" } => IssueType.Primary,
                { Value: "Secondary" } => IssueType.Secondary,
                { Value: "Error" } => IssueType.Error,
                _ => throw new  UnexpectedValueException(IssueTypeGroup, match.Groups[IssueTypeGroup].Value)
            };

        string Message() =>
            Group("Message")?.Value;

        int Offset() =>
            Group("Offset") is { } offset ? int.Parse(offset.Value) : 0;

        IEnumerable<string> IssueIds() =>
            Group("IssueIds") is { } issueIds
                ? issueIds.Value.Split(',').Select(x => x.Trim()).Where(x => !string.IsNullOrEmpty(x)).OrderBy(x => x)
                : new string[] { null }; // We have a single issue without ID even if the group did not match

        Group Group(string name) =>
            match.Groups[name] is { Success: true } group ? group : null;
    }

    private static void EnsureNoInvalidFormat(string filePath, TextLine line)
    {
        var value = line.ToString();
        var match = RxInvalidType.Match(value);
        if (match.Success)
        {
            var type = match.Groups[IssueTypeGroup].Value;
            throw new InvalidOperationException($"""
                {Path.GetFileName(filePath)} line {line.LineNumber} contains '// ... {type}' comment, but it is not recognized as one of the expected patterns.
                Either remove the '{type}' word or fix the pattern.
                """);
        }
        else if (RxInvalidPreciseLocation.IsMatch(value))
        {
            throw new InvalidOperationException($"""
                {Path.GetFileName(filePath)} line {line.LineNumber} looks like it contains comment for precise location '^^'.
                Either remove the precise pattern '^^' from the comment, or fix the pattern.
                """);
        }
    }

    private static IList<IssueLocation> EnsureNoDuplicatedPrimaryIds(IList<IssueLocation> mergedLocations)
    {
        var duplicateLocationsIds = mergedLocations
            .Where(x => x.Type == IssueType.Primary && x.IssueId != null)
            .GroupBy(x => x.IssueId)
            .FirstOrDefault(x => x.Count() > 1);
        if (duplicateLocationsIds is not null)
        {
            var duplicatedIdLines = duplicateLocationsIds.Select(issueLocation => issueLocation.LineNumber).JoinStr(", ");
            throw new InvalidOperationException($"Primary location with id [{duplicateLocationsIds.Key}] found on multiple lines: {duplicatedIdLines}");
        }
        return mergedLocations;
    }

    private static void EnsureNoRemainingCurlyBrace(TextLine line, Capture match)
    {
        var remainingLine = line.ToString().Substring(match.Index + match.Length);
        if (remainingLine.Contains('{') || remainingLine.Contains('}'))
        {
            Execute.Assertion.FailWith("""
                Unexpected '{{' or '}}' found on line: {0}. Either correctly use the '{{{{message}}}}' format or remove the curly braces on the line of the expected issue
                """, line.LineNumber);
        }
    }

    private static Exception UnexpectedPreciseLocationCount(int count, int line) =>
        new InvalidOperationException($$$"""
            Expecting only one precise location per line, found {{{count}}} on line {{{line}}}. If you want to specify more than one precise location per line you need to omit the Noncompliant comment:
            internal class MyClass : IInterface1 // there should be no Noncompliant comment
            ^^^^^^^^ {{Do not create internal classes.}}
                                     ^^^^^^^^^^^ @-1 {{IInterface1 is bad for your health.}}
            """);

    private static Regex CreateRegex(string pattern) =>
        new(pattern, RegexOptions.Compiled, RegexConstants.DefaultTimeout);
}
