/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2014-2025 SonarSource SA
 * mailto:info AT sonarsource DOT com
 * This program is free software; you can redistribute it and/or
 * modify it under the terms of the Sonar Source-Available License Version 1, as published by SonarSource SA.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
 * See the Sonar Source-Available License for more details.
 *
 * You should have received a copy of the Sonar Source-Available License
 * along with this program; if not, see https://sonarsource.com/license/ssal/
 */

using System.IO;
using System.Text;
using SonarAnalyzer.TestFramework.Verification.IssueValidation;

namespace SonarAnalyzer.TestFramework.Verification;

public sealed class DiagnosticVerifierException : AssertFailedException
{
    private readonly string stackTrace;

    public override string StackTrace => stackTrace;

    internal DiagnosticVerifierException(List<VerificationMessage> messages) : base(messages.Select(x => x.FullDescription).JoinStr(Environment.NewLine))
    {
        if (messages.Count == 0)
        {
            throw new ArgumentException($"{nameof(messages)} cannot be empty", nameof(messages));
        }
        var builder = new StringBuilder();
        foreach (var fileMessages in messages.Where(x => x.ShortDescription is not null && File.Exists(x.FullPath)).GroupBy(x => x.FullPath).OrderBy(x => x.Key))
        {
            builder.AppendLine(Path.GetFileName(fileMessages.Key));
            foreach (var message in fileMessages)
            {
                builder.AppendLine($"at {message.ToInvocation()} in {message.FullPath}:line {message.LineNumber}");
            }
            builder.AppendLine("---");    // Empty lines are not rendered => force it with content
        }
        stackTrace = builder.ToString();
    }
}
