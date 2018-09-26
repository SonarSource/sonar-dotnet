using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace ReviewDiffs
{
    class Program
    {
        static void Main(string[] args)
        {
            if (!Debugger.IsAttached)
            {
                Console.WriteLine("This program should only be run from the debugger (F5, not Ctrl-F5), since its goal is to display stuff in the output window.");
                Environment.Exit(-1);
            }
            var relative = "../../../../";
            var actual = Path.GetFullPath(relative + "actual");
            var expected = Path.GetFullPath(relative + "expected");
            var errorCount = 0;
            var diffCount = 0;
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
