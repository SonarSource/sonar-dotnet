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
