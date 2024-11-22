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

using System.IO;

namespace SonarAnalyzer.TestFramework.Common;

public static class Paths
{
    public static string ProjectRoot { get; }
    public static string TestsRoot { get; }
    public static string AnalyzersRoot { get; }
    public static string Rspec { get; }

    static Paths()
    {
        // The AltCover tool has a limitation. It has to be invoked without a parameter for the project/solution path.
        // Due to this we have to call it from the analyzers folder and the working directory is different when running in CI context.
        TestsRoot = Path.Combine(FindRoot("tests"), "tests");
        Console.WriteLine("Test project root: " + TestsRoot);

        AnalyzersRoot = Path.GetDirectoryName(TestsRoot);
        Console.WriteLine("Analyzers root: " + AnalyzersRoot);

        ProjectRoot = FindRoot(".github");
        Console.WriteLine("Project root: " + ProjectRoot);

        Rspec = Path.Combine(ProjectRoot, "analyzers", "rspec");
        Console.WriteLine("Rspec folder " + Rspec);
    }

    /// <summary>
    /// Returns path to the TestCases folder from the currently executed test project.
    /// </summary>
    public static string CurrentTestCases()
    {
        // Under AltCover, this starts deeper than usually and we need to avoid the copied TestCases from TFM folder
        // C:\...\sonar-dotnet\analyzers\tests\SonarAnalyzer.TestFramework.Test\bin\Debug\net7.0-windows\__Instrumented_SonarAnalyzer.TestFramework.Test\
        var current = Path.GetDirectoryName(Path.GetDirectoryName(Path.GetFullPath(".")));
        while (current != TestsRoot)
        {
            var testCases = Path.Combine(current, "TestCases");
            if (Directory.Exists(testCases))
            {
                return testCases;
            }
            else
            {
                current = Path.GetDirectoryName(current);
            }
        }
        throw new InvalidOperationException("Could not find TestCases directory from current path: " + Path.GetFullPath("."));
    }

    private static string FindRoot(string expectedSubdirectory)
    {
        var current = Path.GetFullPath(".");
        var root = Path.GetPathRoot(current);
        while (current != root)
        {
            if (Directory.Exists(Path.Combine(current, expectedSubdirectory)))
            {
                return current;
            }
            else
            {
                current = Path.GetDirectoryName(current);
            }
        }
        throw new InvalidOperationException($"Could not find TestsRoot directory for '{expectedSubdirectory}' from current path: ${Path.GetFullPath(".")}");
    }
}
