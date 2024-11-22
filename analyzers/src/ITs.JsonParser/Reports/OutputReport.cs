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

using ITs.JsonParser.Json;

namespace ITs.JsonParser.Reports;

/// <summary>
/// Custom report extracted from the SARIF report.
/// </summary>
public record OutputReport(string Project, string RuleId, string Assembly, string Tfm, RuleIssues RuleIssues)
{
    public string Path(string root)
    {
        // projectName/rule-assembly{-TFM}?.json
        var suffix = Tfm is null ? string.Empty : $"-{Tfm}";
        return System.IO.Path.Combine(root, Project, $"{RuleId}-{Assembly}{suffix}.json");
    }
}
