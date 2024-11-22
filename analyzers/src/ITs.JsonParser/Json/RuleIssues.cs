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
