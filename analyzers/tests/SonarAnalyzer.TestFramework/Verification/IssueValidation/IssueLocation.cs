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

[DebuggerDisplay("ID:{IssueId} @{LineNumber} Primary:{IsPrimary} Start:{Start} Length:{Length} '{Message}'")]
internal class IssueLocation : IIssueLocation
{
    public string FilePath { get; init; }
    public bool IsPrimary { get; init; }
    public int LineNumber { get; init; }
    public string Message { get; init; }
    public string IssueId { get; init; }
    public int? Start { get; set; }
    public int? Length { get; set; }

    public IssueLocation(Diagnostic diagnostic) : this(diagnostic.GetMessage(), diagnostic.Location)
    {
        IsPrimary = true;
        IssueId = diagnostic.Id;
    }

    public IssueLocation(SecondaryLocation secondaryLocation) : this(secondaryLocation.Message, secondaryLocation.Location) { }

    public IssueLocation() { }

    private IssueLocation(string message, Location location)
    {
        Message = message;
        LineNumber = location.GetLineNumberToReport();
        Start = location.GetLineSpan().StartLinePosition.Character;
        Length = location.SourceSpan.Length;
        FilePath = location.SourceTree?.FilePath;
    }
}
