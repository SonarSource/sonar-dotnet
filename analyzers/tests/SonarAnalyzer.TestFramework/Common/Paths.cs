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

using System.IO;

namespace SonarAnalyzer.TestFramework.Common;

public static class Paths
{
    public static string TestsRoot { get; }
    public static string AnalyzersRoot { get; }
    public static string Rspec { get; }

    static Paths()
    {
        // The AltCover tool has a limitation. It has to be invoked without a parameter for the project/solution path.
        // Due to this we have to call it from the analyzers folder and the working directory is different when running in CI context.
        TestsRoot = FindTestsRoot();
        Console.WriteLine("Test project root: " + TestsRoot);

        AnalyzersRoot = Path.GetDirectoryName(TestsRoot);
        Console.WriteLine("Analyzers root: " + AnalyzersRoot);

        Rspec = Path.Combine(AnalyzersRoot, "rspec");
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

    private static string FindTestsRoot()
    {
        var current = Path.GetFullPath(".");
        var root = Path.GetPathRoot(current);
        while (current != root)
        {
            if (Directory.Exists(Path.Combine(current, "SonarAnalyzer.Test")))
            {
                return current;
            }
            else
            {
                current = Path.GetDirectoryName(current);
            }
        }
        throw new InvalidOperationException("Could not find TestProjectRoot directory from current path: " + Path.GetFullPath("."));
    }
}
