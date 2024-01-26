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

using System.Text;

namespace SonarAnalyzer.TestFramework.Verification.IssueValidation;

internal sealed record IssueLocationPair(IssueLocation Actual, IssueLocation Expected)
{
    public string FilePath => Actual?.FilePath ?? Expected.FilePath;
    public int LineNumber => Actual?.LineNumber ?? Expected.LineNumber;
    public int? Start => Actual?.Start ?? Expected.Start;

    public void AppendAssertionMessage(StringBuilder builder)
    {
        if (Actual is not null && Expected is not null && new IssueLocationKey(Actual) != new IssueLocationKey(Expected))
        {
            throw new InvalidOperationException("Something went horribly wrong. This is supposed to be called only for issues with the same key.");
        }
        if (AssertionMessage() is { } message)
        {
            builder.Append("  Line ").Append(LineNumber);
            if (!Actual?.IsPrimary ?? !Expected.IsPrimary)
            {
                builder.Append(" Secondary location");
            }
            builder.Append(": ").Append(message);
            if (Actual?.RuleId is not null)
            {
                builder.Append(" Rule ").Append(Actual.RuleId);
            }
            if (Expected?.IssueId is not null)
            {
                builder.Append(" ID ").Append(Expected.IssueId);
            }
            builder.AppendLine();
        }
    }

    private string AssertionMessage()
    {
        // ToDo: Better template for compilation errors, so it can be copied
        if (Actual is null)
        {
            return Expected.Message is null
                ? "Missing expected issue"
                : $"Missing issue '{Expected.Message}'";
        }
        else if (Expected is null)
        {
            return $"Unexpected issue '{Actual.Message}'";
        }
        else if (Expected.Message is not null && Actual.Message != Expected.Message)
        {
            return $"The expected message '{Expected.Message}' does not match the actual message '{Actual.Message}'";
        }
        else if (Expected.Start.HasValue && Actual.Start != Expected.Start)
        {
            return $"Should start on column {Expected.Start} but got column {Actual.Start}";
        }
        else if (Expected.Length.HasValue && Actual.Length != Expected.Length)
        {
            return $"Should have a length of {Expected.Length} but got a length of {Actual.Length}";
        }
        else
        {
            throw new InvalidOperationException("Something went wrong. This is not supposed to be called for same issues.");
        }
    }
}
