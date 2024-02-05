using SonarAnalyzer.Helpers;

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
        File.WriteAllText(Path.Combine(rootPath, solution, $"{project}{suffix}.json"), content);
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
