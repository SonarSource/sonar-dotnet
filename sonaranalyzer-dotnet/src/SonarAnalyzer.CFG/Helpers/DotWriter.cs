/*
 * Copyright (C) 2018-2019 SonarSource SA
 * All rights reserved
 * mailto:info AT sonarsource DOT com
 */

using System.IO;
using System.Linq;

namespace SonarAnalyzer.Helpers
{
    internal class DotWriter
    {
        private readonly TextWriter _writer;

        public DotWriter(TextWriter writer)
        {
            _writer = writer;
        }

        public void WriteGraphStart(string graphName)
        {
            _writer.WriteLine($"digraph \"{Encode(graphName)}\" {{");
        }

        public void WriteGraphEnd()
        {
            _writer.WriteLine("}");
        }

        public void WriteNode(string id, string header, params string[] items)
        {
            // Curly braces in the label reverse the orientation of the columns/rows
            // Columns/rows are created with pipe
            // New lines are inserted with \n; \r\n does not work well.
            // ID [shape=record label="{<header>|<line1>\n<line2>\n...}"]
            _writer.Write(id);
            _writer.Write(" [shape=record label=\"{" + header);
            if (items.Length > 0)
            {
                _writer.Write("|");
                _writer.Write(string.Join("|", items.Select(Encode)));
            }
            _writer.Write("}\"");
            _writer.WriteLine("]");
        }

        internal void WriteEdge(string startId, string endId, string label)
        {
            _writer.Write($"{startId} -> {endId}");
            if (!string.IsNullOrEmpty(label))
            {
                _writer.Write($" [label=\"{label}\"]");
            }
            _writer.WriteLine();
        }

        private static string Encode(string s) =>
            s.Replace("\r", string.Empty)
            .Replace("\n", "\\n")
            .Replace("{", "\\{")
            .Replace("}", "\\}")
            .Replace("|", "\\|")
            .Replace("<", "\\<")
            .Replace(">", "\\>")
            .Replace("\"", "\\\"");
    }
}
