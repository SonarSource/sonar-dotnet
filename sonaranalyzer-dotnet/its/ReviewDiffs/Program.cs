/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2019 SonarSource SA
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

using System;
using System.Diagnostics;
using System.IO;
using Newtonsoft.Json.Linq;

namespace ReviewDiffs
{
    static class Program
    {
        static void Main(string[] args)
        {
            if (!Debugger.IsAttached)
            {
                Console.WriteLine("This program should only be run from the debugger (F5, not Ctrl-F5), since its goal is to display stuff in the output window.");
                Environment.Exit(-1);
            }

            const string relative = "../../../../";
            var actual = Path.GetFullPath(relative + "actual");
            var expected = Path.GetFullPath(relative + "expected");
            Debug.WriteLine($"Path for 'actual': {actual}");
            Debug.WriteLine($"Path for 'expected': {expected}");
            if (!Directory.Exists(actual))
            {
                Debug.WriteLine($"Error: 'actual' folder does not exist. This program should be run after integration tests.");
                Environment.Exit(-2);
            }
            if(!Directory.Exists(expected))
            {
                Debug.WriteLine($"Error: 'expected' folder does not exist.");
                Environment.Exit(-2);
            }

            var errorCount = 0;
            var diffCount = 0;
            foreach(var file in Directory.EnumerateFiles(actual, "*.json", SearchOption.AllDirectories))
            {
                var relativePath = Path.GetRelativePath(actual, file);
                var matchingExpected = Path.Combine(expected, relativePath);
                if (File.Exists(matchingExpected))
                {
                    var contentActual = File.ReadAllText(file);
                    var contentExpected = File.ReadAllText(matchingExpected);
                    if (contentActual != contentExpected)
                    {
                        Debug.WriteLine($"Error: File {relativePath} differs in 'expected' and 'actual', case not implemented.");
                        errorCount++;
                    }
                    continue;
                }

                var rawContent = File.ReadAllText(file).Replace("\\", "\\\\"); // The files are not in correct JSon format
                var o = JObject.Parse(rawContent);
                foreach(var i in o["issues"].Children())
                {
                    var loc = i["location"];
                    var region = loc["region"];
                    var fullPath = Path.GetFullPath(Path.Combine(relative, ((string)loc["uri"]).Replace("%20", " ")));
                    diffCount++;
                    Debug.WriteLine($"{fullPath}({(string)region["startLine"]}, {(string)region["startColumn"]}, {(string)region["endLine"]},{(string)region["endColumn"]}): Warning {diffCount,5}: {(string)i["message"]}");
                }
            }

            Debug.WriteLine($"{diffCount} differences were found.");
            Debug.WriteLineIf(errorCount != 0, $"{errorCount} errors were encountered.");
        }
    }
}
