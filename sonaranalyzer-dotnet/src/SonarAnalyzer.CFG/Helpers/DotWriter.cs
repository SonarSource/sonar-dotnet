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

using System.IO;
using System.Linq;

namespace SonarAnalyzer.Helpers
{
    internal class DotWriter
    {
        private readonly TextWriter writer;

        public DotWriter(TextWriter writer)
        {
            this.writer = writer;
        }

        public void WriteGraphStart(string graphName)
        {
            this.writer.WriteLine($"digraph \"{Encode(graphName)}\" {{");
        }

        public void WriteGraphEnd()
        {
            this.writer.WriteLine("}");
        }

        public void WriteNode(string id, string header, params string[] items)
        {
            // Curly braces in the label reverse the orientation of the columns/rows
            // Columns/rows are created with pipe
            // New lines are inserted with \n; \r\n does not work well.
            // ID [shape=record label="{<header>|<line1>\n<line2>\n...}"]
            this.writer.Write(id);
            this.writer.Write(" [shape=record label=\"{" + header);
            if (items.Length > 0)
            {
                this.writer.Write("|");
                this.writer.Write(string.Join("|", items.Select(Encode)));
            }
            this.writer.Write("}\"");
            this.writer.WriteLine("]");
        }

        internal void WriteEdge(string startId, string endId, string label)
        {
            this.writer.Write($"{startId} -> {endId}");
            if (!string.IsNullOrEmpty(label))
            {
                this.writer.Write($" [label=\"{label}\"]");
            }
            this.writer.WriteLine();
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
