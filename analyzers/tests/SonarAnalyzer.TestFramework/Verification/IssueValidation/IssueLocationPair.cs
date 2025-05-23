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

using System.Text;

namespace SonarAnalyzer.TestFramework.Verification.IssueValidation;

internal sealed record IssueLocationPair(IssueLocation Actual, IssueLocation Expected)
{
    public string FilePath => Actual?.FilePath ?? Expected.FilePath;
    public int LineNumber => Actual?.LineNumber ?? Expected.LineNumber;
    public IssueType Type => Actual?.Type ?? Expected.Type;
    public int? Start => Actual?.Start ?? Expected.Start;
    public string IssueId => Actual?.IssueId ?? Expected?.IssueId;
    public string RuleId => Actual?.RuleId ?? Expected.RuleId;

    public VerificationMessage CreateMessage()
    {
        if (Actual is not null && Expected is not null && !new IssueLocationKey(Actual).IsMatch(Expected))
        {
            throw new InvalidOperationException("Something went horribly wrong. This is supposed to be called only for issues with the same key.");
        }
        var builder = new StringBuilder();
        var (concise, detailed) = AssertionMessage();
        builder.Append("  Line ").Append(LineNumber);
        if ((Actual?.Type ?? Expected.Type) == IssueType.Secondary)
        {
            builder.Append(" Secondary location");
        }
        builder.Append(": ").Append(detailed);
        if (Type != IssueType.Error)
        {
            if (Actual?.RuleId is not null)
            {
                builder.Append(" Rule ").Append(Actual.RuleId);
            }
            if ((Actual?.IssueId ?? Expected?.IssueId) is { } issueId)
            {
                builder.Append(" ID ").Append(issueId);
            }
        }
        return new($"{Type} {concise}", builder.ToString(), FilePath, LineNumber);
    }

    private (string Concise, string Detailed) AssertionMessage()
    {
        if (Actual is null)
        {
            return ("Missing", MissingMessage());
        }
        else if (Expected is null)
        {
            return ("Unexpected", UnexpectedMessage());
        }
        else if (Expected.IssueId != Actual.IssueId)
        {
            return ("Different ID", $"The expected issueId '{Expected.IssueId}' does not match the actual issueId '{Actual.IssueId}'");
        }
        else if (Expected.Message is not null && Actual.Message != Expected.Message)
        {
            return ("Different Message", $"The expected message '{Expected.Message}' does not match the actual message '{Actual.Message}'");
        }
        else if (Expected.Start.HasValue && Actual.Start != Expected.Start)
        {
            return ("Different Location", $"Should start on column {Expected.Start} but got column {Actual.Start}");
        }
        else if (Expected.Length.HasValue && Actual.Length != Expected.Length)
        {
            return ("Different Length", $"Should have a length of {Expected.Length} but got a length of {Actual.Length}");
        }
        else
        {
            throw new InvalidOperationException("Something went wrong. This is not supposed to be called for same issues.");
        }
    }

    private string MissingMessage() =>
        Expected.Message is null
            ? "Missing expected issue"
            : $"Missing expected issue '{Expected.Message}'";

    private string UnexpectedMessage()
    {
        var comment = FilePath.EndsWith(".vb", StringComparison.InvariantCultureIgnoreCase) ? "'" : "//";
        return Actual.Type == IssueType.Error
            ? $"Unexpected error, use {comment} Error [{Actual.RuleId}] {Actual.Message}"   // We don't want to assert the precise {{Message}}
            : $"Unexpected issue '{Actual.Message}'";
    }
}
