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

namespace SonarAnalyzer.TestFramework.Verification.IssueValidation;

[DebuggerDisplay("ID:{RuleId} @{LineNumber} Primary:{IsPrimary} Start:{Start} Length:{Length} {Message} {FilePath}")]
internal sealed class IssueLocation // ToDo: Refactor the relation between this and the Key
{
    public string RuleId { get; init; }     // Diagnostic ID for actual issues
    public string FilePath { get; init; }
    public int LineNumber { get; init; }
    public bool IsPrimary { get; init; }
    public string Message { get; init; }
    public string IssueId { get; init; }    // Issue location ID to pair primary and secondary locations
    public int? Start { get; set; }
    public int? Length { get; set; }

    public IssueLocation(Diagnostic diagnostic) : this(diagnostic.GetMessage(), diagnostic.Location)
    {
        IsPrimary = true;
        RuleId = diagnostic.Id;
    }

    public IssueLocation(SecondaryLocation secondaryLocation) : this(secondaryLocation.Message ?? string.Empty, secondaryLocation.Location) { }

    public IssueLocation() { }

    private IssueLocation(string message, Location location)
    {
        var span = location.GetLineSpan();
        Message = message;
        LineNumber = location.GetLineNumberToReport();
        Start = span.StartLinePosition.Character;
        Length = location.SourceSpan.Length;
        FilePath = span.Path ?? string.Empty;   // Project-level issues do not have location
    }

    public override int GetHashCode() =>
        Helpers.HashCode.Combine(FilePath.GetHashCode(), LineNumber, IsPrimary ? 0 : 1);

    public override bool Equals(object obj) =>
        obj is IssueLocation issue
        && (issue.FilePath == string.Empty || FilePath == string.Empty || issue.FilePath == FilePath)
        && issue.LineNumber == LineNumber
        && issue.IsPrimary == IsPrimary
        && (issue.RuleId is null || RuleId is null || issue.RuleId == RuleId)
        && (issue.Message is null || Message is null || issue.Message == Message)
        && (issue.IssueId is null || IssueId is null || issue.IssueId == IssueId)   // For consistency, Roslyn issues will not have IDs
        && (issue.Start is null || Start is null || issue.Start == Start)
        && (issue.Length is null || Length is null || issue.Length == Length);
}

[DebuggerDisplay("@{LineNumber} Primary:{IsPrimary} {FilePath}")]
internal record IssueLocationKey(string FilePath, int LineNumber, bool IsPrimary)
{
    public IssueLocationKey(IssueLocation issue) : this(issue.FilePath, issue.LineNumber, issue.IsPrimary) { }

    public bool IsMatch(IssueLocation issue) =>
        (FilePath == string.Empty || FilePath == issue.FilePath)
        && issue.LineNumber == LineNumber
        && issue.IsPrimary == IsPrimary;
}
