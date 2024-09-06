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

using SonarAnalyzer.Extensions;

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
