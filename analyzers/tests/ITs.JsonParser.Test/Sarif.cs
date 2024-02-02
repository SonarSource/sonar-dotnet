namespace ITs.JsonParser.Test;

internal static class Sarif
{
    internal static string CreateReport(string rootPath, string solution, string project, string tfm, params string[] issues)
    {
        var content = $$"""
            {
              "runs": [
                {
                  "results": [
                    {{string.Join($",{Environment.NewLine}", issues)}}
                  ]
                }
              ]
            }
            """;
        var suffix = tfm is null ? string.Empty : $"-{tfm}";
        File.WriteAllText(Path.Combine(rootPath, solution, $"{project}{suffix}.json"), content);
        return content;
    }

    internal static string CreateIssue(string ruleId, string message, string relativePath, int startLine, int endLine) =>
        $$"""
        {
          "ruleId": "{{ruleId}}",
          "message": "{{message}}.",
          "locations": [
            {
              "resultFile": {
                "uri": "file:///some/local/path/analyzers/its/{{relativePath}}",
                "region": {
                  "startLine": {{startLine}},
                  "startColumn": 42,
                  "endLine": {{endLine}},
                  "endColumn": 69
                }
              }
            }
          ]
        }
        """;

    internal static string CreateIssue(string ruleId, string message) =>
        $$"""
        {
          "ruleId": "{{ruleId}}",
          "message": "{{message}}.",
          "locations": [ ]
        }
        """;
}
