namespace ITs.JsonParser.Test;

internal static class Sarif
{
    internal static string CreateReport(string rootPath, string project, string subProject, string assembly, params string[] issues)
    {
        var contents = $"""
        {Prefix}
        {string.Join($",{Environment.NewLine}", issues)}
        {Suffix}
        """;
        File.WriteAllText(Path.Combine(rootPath, project, $"{subProject}-{assembly}.json"), contents);
        return contents;
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

    internal static string CreateIssue(string ruleId, string message, string relativePath) =>
        $$"""
        {
          "ruleId": "{{ruleId}}",
          "message": "{{message}}.",
          "locations": [ ]
        }
        """;

    private const string Prefix = """
        {
          "runs": [
            {
              "results": [
        """;

    private const string Suffix = """
              ]
            }
          ]
        }
        """;
}
