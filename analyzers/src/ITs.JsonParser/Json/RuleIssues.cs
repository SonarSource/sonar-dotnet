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

namespace ITs.JsonParser.Json;

// Format of JSONs in "actual/" and "expected/"
public class RuleIssues
{
    public RuleIssue[] Issues { get; set; }
}

public class RuleIssue
{
    public string Id { get; set; }
    public string Message { get; set; }
    public string Uri { get; set; }
    public string Location { get; set; }

    public RuleIssue(SarifIssue issue)
    {
        Id = issue.RuleId;
        Message = issue.Message;
        Uri = issue.NormalizedUri();
        if (issue.Location?.Region is { } region)
        {
            Location = region.StartLine == region.EndLine
                ? $"Line {region.StartLine} Position {region.StartColumn}-{region.EndColumn}"
                : $"Lines {region.StartLine}-{region.EndLine} Position {region.StartColumn}-{region.EndColumn}";
        }
    }
}
