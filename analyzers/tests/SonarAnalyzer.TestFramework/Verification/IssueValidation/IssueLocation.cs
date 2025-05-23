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

namespace SonarAnalyzer.TestFramework.Verification.IssueValidation;

internal enum IssueType
{
    // Order of these member is important for DiagnosticVerifier.
    Primary,
    Secondary,
    Error
}

[DebuggerDisplay("ID:{RuleId} {Type} @{LineNumber} Start:{Start} Length:{Length} ID:{IssueId} {Message} {FilePath}")]
internal sealed class IssueLocation
{
    public IssueLocation Primary { get; }   // Only for actual secondary issues
    public string RuleId { get; }           // Diagnostic ID for actual issues
    public string FilePath { get; }
    public int LineNumber { get; }
    public IssueType Type { get; }
    public string Message { get; }
    public string IssueId { get => Primary?.IssueId ?? issueId; }    // Issue location ID to pair primary and secondary locations
    public int? Start { get; set; }
    public int? Length { get; set; }

    private string issueId;

    public IssueLocation(Diagnostic diagnostic)
        : this(diagnostic.Severity == DiagnosticSeverity.Error ? IssueType.Error : IssueType.Primary, diagnostic.Id, diagnostic.GetMessage(), diagnostic.Location, diagnostic.Location.GetLineSpan()) =>
        issueId = diagnostic.Severity == DiagnosticSeverity.Error ? diagnostic.Id : null;   // Error [CS1001] should assert that expected issueId matches the actual RuleId

    public IssueLocation(IssueLocation primary, SecondaryLocation secondary) : this(IssueType.Secondary, primary.RuleId, secondary.Message, secondary.Location, secondary.Location.GetLineSpan()) =>
        Primary = primary;

    public IssueLocation(IssueType type, string filePath, int lineNumber, string message, string issueId, int? start, int? length, string ruleId = null)
    {
        Type = type;
        FilePath = filePath;
        LineNumber = lineNumber;
        Message = message;
        this.issueId = issueId;
        Start = start;
        Length = length;
        RuleId = ruleId;
    }

    private IssueLocation(IssueType type, string ruleId, string message, Location location, FileLinePositionSpan span)
        : this(type, span.Path ?? string.Empty, location.GetLineNumberToReport(), message ?? string.Empty, null, span.StartLinePosition.Character, location.SourceSpan.Length, ruleId) { }

    public void UpdatePrimaryIssueIdFrom(IssueLocation expected)
    {
        if (Type == IssueType.Primary)
        {
            issueId = expected.IssueId;  // Let actual secondary issues find issueId of their expected primary via secondaryIssue.Primary.IssueId
        }
    }

    public override int GetHashCode() =>
        Helpers.HashCode.Combine(FilePath.GetHashCode(), LineNumber, Type);

    public override bool Equals(object obj) =>
        obj is IssueLocation issue
        && (issue.FilePath == string.Empty || FilePath == string.Empty || issue.FilePath == FilePath)
        && issue.LineNumber == LineNumber
        && issue.Type == Type
        && (Type == IssueType.Primary || issue.IssueId == IssueId)   // We ignore issueId for primary issues, as we need to match them only for secondary
        && EqualOrNull(issue.RuleId, RuleId)
        && EqualOrNull(issue.Message, Message)
        && EqualOrNull(issue.Start, Start)
        && EqualOrNull(issue.Length, Length);

    private static bool EqualOrNull(string first, string second) =>
        first is null || second is null || first == second;

    private static bool EqualOrNull(int? first, int? second) =>
        first is null || second is null || first == second;
}

[DebuggerDisplay("{Type} @{LineNumber} {FilePath}")]
internal record IssueLocationKey(IssueType Type, string FilePath, int LineNumber)
{
    public IssueLocationKey(IssueLocation issue) : this(issue.Type, issue.FilePath, issue.LineNumber) { }

    public bool IsMatch(IssueLocation issue) =>
        (FilePath == string.Empty || FilePath == issue.FilePath)
        && issue.LineNumber == LineNumber
        && issue.Type == Type;
}
