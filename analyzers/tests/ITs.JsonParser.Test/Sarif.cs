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

using SonarAnalyzer.Core.Extensions;

namespace ITs.JsonParser.Test;

public static class Sarif
{
    public static string CreateReport(string rootPath, string solution, string project, string tfm, params string[] issues)
    {
        var content = $$"""
            {
              "runs": [
                {
                  "results": [
                    {{issues.JoinStr($",{Environment.NewLine}")}}
                  ]
                }
              ]
            }
            """;
        var suffix = tfm is null ? string.Empty : $"-{tfm}";
        File.WriteAllText(Path.Combine(rootPath, solution, "issues", $"{project}{suffix}.json"), content);
        return content;
    }

    public static string CreateIssue(string ruleId, string message, string relativePath, int startLine, int endLine) =>
        $$"""
        {
          "ruleId": "{{ruleId}}",
          "message": "{{message}}",
          "locations": [
            {
              "resultFile": {
                "uri": "file:///some/local/path/analyzers/its/{{relativePath}}",
                "region": {
                  "startLine": {{startLine}},
                  "startColumn": 42,
                  "endLine": {{endLine}},
                  "endColumn": 99
                }
              }
            }
          ]
        }
        """;

    public static string CreateIssue(string ruleId, string message) =>
        $$"""
        {
          "ruleId": "{{ruleId}}",
          "message": "{{message}}",
          "locations": [ ]
        }
        """;
}
