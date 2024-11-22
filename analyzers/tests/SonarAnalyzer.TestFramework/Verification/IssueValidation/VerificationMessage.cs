/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2014-2024 SonarSource SA
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

namespace SonarAnalyzer.TestFramework.Verification.IssueValidation;

internal sealed record VerificationMessage(string ShortDescription, string FullDescription, string FilePath, int LineNumber)
{
    public static readonly VerificationMessage EmptyLine = new(null, null, null, 0);

    public string FullPath => Path.Combine(Paths.CurrentTestCases(), FilePath);

    public string ToInvocation()
    {
        // VS can process "at What Ever in C:\...".
        // Rider needs    "at What.Ever() in C:\..." to make it clickable.
        if (!ShortDescription.Contains(' '))
        {
            throw new InvalidOperationException("Short description must contain space for Rider to display clickable link."); // We'll change it to dot: "What Ever" -> "What.Ever()"
        }
        return ShortDescription.Replace(' ', '.') + "()";
    }
}
