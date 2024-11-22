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

using SonarAnalyzer.TestFramework.Common;
using SonarAnalyzer.TestFramework.Extensions;

namespace ITs.JsonParser.Test;

[TestClass]
public class IssueParserTest
{
    public TestContext TestContext { get; set; }

    [TestMethod]
    public void Execute_Single()
    {
        var root = TestDirectory();
        var inputPath = Path.Combine(root, "read");
        var outputPath = Path.Combine(root, "write");
        Directory.CreateDirectory(Path.Combine(inputPath, "solution", "issues"));
        Sarif.CreateReport(inputPath, "solution", "project", "net6.0", Sarif.CreateIssue("S100", "Message_1", "foo/bar/File1.cs", 1, 1));
        var outFile = Path.Combine(outputPath, "solution", "S100-project-net6.0.json");

        IssueParser.Execute(inputPath, outputPath);

        VerifyResultFile(outFile, """
            {
              "Issues": [
                {
                  "Id": "S100",
                  "Message": "Message_1",
                  "Uri": "https://github.com/SonarSource/sonar-dotnet-enterprise/blob/master/private/analyzers/its/foo/bar/File1.cs#L1",
                  "Location": "Line 1 Position 42-99"
                }
              ]
            }
            """);
    }

    [TestMethod]
    public void Execute_Multiple()
    {
        var root = TestDirectory();
        var inputPath = Path.Combine(root, "read");
        var outputPath = Path.Combine(root, "write");
        Directory.CreateDirectory(Path.Combine(inputPath, "solution1", "issues"));
        Directory.CreateDirectory(Path.Combine(inputPath, "solution2", "issues"));

        // solution1/project1-net6.0.json
        Sarif.CreateReport(
            inputPath,
            "solution1",
            "project1",
            "net6.0",
            Sarif.CreateIssue("S100", "Message_1", "foo/bar/File1.cs", 1, 1),
            Sarif.CreateIssue("S100", "Message_2", "foo/bar/File1.cs", 42, 43),  // #L1-L2 location range
            Sarif.CreateIssue("S142", "Message_3"));                             // null location

        // solution1/project2-net6.0.json
        Sarif.CreateReport(
            inputPath,
            "solution1",
            "project2",
            null, // No target framework specified
            Sarif.CreateIssue("S200", "Message_1", "foo/bar/File1.cs", 0, 0));

        // solution2/project-net6.0.json
        Sarif.CreateReport(
            inputPath,
            "solution2",
            "project",
            "net6.0",
            Sarif.CreateIssue("S100", "Message_1", "foo/bar/File1.cs", 1, 1));

        var outFile1 = Path.Combine(outputPath, "solution1", "S100-project1-net6.0.json");
        var outFile2 = Path.Combine(outputPath, "solution1", "S142-project1-net6.0.json");
        var outFile3 = Path.Combine(outputPath, "solution1", "S200-project2.json");
        var outFile4 = Path.Combine(outputPath, "solution2", "S100-project-net6.0.json");

        IssueParser.Execute(inputPath, outputPath);

        VerifyResultFile(outFile1, """
            {
              "Issues": [
                {
                  "Id": "S100",
                  "Message": "Message_1",
                  "Uri": "https://github.com/SonarSource/sonar-dotnet-enterprise/blob/master/private/analyzers/its/foo/bar/File1.cs#L1",
                  "Location": "Line 1 Position 42-99"
                },
                {
                  "Id": "S100",
                  "Message": "Message_2",
                  "Uri": "https://github.com/SonarSource/sonar-dotnet-enterprise/blob/master/private/analyzers/its/foo/bar/File1.cs#L42-L43",
                  "Location": "Lines 42-43 Position 42-99"
                }
              ]
            }
            """);

        VerifyResultFile(outFile2, """
            {
              "Issues": [
                {
                  "Id": "S142",
                  "Message": "Message_3",
                  "Uri": null,
                  "Location": null
                }
              ]
            }
            """);

        VerifyResultFile(outFile3, """
            {
              "Issues": [
                {
                  "Id": "S200",
                  "Message": "Message_1",
                  "Uri": "https://github.com/SonarSource/sonar-dotnet-enterprise/blob/master/private/analyzers/its/foo/bar/File1.cs#L0",
                  "Location": "Line 0 Position 42-99"
                }
              ]
            }
            """);

        VerifyResultFile(outFile4, """
            {
              "Issues": [
                {
                  "Id": "S100",
                  "Message": "Message_1",
                  "Uri": "https://github.com/SonarSource/sonar-dotnet-enterprise/blob/master/private/analyzers/its/foo/bar/File1.cs#L1",
                  "Location": "Line 1 Position 42-99"
                }
              ]
            }
            """);
    }

    private string TestDirectory() =>
        Path.GetDirectoryName(TestHelper.TestPath(TestContext, "unused"));

    private static void VerifyResultFile(string path, string expected)
    {
        File.Exists(path).Should().BeTrue();
        var result = File.ReadAllText(path);
        Console.WriteLine($"Path: {path}");
        Console.WriteLine(result);
        result.Should().BeIgnoringLineEndings(expected);
    }
}
